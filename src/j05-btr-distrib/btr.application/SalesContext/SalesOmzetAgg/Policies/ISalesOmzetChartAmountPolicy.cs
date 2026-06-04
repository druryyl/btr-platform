using btr.application.SalesContext.OrderFeature;

namespace btr.application.SalesContext.SalesOmzetAgg.Policies
{
    public interface ISalesOmzetChartAmountPolicy
    {
        decimal ResolveAmount(SalesOmzetView row);

        bool IncludeInRecognizedTotal(SalesOmzetView row);

        bool IncludeInPipelineTotal(SalesOmzetView row);
    }
}
