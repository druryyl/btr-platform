using btr.domain.PurchaseContext.SupplierAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.PurchaseContext.SupplierAgg;
using btr.infrastructure.SalesContext.SalesPersonAgg;
using btr.infrastructure.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Application;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesPersonSupplierDalTest
    {
        private readonly SalesPersonSupplierDal _sut;
        private readonly SalesPersonDal _salesPersonDal;
        private readonly SupplierDal _supplierDal;
        private readonly DatabaseOptions _databaseOptions;

        public SalesPersonSupplierDalTest()
        {
            _databaseOptions = new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            };
            var options = Options.Create(_databaseOptions);
            _sut = new SalesPersonSupplierDal(options);
            _salesPersonDal = new SalesPersonDal(options);
            _supplierDal = new SupplierDal(options);
        }

        private void SeedParents(string salesPersonId, string supplierId)
        {
            _salesPersonDal.Insert(new SalesPersonModel
            {
                SalesPersonId = salesPersonId,
                SalesPersonCode = "TST",
                SalesPersonName = "Test Sales",
                WilayahId = "W1",
                Email = "test@example.com",
                SegmentId = "S1"
            });

            _supplierDal.Insert(new SupplierModel
            {
                SupplierId = supplierId,
                SupplierCode = "TST",
                SupplierName = "Test Principal",
                Address1 = string.Empty,
                Address2 = string.Empty,
                Kota = string.Empty,
                KodePos = string.Empty,
                NoTelp = string.Empty,
                NoFax = string.Empty,
                ContactPerson = string.Empty,
                Npwp = string.Empty,
                NoPkp = string.Empty,
                Keyword = string.Empty,
                DepoId = string.Empty
            });
        }

        [Fact]
        public void UT1_InsertAndListDataTest()
        {
            var salesPersonId = "Z901";
            var supplierId = "Z901";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);

                _sut.Insert(new[]
                {
                    new SalesPersonSupplierModel
                    {
                        SalesPersonId = salesPersonId,
                        SupplierId = supplierId
                    }
                });

                var actual = _sut.ListData(new SalesPersonModel(salesPersonId)).ToList();
                actual.Should().HaveCount(1);
                actual[0].SalesPersonId.Should().Be(salesPersonId);
                actual[0].SupplierId.Should().Be(supplierId);
                actual[0].SupplierCode.Should().Be("TST");
                actual[0].SupplierName.Should().Be("Test Principal");
            }
        }

        [Fact]
        public void UT2_DeleteTest()
        {
            var salesPersonId = "Z902";
            var supplierId = "Z902";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);
                _sut.Insert(new[]
                {
                    new SalesPersonSupplierModel
                    {
                        SalesPersonId = salesPersonId,
                        SupplierId = supplierId
                    }
                });

                _sut.Delete(new SalesPersonModel(salesPersonId));

                var actual = _sut.ListData(new SalesPersonModel(salesPersonId))
                    ?? Enumerable.Empty<SalesPersonSupplierModel>();
                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public void UT3_DuplicatePrimaryKeyTest()
        {
            var salesPersonId = "Z903";
            var supplierId = "Z903";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);
                var assignment = new SalesPersonSupplierModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = supplierId
                };

                _sut.Insert(new[] { assignment });

                Action act = () => _sut.Insert(new[] { assignment });
                act.Should().Throw<SqlException>();
            }
        }
    }
}
