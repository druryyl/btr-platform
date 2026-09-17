namespace btrade.domain.ReturnOrderFeature;

public record ReturnOrderItemType(
    string ReturnOrderId,
    int NoUrut,
    string BrgId,
    string BrgCode,
    string BrgName,
    decimal Qty,
    string SatId,
    string JenisRetur) : IReturnOrderKey;
