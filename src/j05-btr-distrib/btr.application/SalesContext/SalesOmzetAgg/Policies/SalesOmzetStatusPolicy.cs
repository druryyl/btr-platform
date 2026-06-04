using btr.domain.SalesContext.SalesOmzetAgg;
namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetStatusPolicy : ISalesOmzetStatusPolicy
    {
        public SalesOmzetStatusEnum Resolve(SalesOmzetModel row)
        {
            if (row.OmzetStatus == SalesOmzetStatusEnum.Void)
                return SalesOmzetStatusEnum.Void;

            if (!SalesOmzetDates.IsSentinel(row.OmzetDate))
                return SalesOmzetStatusEnum.Completed;

            if (!string.IsNullOrEmpty(row.FakturId))
                return SalesOmzetStatusEnum.PendingOmzet;

            if (row.SaleKind == SaleKindEnum.OrderedSale || !string.IsNullOrEmpty(row.OrderId))
                return SalesOmzetStatusEnum.Outstanding;

            if (!string.IsNullOrEmpty(row.FakturId))
                return SalesOmzetStatusEnum.PendingOmzet;

            return SalesOmzetStatusEnum.Outstanding;
        }
    }
}
