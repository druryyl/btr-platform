using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg.Workers;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.InventoryContext
{
    public class ReturnOrderStatusWorkerTest
    {
        private class FakeReturnOrderDal : IReturnOrderDal
        {
            public ReturnOrderModel Model { get; set; }
            public ReturnOrderModel UpdatedModel { get; set; }

            public void Insert(ReturnOrderModel model) => Model = model;
            public void Update(ReturnOrderModel model) => UpdatedModel = model;
            public ReturnOrderModel GetData(IReturnOrderKey key) => Model?.ReturnOrderId == key?.ReturnOrderId ? Model : null;
            public ReturnOrderModel GetByReturnOrderId(IReturnOrderKey key) => GetData(key);
            public IEnumerable<ReturnOrderModel> ListData() => Model != null ? new[] { Model } : Enumerable.Empty<ReturnOrderModel>();
            public IEnumerable<ReturnOrderModel> ListData(Periode filter) => Model != null ? new[] { Model } : Enumerable.Empty<ReturnOrderModel>();
            public IEnumerable<ReturnOrderModel> ListByStatus(string status) => Model?.Status == status ? new[] { Model } : Enumerable.Empty<ReturnOrderModel>();
            public bool Exists(IReturnOrderKey key) => Model?.ReturnOrderId == key?.ReturnOrderId;
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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateStatus_WithNullOrEmptyId_ReturnsEmpty(string returnOrderId)
        {
            var fakeOrderDal = new FakeReturnOrderDal();
            var fakeItemDal = new FakeReturnOrderItemDal();
            var fakeRjDal = new FakeReturJualDal();
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus(returnOrderId);

            result.Should().BeEmpty();
            fakeOrderDal.UpdatedModel.Should().BeNull();
        }

        [Fact]
        public void UpdateStatus_WhenOrderNotFound_ReturnsEmpty()
        {
            var fakeOrderDal = new FakeReturnOrderDal { Model = null };
            var fakeItemDal = new FakeReturnOrderItemDal();
            var fakeRjDal = new FakeReturJualDal();
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().BeEmpty();
            fakeOrderDal.UpdatedModel.Should().BeNull();
        }

        [Fact]
        public void UpdateStatus_WhenNoActiveReturJual_SetsStatusSynced()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Partial }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal();
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Synced);
            fakeOrderDal.UpdatedModel.Should().NotBeNull();
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Synced);
        }

        [Fact]
        public void UpdateStatus_WhenHomogenOrderAndReturJualMatches_SetsStatusImported()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Synced }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Imported);
        }

        [Fact]
        public void UpdateStatus_WhenHeterogenOrderAndOnlyOneJenisReturMatches_SetsStatusPartial()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Synced }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" },
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "RUSAK" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Partial);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Partial);
        }

        [Fact]
        public void UpdateStatus_WhenHeterogenOrderAndBothJenisReturMatch_SetsStatusImported()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Partial }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" },
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "RUSAK" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1)
                    },
                    new ReturJualModel("RJ002")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "RUSAK",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Imported);
        }

        [Fact]
        public void UpdateStatus_WhenAllReturJualVoided_RevertsToSynced()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Imported }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = DateTime.Today // voided
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Synced);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Synced);
        }

        [Fact]
        public void UpdateStatus_WhenOneOfTwoReturJualVoided_RevertsToPartial()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Imported }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" },
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "RUSAK" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1) // active
                    },
                    new ReturJualModel("RJ002")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "RUSAK",
                        VoidDate = DateTime.Today // voided
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Partial);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Partial);
        }

        [Fact]
        public void UpdateStatus_WhenSalesOrDriverEmptyOnOrder_SyncsFromReturJual()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001")
                {
                    Status = ReturnOrderStatus.Synced,
                    SalesPersonId = "",
                    SalesPersonName = "",
                    DriverId = "",
                    DriverName = ""
                }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        SalesPersonId = "SP01",
                        SalesPersonName = "Budi Sales",
                        DriverId = "DR01",
                        DriverName = "Joko Driver",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.SalesPersonId.Should().Be("SP01");
            fakeOrderDal.UpdatedModel.SalesPersonName.Should().Be("Budi Sales");
            fakeOrderDal.UpdatedModel.DriverId.Should().Be("DR01");
            fakeOrderDal.UpdatedModel.DriverName.Should().Be("Joko Driver");
        }

        [Fact]
        public void UpdateStatus_WhenSalesOrDriverAlreadyPopulated_DoesNotOverwrite()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001")
                {
                    Status = ReturnOrderStatus.Synced,
                    SalesPersonId = "SP_ORIGINAL",
                    SalesPersonName = "Original Sales",
                    DriverId = "DR_ORIGINAL",
                    DriverName = "Original Driver"
                }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        SalesPersonId = "SP_NEW",
                        SalesPersonName = "New Sales",
                        DriverId = "DR_NEW",
                        DriverName = "New Driver",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.SalesPersonId.Should().Be("SP_ORIGINAL");
            fakeOrderDal.UpdatedModel.SalesPersonName.Should().Be("Original Sales");
            fakeOrderDal.UpdatedModel.DriverId.Should().Be("DR_ORIGINAL");
            fakeOrderDal.UpdatedModel.DriverName.Should().Be("Original Driver");
        }

        [Fact]
        public void Execute_ViaINunaService_CallsUpdateStatusSuccessfully()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Synced }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "BAGUS" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.Execute("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Imported);
        }

        [Fact]
        public void UpdateStatus_CaseInsensitiveJenisRetur_MatchesCorrectly()
        {
            var fakeOrderDal = new FakeReturnOrderDal
            {
                Model = new ReturnOrderModel("RO001") { Status = ReturnOrderStatus.Synced }
            };
            var fakeItemDal = new FakeReturnOrderItemDal
            {
                Items = new List<ReturnOrderItemModel>
                {
                    new ReturnOrderItemModel { ReturnOrderId = "RO001", JenisRetur = "bagus" }
                }
            };
            var fakeRjDal = new FakeReturJualDal
            {
                ReturJuals = new List<ReturJualModel>
                {
                    new ReturJualModel("RJ001")
                    {
                        ReturnOrderId = "RO001",
                        JenisRetur = "BAGUS",
                        VoidDate = new DateTime(3000, 1, 1)
                    }
                }
            };
            var worker = new ReturnOrderStatusWorker(fakeOrderDal, fakeItemDal, fakeRjDal);

            var result = worker.UpdateStatus("RO001");

            result.Should().Be(ReturnOrderStatus.Imported);
            fakeOrderDal.UpdatedModel.Status.Should().Be(ReturnOrderStatus.Imported);
        }
    }
}

