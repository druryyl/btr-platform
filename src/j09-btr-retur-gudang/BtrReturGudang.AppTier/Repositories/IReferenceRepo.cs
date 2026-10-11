using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.Domain.Entities;

namespace BtrReturGudang.AppTier.Repositories
{
    /// <summary>
    /// Repository contract for local master/reference data caches (Barang, Customer, Driver, SalesPerson).
    /// Traceable to: ARCHITECTURE §5.1, P1-S03, P2-S04.
    /// </summary>
    public interface IReferenceRepo
    {
        // Barang (Item)
        void BulkUpsertBarang(IEnumerable<Barang> items);
        Task BulkUpsertBarangAsync(IEnumerable<Barang> items);
        IEnumerable<Barang> SearchBarangByNameOrCode(string keyword);
        Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword);
        Barang GetBarangById(string brgId);
        Task<Barang> GetBarangByIdAsync(string brgId);

        // Customer
        void BulkUpsertCustomer(IEnumerable<Customer> customers);
        Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers);
        IEnumerable<Customer> SearchCustomer(string keyword);
        Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword);
        Customer GetCustomerById(string custId);
        Task<Customer> GetCustomerByIdAsync(string custId);

        // Driver
        void BulkUpsertDriver(IEnumerable<Driver> drivers);
        Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers);
        IEnumerable<Driver> GetAllDrivers();
        Task<IEnumerable<Driver>> GetAllDriversAsync();

        // SalesPerson
        void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons);
        Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons);
        IEnumerable<SalesPerson> GetAllSalesPersons();
        Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync();
    }
}
