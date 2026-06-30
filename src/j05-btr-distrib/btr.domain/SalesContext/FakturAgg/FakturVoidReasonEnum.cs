namespace btr.domain.SalesContext.FakturAgg
{
    public enum FakturVoidReasonEnum
    {
        None = 0,
        SalahInput = 1,
        Revisi = 2,
        CustomerReject = 3
    }

    public static class FakturVoidReason
    {
        public static bool IsValid(int code)
            => code == (int)FakturVoidReasonEnum.SalahInput
               || code == (int)FakturVoidReasonEnum.Revisi
               || code == (int)FakturVoidReasonEnum.CustomerReject;
    }
}
