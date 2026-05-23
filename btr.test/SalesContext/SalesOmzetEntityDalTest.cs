using System;
using btr.application.SalesContext.SalesOmzetAgg.Workers;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.SalesContext.SalesOmzetAgg;
using btr.nuna.Application;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetEntityDalTest
    {
        private readonly SalesOmzetEntityDal _dal;
        private readonly SalesOmzetWriter _writer;

        public SalesOmzetEntityDalTest()
        {
            var databaseOptions = new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            };
            var options = Options.Create(databaseOptions);
            _dal = new SalesOmzetEntityDal(options);
            _writer = new SalesOmzetWriter(_dal, new NunaCounterBL(new ParamNoDal(options)));
        }

        private static SalesOmzetModel Faker(string orderId)
        {
            var now = DateTime.Today;
            var sentinel = new DateTime(3000, 1, 1);
            return new SalesOmzetModel
            {
                OrderId = orderId,
                FakturId = string.Empty,
                FakturCode = string.Empty,
                SaleKind = SaleKindEnum.OrderedSale,
                SalesDate = now,
                OmzetDate = sentinel,
                SalesPersonName = "Test Sales",
                OrderDate = now,
                OrderTotal = 100m,
                FakturDate = sentinel,
                FakturTotal = 0m,
                CustomerName = "Test Customer",
                Code = "TC01",
                Alamat = "Test Address",
                OmzetStatus = SalesOmzetStatusEnum.Outstanding,
                CreatedAt = now,
                LastReconciledAt = now
            };
        }

        [Fact]
        public void UT1_WriterInsertGetUpdateByOrderId()
        {
            var orderId = "TEST-SO-" + Guid.NewGuid().ToString("N").Substring(0, 12);
            using (var trans = TransHelper.NewScope())
            {
                var model = Faker(orderId);
                _writer.Save(ref model);
                model.SalesOmzetId.Should().NotBeNullOrEmpty();
                model.SalesOmzetId.Should().StartWith("SO");

                var byId = _dal.GetData(model);
                byId.Should().BeEquivalentTo(model);

                var byOrder = _dal.GetByOrderId(orderId);
                byOrder.SalesOmzetId.Should().Be(model.SalesOmzetId);

                model.CustomerName = "Updated Customer";
                _writer.Save(ref model);

                var updated = _dal.GetData(model);
                updated.CustomerName.Should().Be("Updated Customer");
            }
        }
    }
}
