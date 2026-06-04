using btr.application.SalesContext.SalesOmzetAgg.Snapshots;
using btr.domain.SalesContext.SalesOmzetAgg;
namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetEligibilityPolicy : ISalesOmzetEligibilityPolicy
    {
        public bool IsFakturEligible(FakturSnapshot faktur)
        {
            if (faktur is null) return false;
            return SalesOmzetDates.IsNotVoid(faktur.VoidDate);
        }

        public bool IsOrderEligible(OrderSnapshot order)
        {
            if (order is null) return false;
            // Exclusions (e.g. cancelled sync) can be added here when business rules are defined.
            return !string.IsNullOrEmpty(order.OrderId);
        }

        public bool ShouldRemove(SalesOmzetModel row, FakturSnapshot linkedFaktur)
        {
            if (row is null) return true;
            if (linkedFaktur != null && !IsFakturEligible(linkedFaktur))
                return true;
            return row.OmzetStatus == SalesOmzetStatusEnum.Void;
        }
    }
}
