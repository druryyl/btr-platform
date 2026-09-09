namespace btr.application.ReportingContext.PrincipalAnalyticsAgg
{
    public static class CustomerPrincipalRelationship
    {
        public const string SnapshotKey = "CURRENT";

        public const string Domain = "PrnCusRelationship";

        public const string TableName = "BTRPD_CustomerPrincipalRelationship";

        public const string StatusActive = "Active";

        public const string StatusDormant = "Dormant";

        public const int ActiveWindowMonths = 6;

        public const string HistoricalLimitation =
            "Item Principal is current Item master and historical reconstruction may be limited.";

        public const string PairSalesOutIsPrincipalSalesOut =
            "Pair Sales-Out stored on the projection is pair-attributed PRN-SALES-001 and does not deduct returns, claims, or inventory adjustments.";

        public const string ConsumerQueriesAreNotPartOfThisSlice =
            "Consumer queries are not part of this slice. Coverage, Active Customer, Dormant Customer, Relationship Analytics, and Entity Analytics read this projection in later slices.";
    }
}
