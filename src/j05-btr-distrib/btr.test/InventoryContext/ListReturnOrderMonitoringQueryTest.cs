using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.WarehouseAgg;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.InventoryContext
{
    public class ListReturnOrderMonitoringQueryTest
    {
        private class FakeReturnOrderDal : IReturnOrderDal
        {
            public List<ReturnOrderModel> Orders { get; set; } = new List<ReturnOrderModel>();

            public void Insert(ReturnOrderModel model) => Orders.Add(model);
            public void Update(ReturnOrderModel model) {}
            public ReturnOrderModel GetData(IReturnOrderKey key) => Orders.FirstOrDefault(x => x.ReturnOrderId == key.ReturnOrderId);
            public ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key) => GetData(key);
            public IEnumerable<ReturnOrderModel> ListData() => Orders;
            public IEnumerable<ReturnOrderModel> ListData(Periode filter) => Orders.Where(x => x.ReturnOrderDate >= filter.Tgl1 && x.ReturnOrderDate <= filter.Tgl2);
            public IEnumerable<ReturnOrderModel> ListByStatus(string status) => Orders.Where(x => x.Status == status);
            public bool Exists(IReturnOrderKey key) => Orders.Any(x => x.ReturnOrderId == key.ReturnOrderId);
        }

        private class FakeReturnOrderItemDal : IReturnOrderItemDal
        {
            public List<ReturnOrderItemModel> Items { get; set; } = new List<ReturnOrderItemModel>();

            public void Insert(IEnumerable<ReturnOrderItemModel> listModel) => Items.AddRange(listModel);
            public void Delete(IReturnOrderKey key) => Items.RemoveAll(x => x.ReturnOrderId == key.ReturnOrderId);
            public IEnumerable<ReturnOrderItemModel> ListData(IReturnOrderKey filter) => Items.Where(x => x.ReturnOrderId == filter.ReturnOrderId);
        }

        private class FakeReturJualDal : IReturJualDal
        {
            public List<ReturJualModel> ReturJuals { get; set; } = new List<ReturJualModel>();

            public void Insert(ReturJualModel model) => ReturJuals.Add(model);
            public void Update(ReturJualModel model) {}
            public void Delete(IReturJualKey key) {}
            public ReturJualModel GetData(IReturJualKey key) => ReturJuals.FirstOrDefault(x => x.ReturJualId == key.ReturJualId);
            public IEnumerable<ReturJualModel> ListData(Periode filter) => ReturJuals;
            public IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId) => ReturJuals.Where(x => x.ReturnOrderId == returnOrderId);
        }

        private class FakeWarehouseDal : IWarehouseDal
        {
            public List<WarehouseModel> Warehouses { get; set; } = new List<WarehouseModel>();

            public void Insert(WarehouseModel model) => Warehouses.Add(model);
            public void Update(WarehouseModel model) {}
            public void Delete(IWarehouseKey key) {}
            public WarehouseModel GetData(IWarehouseKey key) => Warehouses.FirstOrDefault(x => x.WarehouseId == key.WarehouseId);
            public IEnumerable<WarehouseModel> ListData() => Warehouses;
            public IEnumerable<WarehouseModel> ListAllForPortal() => Warehouses;
        }

        [Fact]
        public async Task Handle_WithNullRequest_ThrowsArgumentNullException()
        {
            var handler = new ListReturnOrderMonitoringHandler(
                new FakeReturnOrderDal(),
                new FakeReturnOrderItemDal(),
                new FakeReturJualDal(),
                new FakeWarehouseDal());

            Func<Task> act = async () => await handler.Handle(null, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData("invalid-date", "2026-11-09")]
        [InlineData("2026-11-01", "not-a-date")]
        [InlineData("", "2026-11-09")]
        public async Task Handle_WithInvalidDateFormat_ThrowsArgumentException(string tgl1, string tgl2)
        {
            var handler = new ListReturnOrderMonitoringHandler(
                new FakeReturnOrderDal(),
                new FakeReturnOrderItemDal(),
                new FakeReturJualDal(),
                new FakeWarehouseDal());

            var query = new ListReturnOrderMonitoringQuery(tgl1, tgl2);
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Handle_WhenStartDateGreaterThanEndDate_ThrowsArgumentException()
        {
            var handler = new ListReturnOrderMonitoringHandler(
                new FakeReturnOrderDal(),
                new FakeReturnOrderItemDal(),
                new FakeReturJualDal(),
                new FakeWarehouseDal());

            var query = new ListReturnOrderMonitoringQuery("2026-11-10", "2026-11-01");
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Tanggal awal tidak boleh lebih besar dari tanggal akhir*");
        }

        [Fact]
        public async Task Handle_WhenRangeExceeds31Days_ThrowsArgumentException()
        {
            var handler = new ListReturnOrderMonitoringHandler(
                new FakeReturnOrderDal(),
                new FakeReturnOrderItemDal(),
                new FakeReturJualDal(),
                new FakeWarehouseDal());

            var query = new ListReturnOrderMonitoringQuery("2026-10-01", "2026-11-05"); // 35 days
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Rentang tanggal maksimal 31 hari*");
        }

        [Fact]
        public async Task Handle_ValidRequest_MapsReturnOrderAndAssociatedReturJualCorrectly()
        {
            var orderDal = new FakeReturnOrderDal();
            var itemDal = new FakeReturnOrderItemDal();
            var rjDal = new FakeReturJualDal();
            var whDal = new FakeWarehouseDal();

            whDal.Warehouses.Add(new WarehouseModel { WarehouseId = "WH-01", WarehouseName = "Gudang Utama" });

            var roDate = new DateTime(2026, 11, 5, 10, 0, 0);
            var ro1 = new ReturnOrderModel
            {
                ReturnOrderId = "RO-001",
                ReturnOrderNo = "RONO-001",
                ReturnOrderDate = roDate,
                CustomerId = "CUST-01",
                CustomerName = "Toko Sukses",
                WarehouseCode = "WH-01",
                SalesPersonId = "SP-01",
                SalesPersonName = "Budi Sales",
                DriverId = "DR-01",
                DriverName = "Anto Supir",
                Status = "Partial",
                Note = "Catatan retur"
            };
            orderDal.Orders.Add(ro1);

            // Detail items (3 items)
            itemDal.Items.Add(new ReturnOrderItemModel { ReturnOrderId = "RO-001", NoUrut = 1, BrgId = "B1", Qty = 2, JenisRetur = "BAGUS" });
            itemDal.Items.Add(new ReturnOrderItemModel { ReturnOrderId = "RO-001", NoUrut = 2, BrgId = "B2", Qty = 5, JenisRetur = "BAGUS" });
            itemDal.Items.Add(new ReturnOrderItemModel { ReturnOrderId = "RO-001", NoUrut = 3, BrgId = "B3", Qty = 1, JenisRetur = "RUSAK" });

            // Retur Jual aktif BAGUS
            rjDal.ReturJuals.Add(new ReturJualModel
            {
                ReturJualId = "RJ-001",
                ReturJualCode = "RJ-BAGUS-001",
                ReturnOrderId = "RO-001",
                JenisRetur = "BAGUS",
                VoidDate = new DateTime(3000, 1, 1)
            });

            // Retur Jual RUSAK tapi VOID (tidak boleh muncul)
            rjDal.ReturJuals.Add(new ReturJualModel
            {
                ReturJualId = "RJ-002",
                ReturJualCode = "RJ-RUSAK-VOID",
                ReturnOrderId = "RO-001",
                JenisRetur = "RUSAK",
                VoidDate = new DateTime(2026, 11, 6)
            });

            var handler = new ListReturnOrderMonitoringHandler(orderDal, itemDal, rjDal, whDal);
            var query = new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-10");

            var result = (await handler.Handle(query, CancellationToken.None)).ToList();

            result.Should().HaveCount(1);
            var dto = result[0];
            dto.ReturnOrderId.Should().Be("RO-001");
            dto.ReturnOrderNo.Should().Be("RONO-001");
            dto.ReturnOrderDate.Should().Be(roDate);
            dto.CustomerId.Should().Be("CUST-01");
            dto.CustomerName.Should().Be("Toko Sukses");
            dto.WarehouseCode.Should().Be("WH-01");
            dto.WarehouseName.Should().Be("Gudang Utama");
            dto.SalesPersonId.Should().Be("SP-01");
            dto.SalesPersonName.Should().Be("Budi Sales");
            dto.DriverId.Should().Be("DR-01");
            dto.DriverName.Should().Be("Anto Supir");
            dto.Status.Should().Be("Partial");
            dto.TotalItem.Should().Be(3);
            dto.ReturJualBagusCode.Should().Be("RJ-BAGUS-001");
            dto.ReturJualRusakCode.Should().Be(string.Empty); // voided is excluded
            dto.Note.Should().Be("Catatan retur");
        }

        [Fact]
        public async Task Handle_BothBagusAndRusakActive_MapsBothCodesCorrectly()
        {
            var orderDal = new FakeReturnOrderDal();
            var itemDal = new FakeReturnOrderItemDal();
            var rjDal = new FakeReturJualDal();
            var whDal = new FakeWarehouseDal();

            var roDate = new DateTime(2026, 11, 2);
            var ro = new ReturnOrderModel
            {
                ReturnOrderId = "RO-002",
                ReturnOrderNo = "RONO-002",
                ReturnOrderDate = roDate,
                WarehouseCode = "WH-02",
                Status = "Imported"
            };
            orderDal.Orders.Add(ro);

            whDal.Warehouses.Add(new WarehouseModel { WarehouseId = "WH-02", WarehouseName = "Gudang Cabang" });

            itemDal.Items.Add(new ReturnOrderItemModel { ReturnOrderId = "RO-002", NoUrut = 1, BrgId = "B1", Qty = 2, JenisRetur = "BAGUS" });
            itemDal.Items.Add(new ReturnOrderItemModel { ReturnOrderId = "RO-002", NoUrut = 2, BrgId = "B2", Qty = 3, JenisRetur = "RUSAK" });

            rjDal.ReturJuals.Add(new ReturJualModel
            {
                ReturJualId = "RJ-10",
                ReturJualCode = "RJ-BAGUS-10",
                ReturnOrderId = "RO-002",
                JenisRetur = "BAGUS",
                VoidDate = new DateTime(3000, 1, 1)
            });
            rjDal.ReturJuals.Add(new ReturJualModel
            {
                ReturJualId = "RJ-11",
                ReturJualCode = "RJ-RUSAK-11",
                ReturnOrderId = "RO-002",
                JenisRetur = "RUSAK",
                VoidDate = new DateTime(3000, 1, 1)
            });

            var handler = new ListReturnOrderMonitoringHandler(orderDal, itemDal, rjDal, whDal);
            var query = new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-05");

            var result = (await handler.Handle(query, CancellationToken.None)).ToList();

            result.Should().HaveCount(1);
            result[0].TotalItem.Should().Be(2);
            result[0].ReturJualBagusCode.Should().Be("RJ-BAGUS-10");
            result[0].ReturJualRusakCode.Should().Be("RJ-RUSAK-11");
            result[0].WarehouseName.Should().Be("Gudang Cabang");
        }

        [Fact]
        public async Task Handle_WithExact31DaysRange_SucceedsWithoutException()
        {
            var handler = new ListReturnOrderMonitoringHandler(
                new FakeReturnOrderDal(),
                new FakeReturnOrderItemDal(),
                new FakeReturJualDal(),
                new FakeWarehouseDal());

            // 2026-10-01 to 2026-11-01 is exactly 31 days
            var query = new ListReturnOrderMonitoringQuery("2026-10-01", "2026-11-01");
            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WhenOrderHasExplicitWarehouseName_PreservesWarehouseName()
        {
            var orderDal = new FakeReturnOrderDal();
            var itemDal = new FakeReturnOrderItemDal();
            var rjDal = new FakeReturJualDal();
            var whDal = new FakeWarehouseDal();

            var ro = new ReturnOrderModel
            {
                ReturnOrderId = "RO-003",
                ReturnOrderNo = "RONO-003",
                ReturnOrderDate = new DateTime(2026, 11, 2),
                WarehouseCode = "WH-03",
                WarehouseName = "Gudang Khusus Pre-set",
                Status = "Synced"
            };
            orderDal.Orders.Add(ro);

            var handler = new ListReturnOrderMonitoringHandler(orderDal, itemDal, rjDal, whDal);
            var query = new ListReturnOrderMonitoringQuery("2026-11-01", "2026-11-05");

            var result = (await handler.Handle(query, CancellationToken.None)).ToList();

            result.Should().HaveCount(1);
            result[0].WarehouseName.Should().Be("Gudang Khusus Pre-set");
            result[0].ReturJualBagusCode.Should().BeEmpty();
            result[0].ReturJualRusakCode.Should().BeEmpty();
        }
    }
}

