using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetAgg.Contracts
{
    public interface ISalesOmzetMaterializeHealthDal
    {
        SalesOmzetMaterializeHealth GetHealth(Periode window);
    }
}
