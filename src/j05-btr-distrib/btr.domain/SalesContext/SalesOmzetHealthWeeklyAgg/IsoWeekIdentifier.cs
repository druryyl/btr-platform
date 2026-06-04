namespace btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg
{
    public struct IsoWeekIdentifier
    {
        public IsoWeekIdentifier(int yearNumber, int weekNumber)
        {
            YearNumber = yearNumber;
            WeekNumber = weekNumber;
        }

        public int YearNumber { get; }
        public int WeekNumber { get; }
    }
}
