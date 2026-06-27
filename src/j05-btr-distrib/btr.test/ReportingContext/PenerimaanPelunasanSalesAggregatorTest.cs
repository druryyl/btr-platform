using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.FinanceContext.PiutangAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PenerimaanPelunasanSalesAggregatorTest
    {
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        [Fact]
        public void Build_ReturnsEmpty_WhenNoLunasRows()
        {
            var dal = new FakePenerimaanPelunasanSalesDal();
            var aggregator = new PenerimaanPelunasanSalesAggregator(dal);

            aggregator.Build(June2026).Should().BeEmpty();
        }

        [Fact]
        public void Build_AggregatesByDateAndSalesPerson()
        {
            var dal = new FakePenerimaanPelunasanSalesDal
            {
                LunasRows = new[]
                {
                    Lunas("P1", new DateTime(2026, 6, 5), 0, 100m, "SP1", "Rep A"),
                    Lunas("P2", new DateTime(2026, 6, 5), 1, 50m, "SP1", "Rep A"),
                    Lunas("P3", new DateTime(2026, 6, 6), 0, 200m, "SP2", "Rep B"),
                },
                ElementRows = new[]
                {
                    Element("P1", retur: 10m),
                    Element("P2", potongan: 5m),
                    Element("P3", materaiAdmin: 3m),
                },
                ProjectionRows = new[]
                {
                    Projection(new DateTime(2026, 6, 5), "SP1", "Rep A", 100m, 50m, 10m, 5m, 0m),
                    Projection(new DateTime(2026, 6, 6), "SP2", "Rep B", 200m, 0m, 0m, 0m, 3m),
                }
            };

            var result = new PenerimaanPelunasanSalesAggregator(dal).Build(June2026);

            AssertMatchesProjection(dal.ProjectionRows, result);
        }

        [Fact]
        public void Build_DuplicateCountsPerPiutangTotals_ForEachLunasRow()
        {
            var dal = new FakePenerimaanPelunasanSalesDal
            {
                LunasRows = new[]
                {
                    Lunas("P1", new DateTime(2026, 6, 5), 0, 60m, "SP1", "Rep A"),
                    Lunas("P1", new DateTime(2026, 6, 5), 0, 40m, "SP1", "Rep A"),
                },
                ElementRows = new[]
                {
                    Element("P1", retur: 10m, potongan: 2m, materaiAdmin: 1m),
                },
                ProjectionRows = new[]
                {
                    Projection(new DateTime(2026, 6, 5), "SP1", "Rep A", 200m, 0m, 20m, 4m, 2m),
                }
            };

            var result = new PenerimaanPelunasanSalesAggregator(dal).Build(June2026);

            result.Should().ContainSingle();
            result[0].BayarTunai.Should().Be(200m);
            result[0].Retur.Should().Be(20m);
            result[0].Potongan.Should().Be(4m);
            result[0].MateraiAdmin.Should().Be(2m);
            result[0].TotalBayar.Should().Be(200m);

            AssertMatchesProjection(dal.ProjectionRows, result);
        }

        [Fact]
        public void Build_MatchesListDataProjection_ForSameFixture()
        {
            var dal = new FakePenerimaanPelunasanSalesDal
            {
                LunasRows = new[]
                {
                    Lunas("P1", new DateTime(2026, 6, 5), 0, 100m, "SP1", "Rep A"),
                    Lunas("P1", new DateTime(2026, 6, 5), 1, 25m, "SP1", "Rep A"),
                    Lunas("P2", new DateTime(2026, 6, 5), 0, 75m, "SP1", "Rep A"),
                },
                ElementRows = new[]
                {
                    Element("P1", retur: 8m),
                    Element("P2", potongan: 4m),
                },
                ProjectionRows = new[]
                {
                    Projection(new DateTime(2026, 6, 5), "SP1", "Rep A", 350m, 50m, 16m, 8m, 0m),
                }
            };

            var iterated = new PenerimaanPelunasanSalesAggregator(dal).Build(June2026);
            var projected = dal.ListData(June2026).ToList();

            AssertMatchesProjection(projected, iterated);
        }

        private static void AssertMatchesProjection(
            IEnumerable<PenerimaanPelunasanSalesDto> expected,
            List<PenerimaanPelunasanSalesDto> actual)
        {
            var expectedList = expected
                .OrderBy(r => r.LunasDate)
                .ThenBy(r => r.SalesPersonId)
                .ToList();

            actual.Should().HaveCount(expectedList.Count);

            for (var i = 0; i < expectedList.Count; i++)
            {
                actual[i].LunasDate.Should().Be(expectedList[i].LunasDate);
                actual[i].SalesPersonId.Should().Be(expectedList[i].SalesPersonId);
                actual[i].SalesName.Should().Be(expectedList[i].SalesName);
                actual[i].BayarTunai.Should().Be(expectedList[i].BayarTunai);
                actual[i].BayarGiro.Should().Be(expectedList[i].BayarGiro);
                actual[i].Retur.Should().Be(expectedList[i].Retur);
                actual[i].Potongan.Should().Be(expectedList[i].Potongan);
                actual[i].MateraiAdmin.Should().Be(expectedList[i].MateraiAdmin);
                actual[i].TotalBayar.Should().Be(expectedList[i].TotalBayar);
            }
        }

        private static PenerimaanPelunasanSalesLunasSourceDto Lunas(
            string piutangId,
            DateTime lunasDate,
            int jenisLunas,
            decimal nilai,
            string salesPersonId,
            string salesName) =>
            new PenerimaanPelunasanSalesLunasSourceDto
            {
                PiutangId = piutangId,
                LunasDate = lunasDate,
                JenisLunas = jenisLunas,
                Nilai = nilai,
                SalesPersonId = salesPersonId,
                SalesName = salesName
            };

        private static PenerimaanPelunasanSalesElementDto Element(
            string piutangId,
            decimal retur = 0m,
            decimal potongan = 0m,
            decimal materaiAdmin = 0m) =>
            new PenerimaanPelunasanSalesElementDto
            {
                PiutangId = piutangId,
                Retur = retur,
                Potongan = potongan,
                MateraiAdmin = materaiAdmin
            };

        private static PenerimaanPelunasanSalesDto Projection(
            DateTime lunasDate,
            string salesPersonId,
            string salesName,
            decimal bayarTunai,
            decimal bayarGiro,
            decimal retur,
            decimal potongan,
            decimal materaiAdmin) =>
            new PenerimaanPelunasanSalesDto
            {
                LunasDate = lunasDate,
                SalesPersonId = salesPersonId,
                SalesName = salesName,
                BayarTunai = bayarTunai,
                BayarGiro = bayarGiro,
                Retur = retur,
                Potongan = potongan,
                MateraiAdmin = materaiAdmin,
                TotalBayar = bayarTunai + bayarGiro
            };

        private sealed class FakePenerimaanPelunasanSalesDal : IPenerimaanPelunasanSalesDal
        {
            public IReadOnlyList<PenerimaanPelunasanSalesLunasSourceDto> LunasRows { get; set; }
                = Array.Empty<PenerimaanPelunasanSalesLunasSourceDto>();

            public IReadOnlyList<PenerimaanPelunasanSalesElementDto> ElementRows { get; set; }
                = Array.Empty<PenerimaanPelunasanSalesElementDto>();

            public IReadOnlyList<PenerimaanPelunasanSalesDto> ProjectionRows { get; set; }
                = Array.Empty<PenerimaanPelunasanSalesDto>();

            public IEnumerable<PenerimaanPelunasanSalesDto> ListData(Periode filter) =>
                ProjectionRows;

            public IEnumerable<PenerimaanPelunasanSalesLunasSourceDto> ListPiutangLunasSource(Periode filter) =>
                LunasRows;

            public IEnumerable<PenerimaanPelunasanSalesElementDto> ListPiutangElementTotals(IEnumerable<string> piutangIds)
            {
                var ids = new HashSet<string>(piutangIds ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
                return ElementRows.Where(e => ids.Contains(e.PiutangId));
            }
        }
    }
}
