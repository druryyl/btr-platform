using btr.domain.PurchaseContext.SupplierAgg;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.PurchaseContext.SupplierAgg;
using btr.infrastructure.SalesContext.SalesPersonAgg;
using btr.infrastructure.SalesContext.SalesPersonPrincipalTargetAgg;
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
    public class SalesPersonPrincipalTargetDalTest
    {
        private readonly SalesPersonPrincipalTargetDal _sut;
        private readonly SalesPersonDal _salesPersonDal;
        private readonly SupplierDal _supplierDal;
        private readonly SalesPersonSupplierDal _salesPersonSupplierDal;
        private readonly DatabaseOptions _databaseOptions;

        public SalesPersonPrincipalTargetDalTest()
        {
            _databaseOptions = new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            };
            var options = Options.Create(_databaseOptions);
            _sut = new SalesPersonPrincipalTargetDal(options);
            _salesPersonDal = new SalesPersonDal(options);
            _supplierDal = new SupplierDal(options);
            _salesPersonSupplierDal = new SalesPersonSupplierDal(options);
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
        public void UT1_UpsertAndListBySalesPersonPeriodTest()
        {
            var salesPersonId = "Z911";
            var supplierId = "Z911";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);

                _sut.Upsert(new[]
                {
                    new SalesPersonPrincipalTargetModel
                    {
                        SalesPersonId = salesPersonId,
                        SupplierId = supplierId,
                        TargetYear = 2026,
                        TargetMonth = 1,
                        TargetAmount = 500_000_000m
                    }
                });

                var actual = _sut.ListBySalesPersonPeriod(salesPersonId, 2026, 1).ToList();
                actual.Should().HaveCount(1);
                actual[0].SalesPersonId.Should().Be(salesPersonId);
                actual[0].SupplierId.Should().Be(supplierId);
                actual[0].TargetAmount.Should().Be(500_000_000m);
                actual[0].SupplierCode.Should().Be("TST");
                actual[0].SupplierName.Should().Be("Test Principal");
            }
        }

        [Fact]
        public void UT2_UpsertUpdatesExistingRowTest()
        {
            var salesPersonId = "Z912";
            var supplierId = "Z912";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);

                var row = new SalesPersonPrincipalTargetModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = supplierId,
                    TargetYear = 2026,
                    TargetMonth = 2,
                    TargetAmount = 100m
                };

                _sut.Upsert(new[] { row });
                row.TargetAmount = 200m;
                _sut.Upsert(new[] { row });

                var actual = _sut.ListBySalesPersonPeriod(salesPersonId, 2026, 2).Single();
                actual.TargetAmount.Should().Be(200m);
            }
        }

        [Fact]
        public void UT3_SumBySalesPersonPeriodTest()
        {
            var salesPersonId = "Z913";
            var supplierId1 = "Z913";
            var supplierId2 = "Z914";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId1);
                _supplierDal.Insert(new SupplierModel
                {
                    SupplierId = supplierId2,
                    SupplierCode = "TS2",
                    SupplierName = "Test Principal 2",
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

                _sut.Upsert(new[]
                {
                    new SalesPersonPrincipalTargetModel
                    {
                        SalesPersonId = salesPersonId,
                        SupplierId = supplierId1,
                        TargetYear = 2026,
                        TargetMonth = 3,
                        TargetAmount = 300m
                    },
                    new SalesPersonPrincipalTargetModel
                    {
                        SalesPersonId = salesPersonId,
                        SupplierId = supplierId2,
                        TargetYear = 2026,
                        TargetMonth = 3,
                        TargetAmount = 200m
                    }
                });

                _sut.SumBySalesPersonPeriod(salesPersonId, 2026, 3).Should().Be(500m);
            }
        }

        [Fact]
        public void UT4_DuplicatePrimaryKeyHandledByUpsertTest()
        {
            var salesPersonId = "Z915";
            var supplierId = "Z915";

            using (var trans = TransHelper.NewScope())
            {
                SeedParents(salesPersonId, supplierId);

                var row = new SalesPersonPrincipalTargetModel
                {
                    SalesPersonId = salesPersonId,
                    SupplierId = supplierId,
                    TargetYear = 2026,
                    TargetMonth = 4,
                    TargetAmount = 50m
                };

                _sut.Upsert(new[] { row, row });

                var actual = _sut.ListBySalesPersonPeriod(salesPersonId, 2026, 4);
                actual.Should().HaveCount(1);
            }
        }
    }
}
