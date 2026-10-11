using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.AppTier.Services
{
    /// <summary>
    /// Application service contract for barcode scanning, resolution, and on-the-spot registration.
    /// Traceable to: ARCHITECTURE §5.1, §6, P1-S03, P3-S09.
    /// </summary>
    public interface IBarcodeService
    {
        BarcodeLookupResult LookupBarcode(string rawBarcode);
        Task<BarcodeLookupResult> LookupBarcodeAsync(string rawBarcode);

        BarcodeRegistrationDto QuickRegisterUnmappedBarcode(
            string rawBarcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            decimal pengali = 1.0m);

        Task<BarcodeRegistrationDto> QuickRegisterUnmappedBarcodeAsync(
            string rawBarcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            decimal pengali = 1.0m);

        IEnumerable<BarcodeDto> SearchBarcodes(string keyword);
        Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword);

        IEnumerable<BarcodeRegistrationDto> GetPendingRegistrations();
        Task<IEnumerable<BarcodeRegistrationDto>> GetPendingRegistrationsAsync();
    }
}
