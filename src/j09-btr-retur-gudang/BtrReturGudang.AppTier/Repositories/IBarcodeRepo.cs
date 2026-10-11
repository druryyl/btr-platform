using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.AppTier.Repositories
{
    /// <summary>
    /// Repository contract for Barcode local caching and Barcode Registration queue.
    /// Traceable to: ARCHITECTURE §5.1, P1-S03, P2-S04.
    /// </summary>
    public interface IBarcodeRepo
    {
        Barcode LookupByBarcode(string barcode);
        Task<Barcode> LookupByBarcodeAsync(string barcode);

        void InsertBarcode(Barcode barcode);
        Task InsertBarcodeAsync(Barcode barcode);

        void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes);
        Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes);

        IEnumerable<Barcode> GetAllBarcodes();
        Task<IEnumerable<Barcode>> GetAllBarcodesAsync();

        IEnumerable<Barcode> SearchBarcodes(string keyword);
        Task<IEnumerable<Barcode>> SearchBarcodesAsync(string keyword);

        void QueueRegistration(BarcodeRegistration registration);
        Task QueueRegistrationAsync(BarcodeRegistration registration);

        IEnumerable<BarcodeRegistration> GetPendingRegistrations();
        Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync();

        void MarkRegistrationUploaded(string requestId);
        Task MarkRegistrationUploadedAsync(string requestId);
    }
}
