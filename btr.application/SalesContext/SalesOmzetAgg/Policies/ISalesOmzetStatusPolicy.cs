using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetStatusPolicy
    {
        SalesOmzetStatusEnum Resolve(SalesOmzetModel row);
    }
}
