namespace btr.application.SalesContext.SalesOmzetAgg.Contracts
{
    public interface ISalesOmzetTargetDal
    {
        /// <summary>Monthly omzet target for incentive; null when no row exists.</summary>
        decimal? GetTargetAmount(string salesPersonId, int year, int month);

        /// <summary>Sum of TargetAmount for all salespeople with a target row for the given month.</summary>
        decimal SumTargetAmountForMonth(int year, int month);
    }
}
