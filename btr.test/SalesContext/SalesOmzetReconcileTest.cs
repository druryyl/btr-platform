using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturAgg.Contracts;
using btr.application.SalesContext.FakturControlAgg;
using btr.application.SalesContext.OrderFeature;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SalesContext.SalesOmzetAgg.Policies;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.UseCases;
using btr.application.SalesContext.SalesOmzetAgg.Workers;
using btr.domain.SalesContext.SalesOmzetAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.SalesContext.CustomerAgg;
using btr.infrastructure.SalesContext.FakturAgg;
using btr.infrastructure.SalesContext.FakturControlAgg;
using btr.infrastructure.SalesContext.OrderFeature;
using btr.infrastructure.SalesContext.SalesOmzetAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesOmzetReconcileTest
    {
        private readonly IReconcileSalesOmzetWorker _worker;
        private readonly ISalesOmzetEntityDal _entityDal;
        private readonly string _connString;

        public SalesOmzetReconcileTest()
        {
            var databaseOptions = new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            };
            var options = Options.Create(databaseOptions);
            _connString = ConnStringHelper.Get(databaseOptions);

            _entityDal = new SalesOmzetEntityDal(options);
            var writer = new SalesOmzetWriter(_entityDal, new NunaCounterBL(new ParamNoDal(options)));

            var source = new SalesOmzetSourceDal(
                new OrderDal(options),
                new FakturDal(options),
                new CustomerDal(options),
                new FakturControlStatusDal(options),
                options);

            var linker = new SalesOmzetLinker(
                _entityDal,
                writer,
                source,
                new SalesOmzetSnapshotBuilder(),
                new SalesOmzetSaleKindPolicy(),
                new SalesOmzetStatusPolicy(),
                new SalesOmzetEligibilityPolicy());

            _worker = new ReconcileSalesOmzetWorker(source, linker, _entityDal);
        }

        private static Periode LastSevenDays()
        {
            var end = DateTime.Today;
            return new Periode(end.AddDays(-7), end);
        }

        [Fact]
        public void UT1_ReconcilePeriodeScoped_CompletesWithoutError()
        {
            using (var trans = TransHelper.NewScope())
            {
                Action act = () => _worker.Execute(new ReconcileSalesOmzetRequest { Periode = LastSevenDays() });
                act.Should().NotThrow();
            }
        }

        [Fact]
        public void UT2_ReconcileTwice_NoDuplicateOrderOrFakturKeys()
        {
            var periode = LastSevenDays();
            using (var trans = TransHelper.NewScope())
            {
                _worker.Execute(new ReconcileSalesOmzetRequest { Periode = periode });
                _worker.Execute(new ReconcileSalesOmzetRequest { Periode = periode });

                CountDuplicateKeys("OrderId").Should().Be(0);
                CountDuplicateKeys("FakturId").Should().Be(0);
            }
        }

        private int CountDuplicateKeys(string column)
        {
            var sql = $@"
            SELECT COUNT(*) FROM (
                SELECT {column}
                FROM BTR_SalesOmzet
                WHERE {column} <> ''
                GROUP BY {column}
                HAVING COUNT(*) > 1
            ) dup";

            using (var conn = new SqlConnection(_connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}
