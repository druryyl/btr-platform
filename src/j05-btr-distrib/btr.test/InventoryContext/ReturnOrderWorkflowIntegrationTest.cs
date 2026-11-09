using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.InventoryContext
{
    /// <summary>
    /// Integration test verifying the full lifecycle transitions between
    /// ReturnOrder, ReturJual creation, ReturnOrderStatusWorker evaluations,
    /// and ListReturnOrderMonitoringHandler queries.
    /// </summary>
    public class ReturnOrderWorkflowIntegrationTest
    {
        private class InMemoryReturnOrderDal : IReturnOrderDal
        {
            public Dictionary<string, ReturnOrderModel> Storage { get; } = new Dictionary<string, ReturnOrderModel>();

            public void Insert(ReturnOrderModel model) => Storage[model.ReturnOrderId] = model;
            public void Update(ReturnOrderModel model) => Storage[model.ReturnOrderId] = model;
            public ReturnOrderModel GetData(IReturnOrderKey key) => key != null && Storage.TryGetValue(key.ReturnOrderId, out var model) ? model : null;
            public ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key) => GetData(key);
            public IEnumerable<ReturnOrderModel> ListData() => Storage.Values;
            public IEnumerable<ReturnOrderModel> ListData(Periode filter) => Storage.Values.Where(x => x.ReturnOrderDate >= filter.Tgl1 && x.ReturnOrderDate <= filter.Tgl2);
            public IEnumerable<ReturnOrderModel> ListByStatus(string status) => Storage.Values.Where(x => x.Status == status);
            public bool Exists(IReturnOrderKey key) => key != null && Storage.ContainsKey(key.ReturnOrderId);
        }

        private class InMemoryReturnOrderItemDal : IReturnOrderItemDal
        {
            public List<ReturnOrderItemModel> Items { get; } = new List<ReturnOrderItemModel>();

            public void Insert(IEnumerable<ReturnOrderItemModel> listModel) => Items.AddRange(listModel);
            public void Delete(IReturnOrderKey key) => Items.RemoveAll(x => x.ReturnOrderId == key.ReturnOrderId);
            public IEnumerable<ReturnOrderItemModel> ListData(IReturnOrderKey filter) => Items.Where(x => x.ReturnOrderId == filter.ReturnOrderId);
        }

        private class InMemoryReturJualDal : IReturJualDal
        {
            public List<ReturJualModel> Storage { get; } = new List<ReturJualModel>();

            public void Insert(ReturJualModel model) => Storage.Add(model);
            public void Update(ReturJualModel model)
            {
                var idx = Storage.FindIndex(x => x.ReturJualId == model.ReturJualId);
                if (idx >= 0) Storage[idx] = model;
            }
            public void Delete(IReturJualKey key) => Storage.RemoveAll(x => x.ReturJualId == key.ReturJualId);
            public ReturJualModel GetData(IReturJualKey key) => Storage.FirstOrDefault(x => x.ReturJualId == key.ReturJualId);
            public IEnumerable<ReturJualModel> ListData(Periode filter) => Storage;
            public IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId) => Storage.Where(x => x.ReturnOrderId == returnOrderId);
        }

        private class InMemoryWarehouseDal : IWarehouseDal
        {
            public List<WarehouseModel> Warehouses { get; } = new List<WarehouseModel>();

            public void Insert(WarehouseModel model) => Warehouses.Add(model);
            public void Update(WarehouseModel model) {}
            public void Delete(IWarehouseKey key) {}
            public WarehouseModel GetData(IWarehouseKey key) => Warehouses.FirstOrDefault(x => x.WarehouseId == key.WarehouseId);
            public IEnumerable<WarehouseModel> ListData() => Warehouses;
            public IEnumerable<WarehouseModel> ListAllForPortal() => Warehouses;
        }

        [Fact]
        public async Task HeterogeneousReturnOrder_FullLifecycle_TransitionsAndMonitoringQueriesAreAccurate()
        {
            // SETUP
            var orderDal = new InMemoryReturnOrderDal();
            var itemDal = new InMemoryReturnOrderItemDal();
            var rjDal = new InMemoryReturJualDal();
            var whDal = new InMemoryWarehouseDal();

            whDal.Warehouses.Add(new WarehouseModel { WarehouseId = "WH-CENTRAL", WarehouseName = "Gudang Distribusi Pusat" });

            var worker = new ReturnOrderStatusWorker(orderDal, itemDal, rjDal);
            var queryHandler = new ListReturnOrderMonitoringHandler(orderDal, itemDal, rjDal, whDal);

            var roId = "RO-2026-HETERO";
            var roDate = new DateTime(2026, 11, 8);

            // STAGE 1: Return Order Created (Heterogeneous: BAGUS & RUSAK)
            var order = new ReturnOrderModel(roId)
            {
                ReturnOrderNo = "RONO-8888",
                ReturnOrderDate = roDate,
                CustomerId = "CUST-007",
                CustomerName = "Toko Berkah Abadi",
                WarehouseCode = "WH-CENTRAL",
                Status = ReturnOrderStatus.Synced,
                Note = "Retur campur kondisi bagus dan cacat pabrik"
            };
            orderDal.Insert(order);

            itemDal.Insert(new[]
            {
                new ReturnOrderItemModel { ReturnOrderId = roId, NoUrut = 1, BrgId = "BRG-01", Qty = 10, JenisRetur = "BAGUS" },
                new ReturnOrderItemModel { ReturnOrderId = roId, NoUrut = 2, BrgId = "BRG-02", Qty = 5, JenisRetur = "RUSAK" }
            });

            // Initial monitoring check
            var monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-15"), CancellationToken.None)).ToList();
            monitorResult.Should().HaveCount(1);
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Synced);
            monitorResult[0].WarehouseName.Should().Be("Gudang Distribusi Pusat");
            monitorResult[0].TotalItem.Should().Be(2);
            monitorResult[0].ReturJualBagusCode.Should().BeEmpty();
            monitorResult[0].ReturJualRusakCode.Should().BeEmpty();

            // STAGE 2: Realize first Retur Jual (BAGUS)
            var rjBagus = new ReturJualModel("RJ-ID-001")
            {
                ReturJualCode = "RJ-2026-BAGUS-01",
                ReturnOrderId = roId,
                JenisRetur = "BAGUS",
                SalesPersonId = "SP-001",
                SalesPersonName = "Salesman A",
                DriverId = "DR-001",
                DriverName = "Driver A",
                VoidDate = new DateTime(3000, 1, 1)
            };
            rjDal.Insert(rjBagus);

            var statusAfterBagus = worker.UpdateStatus(roId);
            statusAfterBagus.Should().Be(ReturnOrderStatus.Partial);

            var updatedRoAfterBagus = orderDal.GetData(new ReturnOrderModel(roId));
            updatedRoAfterBagus.Status.Should().Be(ReturnOrderStatus.Partial);
            updatedRoAfterBagus.SalesPersonId.Should().Be("SP-001");
            updatedRoAfterBagus.DriverId.Should().Be("DR-001");

            monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-15"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Partial);
            monitorResult[0].ReturJualBagusCode.Should().Be("RJ-2026-BAGUS-01");
            monitorResult[0].ReturJualRusakCode.Should().BeEmpty();

            // STAGE 3: Realize second Retur Jual (RUSAK)
            var rjRusak = new ReturJualModel("RJ-ID-002")
            {
                ReturJualCode = "RJ-2026-RUSAK-01",
                ReturnOrderId = roId,
                JenisRetur = "RUSAK",
                SalesPersonId = "SP-001",
                SalesPersonName = "Salesman A",
                DriverId = "DR-001",
                DriverName = "Driver A",
                VoidDate = new DateTime(3000, 1, 1)
            };
            rjDal.Insert(rjRusak);

            var statusAfterRusak = worker.UpdateStatus(roId);
            statusAfterRusak.Should().Be(ReturnOrderStatus.Imported);

            monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-15"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Imported);
            monitorResult[0].ReturJualBagusCode.Should().Be("RJ-2026-BAGUS-01");
            monitorResult[0].ReturJualRusakCode.Should().Be("RJ-2026-RUSAK-01");

            // STAGE 4: Void RUSAK Retur Jual -> should revert to Partial
            rjRusak.VoidDate = DateTime.Today;
            rjDal.Update(rjRusak);

            var statusAfterVoidRusak = worker.UpdateStatus(roId);
            statusAfterVoidRusak.Should().Be(ReturnOrderStatus.Partial);

            monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-15"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Partial);
            monitorResult[0].ReturJualBagusCode.Should().Be("RJ-2026-BAGUS-01");
            monitorResult[0].ReturJualRusakCode.Should().BeEmpty();

            // STAGE 5: Void BAGUS Retur Jual -> should revert to Synced
            rjBagus.VoidDate = DateTime.Today;
            rjDal.Update(rjBagus);

            var statusAfterVoidBagus = worker.UpdateStatus(roId);
            statusAfterVoidBagus.Should().Be(ReturnOrderStatus.Synced);

            monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-15"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Synced);
            monitorResult[0].ReturJualBagusCode.Should().BeEmpty();
            monitorResult[0].ReturJualRusakCode.Should().BeEmpty();
        }

        [Fact]
        public async Task HomogeneousReturnOrder_FullLifecycle_TransitionsDirectlyToImportedAndRevertsToSynced()
        {
            var orderDal = new InMemoryReturnOrderDal();
            var itemDal = new InMemoryReturnOrderItemDal();
            var rjDal = new InMemoryReturJualDal();
            var whDal = new InMemoryWarehouseDal();

            var worker = new ReturnOrderStatusWorker(orderDal, itemDal, rjDal);
            var queryHandler = new ListReturnOrderMonitoringHandler(orderDal, itemDal, rjDal, whDal);

            var roId = "RO-HOMOGEN-01";
            var order = new ReturnOrderModel(roId)
            {
                ReturnOrderNo = "RONO-HOMOGEN-01",
                ReturnOrderDate = new DateTime(2026, 11, 7),
                CustomerId = "CUST-001",
                Status = ReturnOrderStatus.Synced
            };
            orderDal.Insert(order);

            itemDal.Insert(new[]
            {
                new ReturnOrderItemModel { ReturnOrderId = roId, NoUrut = 1, BrgId = "BRG-A", Qty = 5, JenisRetur = "BAGUS" },
                new ReturnOrderItemModel { ReturnOrderId = roId, NoUrut = 2, BrgId = "BRG-B", Qty = 3, JenisRetur = "BAGUS" }
            });

            // Step 1: Realize Retur Jual
            var rj = new ReturJualModel("RJ-HOMOGEN-01")
            {
                ReturJualCode = "RJ-2026-HOMO",
                ReturnOrderId = roId,
                JenisRetur = "BAGUS",
                VoidDate = new DateTime(3000, 1, 1)
            };
            rjDal.Insert(rj);

            var status = worker.UpdateStatus(roId);
            status.Should().Be(ReturnOrderStatus.Imported);

            var monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-10"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Imported);
            monitorResult[0].ReturJualBagusCode.Should().Be("RJ-2026-HOMO");

            // Step 2: Void Retur Jual
            rj.VoidDate = DateTime.Today;
            rjDal.Update(rj);

            var revertedStatus = worker.UpdateStatus(roId);
            revertedStatus.Should().Be(ReturnOrderStatus.Synced);

            monitorResult = (await queryHandler.Handle(new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-10"), CancellationToken.None)).ToList();
            monitorResult[0].Status.Should().Be(ReturnOrderStatus.Synced);
            monitorResult[0].ReturJualBagusCode.Should().BeEmpty();
        }
    }
}
