using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Wpf.ViewModels;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class BarcodeRegistryViewModelTests
    {
        #region Test Fakes

        private class FakeAudioHelper : IAudioHelper
        {
            public bool EnableSoundFeedback { get; set; } = true;
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
        }

        private class FakeBarcodeRepo : IBarcodeRepo
        {
            public Dictionary<string, Barcode> Barcodes { get; } = new Dictionary<string, Barcode>(StringComparer.OrdinalIgnoreCase);
            public List<BarcodeRegistration> PendingRegistrations { get; } = new List<BarcodeRegistration>();
            public List<string> DeletedBarcodes { get; } = new List<string>();

            public Barcode LookupByBarcode(string barcode) => Barcodes.TryGetValue(barcode, out var b) ? b : null;
            public Task<Barcode> LookupByBarcodeAsync(string barcode) => Task.FromResult(LookupByBarcode(barcode));

            public void InsertBarcode(Barcode barcode) => Barcodes[barcode.BarcodeValue] = barcode;
            public Task InsertBarcodeAsync(Barcode barcode) { InsertBarcode(barcode); return Task.CompletedTask; }

            public void BulkUpsertBarcodes(IEnumerable<Barcode> barcodes)
            {
                foreach (var b in barcodes) InsertBarcode(b);
            }
            public Task BulkUpsertBarcodesAsync(IEnumerable<Barcode> barcodes) { BulkUpsertBarcodes(barcodes); return Task.CompletedTask; }

            public IEnumerable<Barcode> GetAllBarcodes() => Barcodes.Values.ToList();
            public Task<IEnumerable<Barcode>> GetAllBarcodesAsync() => Task.FromResult(GetAllBarcodes());

            public IEnumerable<Barcode> SearchBarcodes(string keyword) =>
                Barcodes.Values.Where(b => b.BarcodeValue.Contains(keyword) || b.BrgName.Contains(keyword)).ToList();
            public Task<IEnumerable<BarcodeDto>> SearchBarcodesAsync(string keyword) =>
                Task.FromResult(SearchBarcodes(keyword).Select(BarcodeDto.FromDomain));

            public void QueueRegistration(BarcodeRegistration registration) => PendingRegistrations.Add(registration);
            public Task QueueRegistrationAsync(BarcodeRegistration registration) { QueueRegistration(registration); return Task.CompletedTask; }

            public IEnumerable<BarcodeRegistration> GetPendingRegistrations() => PendingRegistrations.ToList();
            public Task<IEnumerable<BarcodeRegistration>> GetPendingRegistrationsAsync() => Task.FromResult(GetPendingRegistrations());

            public void MarkRegistrationUploaded(string requestId)
            {
                var r = PendingRegistrations.FirstOrDefault(x => x.RequestId == requestId);
                r?.MarkUploaded();
            }
            public Task MarkRegistrationUploadedAsync(string requestId) { MarkRegistrationUploaded(requestId); return Task.CompletedTask; }

            public void DeleteBarcode(string barcode)
            {
                Barcodes.Remove(barcode);
                PendingRegistrations.RemoveAll(r => string.Equals(r.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
                DeletedBarcodes.Add(barcode);
            }

            public Task DeleteBarcodeAsync(string barcode)
            {
                DeleteBarcode(barcode);
                return Task.CompletedTask;
            }

            Task<IEnumerable<Barcode>> IBarcodeRepo.SearchBarcodesAsync(string keyword) =>
                Task.FromResult(SearchBarcodes(keyword));
        }

        #endregion

        [Fact]
        public async Task LoadBarcodesAsync_PopulatesActiveAndPendingBarcodes()
        {
            var repo = new FakeBarcodeRepo();
            repo.InsertBarcode(new Barcode("899123456", "BRG01", "Kopi ABC", "Dus", 24m, true));
            repo.QueueRegistration(BarcodeRegistration.Create("899999999", "BRG02", "Pak", "GDG-PST", "Operator-1"));

            var vm = new BarcodeRegistryViewModel(repo);
            await vm.LoadBarcodesAsync();

            Assert.Equal(2, vm.AllItems.Count);
            Assert.Contains(vm.AllItems, x => x.Barcode == "899123456" && x.Status == "Aktif Pusat" && !x.IsPending);
            Assert.Contains(vm.AllItems, x => x.Barcode == "899999999" && x.Status == "Pending Upload" && x.IsPending);
        }

        [Fact]
        public void SearchKeyword_FiltersItemsByBarcodeOrName()
        {
            var vm = new BarcodeRegistryViewModel();
            vm.SetBarcodes(new[]
            {
                new BarcodeDisplayItem { Barcode = "899111", BrgId = "B1", BrgName = "Kopi Susu", Satuan = "Dus", Pengali = 24, Status = "Aktif Pusat" },
                new BarcodeDisplayItem { Barcode = "899222", BrgId = "B2", BrgName = "Teh Melati", Satuan = "Pak", Pengali = 12, Status = "Aktif Pusat" },
                new BarcodeDisplayItem { Barcode = "999333", BrgId = "B3", BrgName = "Indomie Goreng", Satuan = "Dus", Pengali = 40, Status = "Pending Upload", IsPending = true }
            });

            Assert.Equal(3, vm.FilteredItems.Count);

            // Filter by product name
            vm.SearchKeyword = "Teh";
            Assert.Single(vm.FilteredItems);
            Assert.Equal("899222", vm.FilteredItems[0].Barcode);

            // Filter by barcode digits
            vm.SearchKeyword = "999";
            Assert.Single(vm.FilteredItems);
            Assert.Equal("999333", vm.FilteredItems[0].Barcode);

            // Reset
            vm.SearchKeyword = string.Empty;
            Assert.Equal(3, vm.FilteredItems.Count);
        }

        [Fact]
        public void SelectedStatusFilter_FiltersByStatus()
        {
            var vm = new BarcodeRegistryViewModel();
            vm.SetBarcodes(new[]
            {
                new BarcodeDisplayItem { Barcode = "BC-01", BrgId = "B1", BrgName = "Item 1", Status = "Aktif Pusat", IsPending = false },
                new BarcodeDisplayItem { Barcode = "BC-02", BrgId = "B2", BrgName = "Item 2", Status = "Pending Upload", IsPending = true }
            });

            vm.SelectedStatusFilter = "Aktif Pusat";
            Assert.Single(vm.FilteredItems);
            Assert.False(vm.FilteredItems[0].IsPending);

            vm.SelectedStatusFilter = "Pending Upload";
            Assert.Single(vm.FilteredItems);
            Assert.True(vm.FilteredItems[0].IsPending);

            vm.SelectedStatusFilter = "Semua Status";
            Assert.Equal(2, vm.FilteredItems.Count);
        }

        [Fact]
        public void Pagination_CalculatesPagesAndNavigatesProperly()
        {
            var vm = new BarcodeRegistryViewModel();
            var items = Enumerable.Range(1, 45).Select(i => new BarcodeDisplayItem
            {
                Barcode = $"BC-{i:D3}",
                BrgId = $"B{i}",
                BrgName = $"Product {i}",
                Status = "Aktif Pusat"
            }).ToList();

            vm.PageSize = 20;
            vm.SetBarcodes(items);

            Assert.Equal(3, vm.TotalPages); // 45 / 20 -> 3 pages
            Assert.Equal(1, vm.CurrentPage);
            Assert.Equal(20, vm.PagedItems.Count);
            Assert.True(vm.CanNextPage);
            Assert.False(vm.CanPreviousPage);

            vm.NextPage();
            Assert.Equal(2, vm.CurrentPage);
            Assert.Equal(20, vm.PagedItems.Count);
            Assert.True(vm.CanNextPage);
            Assert.True(vm.CanPreviousPage);

            vm.NextPage();
            Assert.Equal(3, vm.CurrentPage);
            Assert.Equal(5, vm.PagedItems.Count); // Last page has 5
            Assert.False(vm.CanNextPage);
            Assert.True(vm.CanPreviousPage);

            vm.PreviousPage();
            Assert.Equal(2, vm.CurrentPage);
        }

        [Fact]
        public async Task DeleteBarcodeAsync_DeletesFromRepoAndList()
        {
            var repo = new FakeBarcodeRepo();
            var audio = new FakeAudioHelper();
            repo.InsertBarcode(new Barcode("899123456", "BRG01", "Kopi ABC", "Dus", 24m, true));

            var vm = new BarcodeRegistryViewModel(barcodeRepo: repo, audioHelper: audio);
            vm.ConfirmDeleteFunc = _ => true; // Auto-confirm
            await vm.LoadBarcodesAsync();

            Assert.Single(vm.AllItems);

            var itemToDelete = vm.AllItems[0];
            await vm.DeleteBarcodeAsync(itemToDelete);

            Assert.Empty(vm.AllItems);
            Assert.Empty(vm.FilteredItems);
            Assert.Single(repo.DeletedBarcodes);
            Assert.Equal("899123456", repo.DeletedBarcodes[0]);
            Assert.Equal(1, audio.SuccessCount);
        }
    }
}
