using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.nuna.Domain;

namespace btr.application.FinanceContext.PiutangAgg.Services
{
    public class PenerimaanPelunasanSalesAggregator
    {
        private readonly IPenerimaanPelunasanSalesDal _dal;

        public PenerimaanPelunasanSalesAggregator(IPenerimaanPelunasanSalesDal dal)
        {
            _dal = dal ?? throw new ArgumentNullException(nameof(dal));
        }

        public List<PenerimaanPelunasanSalesDto> Build(Periode periode)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var lunasRows = _dal.ListPiutangLunasSource(periode)?.ToList()
                ?? new List<PenerimaanPelunasanSalesLunasSourceDto>();
            if (lunasRows.Count == 0)
                return new List<PenerimaanPelunasanSalesDto>();

            var tunaiByPiutang = lunasRows
                .Where(r => r.JenisLunas == 0)
                .GroupBy(r => r.PiutangId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Nilai));

            var giroByPiutang = lunasRows
                .Where(r => r.JenisLunas == 1)
                .GroupBy(r => r.PiutangId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Nilai));

            var piutangIds = lunasRows.Select(r => r.PiutangId).Distinct();
            var elementsByPiutang = (_dal.ListPiutangElementTotals(piutangIds) ?? Enumerable.Empty<PenerimaanPelunasanSalesElementDto>())
                .ToDictionary(e => e.PiutangId);

            var buckets = new Dictionary<(DateTime Date, string SalesPersonId), PenerimaanPelunasanSalesDto>();

            foreach (var row in lunasRows)
            {
                var salesPersonId = row.SalesPersonId ?? string.Empty;
                var key = (row.LunasDate.Date, salesPersonId);

                if (!buckets.TryGetValue(key, out var dto))
                {
                    dto = new PenerimaanPelunasanSalesDto
                    {
                        LunasDate = row.LunasDate.Date,
                        SalesPersonId = salesPersonId,
                        SalesName = row.SalesName ?? string.Empty
                    };
                    buckets[key] = dto;
                }

                if (tunaiByPiutang.TryGetValue(row.PiutangId, out var tunai))
                    dto.BayarTunai += tunai;

                if (giroByPiutang.TryGetValue(row.PiutangId, out var giro))
                    dto.BayarGiro += giro;

                if (elementsByPiutang.TryGetValue(row.PiutangId, out var element))
                {
                    dto.Retur += element.Retur;
                    dto.Potongan += element.Potongan;
                    dto.MateraiAdmin += element.MateraiAdmin;
                }
            }

            foreach (var dto in buckets.Values)
                dto.TotalBayar = dto.BayarTunai + dto.BayarGiro;

            return buckets.Values
                .OrderBy(d => d.LunasDate)
                .ThenBy(d => d.SalesName)
                .ToList();
        }
    }
}
