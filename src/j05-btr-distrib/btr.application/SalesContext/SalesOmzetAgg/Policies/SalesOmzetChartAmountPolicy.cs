using btr.application.SalesContext.OrderFeature;
using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetChartAmountPolicy : ISalesOmzetChartAmountPolicy
    {
        public decimal ResolveAmount(SalesOmzetView row)
        {
            if (row is null)
                return 0;

            switch (row.OmzetStatus)
            {
                case SalesOmzetStatusEnum.Completed:
                case SalesOmzetStatusEnum.PendingOmzet:
                    return row.FakturTotal;
                case SalesOmzetStatusEnum.Outstanding:
                    return row.OrderTotal;
                default:
                    return 0;
            }
        }

        public bool IncludeInRecognizedTotal(SalesOmzetView row) =>
            row != null && row.OmzetStatus == SalesOmzetStatusEnum.Completed;

        public bool IncludeInPipelineTotal(SalesOmzetView row) =>
            row != null && (row.OmzetStatus == SalesOmzetStatusEnum.PendingOmzet
                            || row.OmzetStatus == SalesOmzetStatusEnum.Outstanding);
    }
}
