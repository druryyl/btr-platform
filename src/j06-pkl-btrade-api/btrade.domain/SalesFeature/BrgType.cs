namespace btrade.domain.SalesFeature;

public record BrgType(string BrgId, string BrgCode, string BrgName, 
    string KategoriName,  string SatBesar, string SatKecil, int Konversi,
    decimal HrgSat, int Stok, string ServerId): IBrgKey;


public interface IBrgKey : IServerId
{
    string BrgId { get; }
}
