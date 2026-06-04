using btr.domain.SalesContext.SalesOmzetAgg;
namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public class SalesOmzetSaleKindPolicy : ISalesOmzetSaleKindPolicy
    {
        public SaleKindEnum Resolve(SalesOmzetModel row)
        {
            if (!string.IsNullOrEmpty(row.OrderId))
                return SaleKindEnum.OrderedSale;
            if (!string.IsNullOrEmpty(row.FakturId))
                return SaleKindEnum.DirectSale;
            return row.SaleKind;
        }
    }
}
