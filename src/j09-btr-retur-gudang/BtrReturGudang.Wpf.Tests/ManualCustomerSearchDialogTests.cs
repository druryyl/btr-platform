using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Wpf.ViewModels;
using BtrReturGudang.Wpf.Views;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class ManualCustomerSearchDialogTests
    {
        #region Test Fakes

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Customer> Customers { get; } = new List<Customer>();

            public FakeReferenceRepo()
            {
                Customers.Add(new Customer("CUST-001", "TOKO REJEKI", "Jl. Merdeka No. 10", "C001", "SLAWI"));
                Customers.Add(new Customer("CUST-002", "TOKO MAKMUR", "Jl. Sudirman No. 45", "C002", "TEGAL"));
                Customers.Add(new Customer("CUST-003", "WARUNG BERKAH", "Jl. Diponegoro No. 88", "C003", "BREBES"));
                Customers.Add(new Customer("CUST-004", "MINIMARKET SEJAHTERA", "Jl. Gajah Mada No. 12", "C004", "SEMARANG"));
            }

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) => Customers.AddRange(customers);
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers)
            {
                Customers.AddRange(customers);
                return Task.CompletedTask;
            }

            public IEnumerable<Customer> SearchCustomer(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return Customers;
                var k = keyword.Trim();
                return Customers.Where(c =>
                    (c.CustomerId != null && c.CustomerId.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.CustomerCode != null && c.CustomerCode.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.CustomerName != null && c.CustomerName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Alamat != null && c.Alamat.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Wilayah != null && c.Wilayah.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) =>
                Task.FromResult(SearchCustomer(keyword));

            public Customer GetCustomerById(string custId) =>
                Customers.FirstOrDefault(c => c.CustomerId == custId);

            public Task<Customer> GetCustomerByIdAsync(string custId) =>
                Task.FromResult(GetCustomerById(custId));

            // Stubs for unused interface methods
            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => Enumerable.Empty<Barang>();
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(Enumerable.Empty<Barang>());
            public Barang GetBarangById(string brgId) => null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult<Barang>(null);

            public void BulkUpsertDriver(IEnumerable<Driver> drivers) { }
            public Task BulkUpsertDriverAsync(IEnumerable<Driver> drivers) => Task.CompletedTask;
            public IEnumerable<Driver> GetAllDrivers() => Enumerable.Empty<Driver>();
            public Task<IEnumerable<Driver>> GetAllDriversAsync() => Task.FromResult(Enumerable.Empty<Driver>());

            public void BulkUpsertSalesPerson(IEnumerable<SalesPerson> salesPersons) { }
            public Task BulkUpsertSalesPersonAsync(IEnumerable<SalesPerson> salesPersons) => Task.CompletedTask;
            public IEnumerable<SalesPerson> GetAllSalesPersons() => Enumerable.Empty<SalesPerson>();
            public Task<IEnumerable<SalesPerson>> GetAllSalesPersonsAsync() => Task.FromResult(Enumerable.Empty<SalesPerson>());
        }

        private class FakeAudioHelper : IAudioHelper
        {
            public bool EnableSoundFeedback { get; set; } = true;
            public int SuccessBeepCount { get; private set; }
            public int WarningBeepCount { get; private set; }

            public void PlaySuccess() => SuccessBeepCount++;
            public void PlayWarning() => WarningBeepCount++;
        }

        #endregion

        #region STA Thread Test Helper

        private static readonly object _staLock = new object();

        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    lock (_staLock)
                    {
                        if (Application.Current == null)
                        {
                            try
                            {
                                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }

                        if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.Resources.MergedDictionaries.Count == 0)
                        {
                            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Colors.xaml", UriKind.Absolute)
                            });
                            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/Styles.xaml", UriKind.Absolute)
                            });
                            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/BtrReturGudang.Wpf;component/Themes/DataGrid.xaml", UriKind.Absolute)
                            });
                        }

                        action();
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        #endregion

        [Fact]
        public async Task ManualCustomerSearchViewModel_InitializesAndLoadsAllCustomers_WhenEmptyKeyword()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio, initialKeyword: null);
            await vm.SearchCustomersAsync(string.Empty, immediate: true);

            Assert.Equal(4, vm.Customers.Count);
            Assert.NotNull(vm.SelectedCustomer);
            Assert.Equal("CUST-001", vm.SelectedCustomer.CustomerId);
            Assert.Equal(0, vm.SelectedIndex);
            Assert.Contains("4 customer ditemukan", vm.StatusText);
            Assert.Contains("TOKO REJEKI", vm.SelectedCustomerDisplayText);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_FiltersRealTime_ByCustomerName()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);

            await vm.SearchCustomersAsync("MAKMUR", immediate: true);

            Assert.Single(vm.Customers);
            Assert.Equal("TOKO MAKMUR", vm.Customers[0].CustomerName);
            Assert.Equal("CUST-002", vm.SelectedCustomer.CustomerId);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_FiltersRealTime_ByCustomerCode()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);

            await vm.SearchCustomersAsync("C003", immediate: true);

            Assert.Single(vm.Customers);
            Assert.Equal("WARUNG BERKAH", vm.Customers[0].CustomerName);
            Assert.Equal("CUST-003", vm.SelectedCustomer.CustomerId);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_FiltersRealTime_ByAlamat()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);

            // "Sudirman" is in Alamat of CUST-002 ("Jl. Sudirman No. 45")
            await vm.SearchCustomersAsync("Sudirman", immediate: true);

            Assert.Single(vm.Customers);
            Assert.Equal("TOKO MAKMUR", vm.Customers[0].CustomerName);
            Assert.Equal("Jl. Sudirman No. 45", vm.Customers[0].Alamat);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_FiltersLargeDataset_Over2000Customers_WithoutBlocking()
        {
            var repo = new FakeReferenceRepo();
            // Populate dataset to exceed 2,000 customers (2,280 customers)
            var bulkList = new List<Customer>();
            for (int i = 5; i <= 2280; i++)
            {
                bulkList.Add(new Customer(
                    customerId: $"CUST-{i:D4}",
                    customerName: $"CUSTOMER TOKO {i}",
                    alamat: $"Jl. Industri Blok {i / 100}",
                    customerCode: $"C{i:D4}",
                    wilayah: i % 2 == 0 ? "SEMARANG" : "TEGAL"));
            }
            repo.Customers.AddRange(bulkList);
            Assert.Equal(2280, repo.Customers.Count);

            var vm = new ManualCustomerSearchViewModel(repo);

            // Filter for customer #2150
            await vm.SearchCustomersAsync("C2150", immediate: true);

            Assert.Single(vm.Customers);
            Assert.Equal("CUSTOMER TOKO 2150", vm.Customers[0].CustomerName);
            Assert.Equal("CUST-2150", vm.SelectedCustomer.CustomerId);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_NavigateDown_And_NavigateUp_ClampWithinBounds()
        {
            var repo = new FakeReferenceRepo();
            var vm = new ManualCustomerSearchViewModel(repo);
            await vm.SearchCustomersAsync(string.Empty, immediate: true);

            Assert.Equal(4, vm.Customers.Count);
            Assert.Equal(0, vm.SelectedIndex);

            // Navigate Down
            vm.NavigateDownCommand.Execute(null);
            Assert.Equal(1, vm.SelectedIndex);
            Assert.Equal("CUST-002", vm.SelectedCustomer.CustomerId);

            vm.NavigateDownCommand.Execute(null);
            Assert.Equal(2, vm.SelectedIndex);
            Assert.Equal("CUST-003", vm.SelectedCustomer.CustomerId);

            vm.NavigateDownCommand.Execute(null);
            Assert.Equal(3, vm.SelectedIndex);
            Assert.Equal("CUST-004", vm.SelectedCustomer.CustomerId);

            // Attempt navigate beyond count
            vm.NavigateDownCommand.Execute(null);
            Assert.Equal(3, vm.SelectedIndex);

            // Navigate Up
            vm.NavigateUpCommand.Execute(null);
            Assert.Equal(2, vm.SelectedIndex);
            Assert.Equal("CUST-003", vm.SelectedCustomer.CustomerId);

            vm.NavigateUpCommand.Execute(null);
            vm.NavigateUpCommand.Execute(null);
            Assert.Equal(0, vm.SelectedIndex);
            Assert.Equal("CUST-001", vm.SelectedCustomer.CustomerId);

            // Attempt navigate above 0
            vm.NavigateUpCommand.Execute(null);
            Assert.Equal(0, vm.SelectedIndex);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_ConfirmSelection_CommitsCustomer_AndRequestsCloseOk()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);
            await vm.SearchCustomersAsync(string.Empty, immediate: true);

            bool? closeResult = null;
            vm.RequestClose += r => closeResult = r;

            vm.SelectedIndex = 1;
            vm.SelectedCustomer = vm.Customers[1];

            vm.ConfirmSelectionCommand.Execute(null);

            Assert.True(closeResult);
            Assert.Equal("CUST-002", vm.SelectedCustomer.CustomerId);
            Assert.Equal(1, audio.SuccessBeepCount);
        }

        [Fact]
        public void ManualCustomerSearchViewModel_SelectWalkInCustomer_CreatesCustManualFallback()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);

            bool? closeResult = null;
            vm.RequestClose += r => closeResult = r;

            vm.SearchKeyword = "Pak Budi Toko Baru";
            vm.SelectWalkInCustomerCommand.Execute(null);

            Assert.True(closeResult);
            Assert.NotNull(vm.SelectedCustomer);
            Assert.Equal("CUST-MANUAL", vm.SelectedCustomer.CustomerId);
            Assert.Equal("Pak Budi Toko Baru", vm.SelectedCustomer.CustomerName);
            Assert.Equal("CUST-MANUAL", vm.SelectedCustomer.CustomerCode);
            Assert.Equal("Walk-in", vm.SelectedCustomer.Wilayah);
            Assert.Equal(1, audio.SuccessBeepCount);
        }

        [Fact]
        public async Task ManualCustomerSearchViewModel_ConfirmSelection_OnEmptyResults_FallsBackToWalkIn()
        {
            var repo = new FakeReferenceRepo();
            var audio = new FakeAudioHelper();
            var vm = new ManualCustomerSearchViewModel(repo, audio);

            bool? closeResult = null;
            vm.RequestClose += r => closeResult = r;

            // Search for non-existent customer name
            vm.SearchKeyword = "Warung Pojok Pelanggan Non-Master";
            await vm.SearchCustomersAsync(vm.SearchKeyword, immediate: true);
            Assert.Empty(vm.Customers);

            // Enter key invokes ConfirmSelectionCommand
            vm.ConfirmSelectionCommand.Execute(null);

            Assert.True(closeResult);
            Assert.NotNull(vm.SelectedCustomer);
            Assert.Equal("CUST-MANUAL", vm.SelectedCustomer.CustomerId);
            Assert.Equal("Warung Pojok Pelanggan Non-Master", vm.SelectedCustomer.CustomerName);
            Assert.Equal(1, audio.SuccessBeepCount);
        }

        [Fact]
        public void ManualCustomerSearchViewModel_Cancel_RequestsCloseCancel()
        {
            var repo = new FakeReferenceRepo();
            var vm = new ManualCustomerSearchViewModel(repo);

            bool? closeResult = null;
            vm.RequestClose += r => closeResult = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closeResult);
        }

        [Fact]
        public void ManualCustomerSearchDialog_RendersInStaThread_WithCorrectColumnsAndGeometry()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new FakeReferenceRepo();
                var audio = new FakeAudioHelper();
                var vm = new ManualCustomerSearchViewModel(repo, audio, "TOKO");

                var dlg = new ManualCustomerSearchDialog(vm);

                Assert.Equal(760, dlg.Width);
                Assert.Equal(480, dlg.Height);
                Assert.Equal(700, dlg.MinWidth);
                Assert.Equal(420, dlg.MinHeight);
                Assert.Equal(WindowStartupLocation.CenterOwner, dlg.WindowStartupLocation);

                // Check DataGrid columns
                Assert.Equal(4, dlg.DgvCustomers.Columns.Count);
                Assert.Equal("Kode", dlg.DgvCustomers.Columns[0].Header);
                Assert.Equal("Nama Customer", dlg.DgvCustomers.Columns[1].Header);
                Assert.Equal("Wilayah / Kota", dlg.DgvCustomers.Columns[2].Header);
                Assert.Equal("Alamat", dlg.DgvCustomers.Columns[3].Header);

                // Check Buttons exist
                Assert.NotNull(dlg.BtnSearch);
                Assert.NotNull(dlg.BtnSelect);
                Assert.NotNull(dlg.BtnCancel);
                Assert.NotNull(dlg.BtnWalkInTop);
                Assert.NotNull(dlg.BtnWalkInBottom);

                dlg.Close();
            });
        }
    }
}
