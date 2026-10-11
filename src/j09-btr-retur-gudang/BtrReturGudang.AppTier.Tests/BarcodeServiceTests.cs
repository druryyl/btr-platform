using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using Xunit;

namespace BtrReturGudang.AppTier.Tests
{
    public class BarcodeServiceTests
    {
        private class FakeBarcodeRepo : IBarcodeRepo
        {
            public readonly Dictionary<string, Barcode> Barcodes = new Dictionary<string, Barcode>();
            public readonly List<BarcodeRegistration> Registrations = new List<BarcodeRegistration>();

            public Barcode LookupByBarcode(string barcode) =>
                Barcodes.TryGetValue(barcode, out var b) ? b : null;

            public Task<Barcode> LookupByBarcodeAsync(string barcode) =>
                Task.FromResult(LookupByBarcode(barcode));

            public void InsertBarcode(Barcode barcode) =>
                Barcodes[barcode.BarcodeValue] = barcode;

            public Task InsertBarcodeAsync(Barcode barcode)
            {
                InsertBarcode(barcode);
                return Task.CompletedTask;
            }

            public void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes)
            {
                foreach (var b in barcodes) Barcodes[b.BarcodeValue] = b;
            }

            public Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes)
            {
                BulkUpsertBarcodes(barcodes);
                return Task.CompletedTask;
            }

            public IEnumerable<Barcode> GetAllBarcodes() => Barcodes.Values;
            public Task<IEnumerable<Barcode>> GetAllBarcodesAsync() => Task.FromResult(GetAllBarcodes());

            public IEnumerable<Barcode> SearchBarcodes(string keyword) =>
                Barcodes.Values.Where(b => b.BarcodeValue.Contains(keyword) || b.BrgName.Contains(keyword));

            public Task<IEnumerable<Barcode>> SearchBarcodesAsync(string keyword) =>
                Task.FromResult(SearchBarcodes(keyword));

            public void QueueRegistration(BarcodeRegistration registration) =>
                Registrations.Add(registration);

            public Task QueueRegistrationAsync(BarcodeRegistration registration)
            {
                QueueRegistration(registration);
                return Task.CompletedTask;
            }

            public IEnumerable<BarcodeRegistration> GetPendingRegistrations() =>
                Registrations.Where(r => r.StatusSync == Domain.Enums.BarcodeRegistrationStatus.Pending);

            public Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync() =>
                Task.FromResult(GetPendingRegistrations());

            public void MarkRegistrationUploaded(string requestId)
            {
                var reg = Registrations.FirstOrDefault(r => r.RequestId == requestId);
                reg?.MarkUploaded();
            }

            public Task MarkRegistrationUploadedAsync(string requestId)
            {
                MarkRegistrationUploaded(requestId);
                return Task.CompletedTask;
            }
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public readonly Dictionary<string, Barang> Items = new Dictionary<string, Barang>();

            public void BulkUpsertBarang(IEnumerable<Barang> items)
            {
                foreach (var item in items) Items[item.BrgId] = item;
            }

            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items)
            {
                BulkUpsertBarang(items);
                return Task.CompletedTask;
            }

            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) =>
                Items.Values.Where(i => i.BrgId.Contains(keyword) || i.BrgName.Contains(keyword));

            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) =>
                Task.FromResult(SearchBarangByNameOrCode(keyword));

            public Barang GetBarangById(string brgId) =>
                Items.TryGetValue(brgId, out var b) ? b : null;

            public Task<Barang> GetBarangByIdAsync(string brgId) =>
                Task.FromResult(GetBarangById(brgId));

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;
            public IEnumerable<Customer> SearchCustomer(string keyword) => Enumerable.Empty<Customer>();
            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) => Task.FromResult(Enumerable.Empty<Customer>());
            public Customer GetCustomerById(string custId) => null;
            public Task<Customer> GetCustomerByIdAsync(string custId) => Task.FromResult<Customer>(null);
            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) => Task.CompletedTask;
            public IEnumerable<Driver> GetAllDrivers() => Enumerable.Empty<Driver>();
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(Enumerable.Empty<Driver>());
            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) => Task.CompletedTask;
            public IEnumerable<SalesPerson> GetAllSalesPersons() => Enumerable.Empty<SalesPerson>();
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(Enumerable.Empty<SalesPerson>());
        }

        [Fact]
        public void LookupBarcode_NormalizesAndFindsExistingBarcode()
        {
            var barcodeRepo = new FakeBarcodeRepo();
            barcodeRepo.InsertBarcode(new Barcode("8991234567890", "BRG-01", "Indomie Goreng", "PCS", 1.0m, true));

            var service = new BarcodeService(barcodeRepo);

            // Raw scan with trailing \r\n and whitespace
            var result = service.LookupBarcode(" 8991234567890\r\n ");

            Assert.True(result.Found);
            Assert.Equal("8991234567890", result.NormalizedBarcode);
            Assert.Equal("BRG-01", result.BrgId);
            Assert.Equal("Indomie Goreng", result.BrgName);
            Assert.Equal("PCS", result.Satuan);
            Assert.Equal(1.0m, result.Pengali);
        }

        [Fact]
        public void LookupBarcode_UnknownBarcode_ReturnsMiss()
        {
            var barcodeRepo = new FakeBarcodeRepo();
            var service = new BarcodeService(barcodeRepo);

            var result = service.LookupBarcode("UNKNOWN-BARCODE-999");

            Assert.False(result.Found);
            Assert.Equal("UNKNOWN-BARCODE-999", result.NormalizedBarcode);
        }

        [Fact]
        public void QuickRegisterUnmappedBarcode_AddsToLocalCacheAndQueuesRegistration()
        {
            var barcodeRepo = new FakeBarcodeRepo();
            var refRepo = new FakeReferenceRepo();
            refRepo.BulkUpsertBarang(new[] { new Barang("BRG-99", "Teh Botol Kotak", "KARTON", "PCS") });

            var service = new BarcodeService(barcodeRepo, refRepo);

            var regDto = service.QuickRegisterUnmappedBarcode(
                "899000111222\r\n",
                "BRG-99",
                "KARTON",
                "WHS-01",
                "Operator-1",
                pengali: 24m);

            Assert.NotNull(regDto);
            Assert.Equal("899000111222", regDto.Barcode);
            Assert.Equal("BRG-99", regDto.BrgId);
            Assert.Equal("Teh Botol Kotak", regDto.BrgName);
            Assert.Equal("KARTON", regDto.Satuan);
            Assert.Equal(24m, regDto.Pengali);
            Assert.Equal("Pending", regDto.StatusSync);

            // Verify local barcode lookup now succeeds immediately
            var lookupAfter = service.LookupBarcode("899000111222");
            Assert.True(lookupAfter.Found);
            Assert.Equal("BRG-99", lookupAfter.BrgId);
            Assert.Equal(24m, lookupAfter.Pengali);

            // Verify queued in registration table
            Assert.Single(barcodeRepo.Registrations);
        }
    }
}
