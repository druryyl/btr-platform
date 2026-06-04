namespace btr.application.SalesContext.SalesOmzetAgg.Contracts
{
    public interface ISalesOmzetTargetDal
    {
        /// <summary>Monthly omzet target for incentive; null when no row exists.</summary>
        decimal? GetTargetAmount(string salesPersonId, int year, int month);
    }
}
