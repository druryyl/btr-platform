using btr.domain.SalesContext.SalesOmzetAgg;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetSaleKindPolicy
    {
        SaleKindEnum Resolve(SalesOmzetModel row);
    }
}
