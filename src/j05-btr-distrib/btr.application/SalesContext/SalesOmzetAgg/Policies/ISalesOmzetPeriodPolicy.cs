using btr.domain.SalesContext.SalesOmzetAgg;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetPeriodPolicy
    {
        bool IsInPeriod(SalesOmzetModel row, Periode periode, SalesOmzetPeriodFilterMode mode);

        /// <summary>SQL fragment for Phase 4 ListData; caller supplies @Tgl1/@Tgl2 on DynamicParameters.</summary>
        string ToSqlWhere(SalesOmzetPeriodFilterMode mode);
    }
}
