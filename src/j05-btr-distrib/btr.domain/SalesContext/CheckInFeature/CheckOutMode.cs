namespace btr.domain.SalesContext.CheckInFeature
{
    /// <summary>
    /// Indicates how a visit was closed.
    /// </summary>
    public static class CheckOutMode
    {
        public const string Manual = "MANUAL";
        public const string Auto = "AUTO";

        public static bool IsValid(string value)
        {
            return value == Manual || value == Auto;
        }
    }
}
