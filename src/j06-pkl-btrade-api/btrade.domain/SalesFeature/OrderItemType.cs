using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.SalesFeature
{
    public record OrderItemType(
        string OrderId,
        int NoUrut,
        string BrgId,
        string BrgCode,
        string BrgName,
        string KategoriName,
        int QtyBesar,
        string SatBesar,
        int QtyKecil,
        string SatKecil,
        int QtyBonus,
        int Konversi,
        decimal UnitPrice,
        decimal Disc1,
        decimal Disc2,
        decimal Disc3,
        decimal Disc4,
        decimal LineTotal
    ) : IOrderKey;
}
