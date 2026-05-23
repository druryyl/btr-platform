namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.UseCases
{
    public class GenerateSalesOmzetHealthWeeklyRequest
    {
        public int YearNumber { get; set; }
        public int WeekNumber { get; set; }
        public GenerateSalesOmzetHealthWeeklyResult Result { get; set; }
    }

    public class GenerateSalesOmzetHealthWeeklyResult
    {
        public string HealthWeeklyId { get; set; }
        public int HealthScore { get; set; }
        public string HealthLevel { get; set; }
        public int MissingOrdersCount { get; set; }
        public int MissingFaktursCount { get; set; }
        public int UnlinkedFaktursCount { get; set; }
        public int StaleDataCount { get; set; }
        public int CalculationDurationMs { get; set; }
    }
}
