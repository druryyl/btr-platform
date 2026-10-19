using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.AppTier.Repositories;
using BtrReturGudang.AppTier.Services;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    /// <summary>
    /// Unit and UI tests for SCR-08 (ManualCustomerSearchDialog) and its integration
    /// with ReturnOrderEntryView.
    /// Traceable to: FEATURE BTR-RETUR-DESKTOP-001, TD-011, TD-013.
    /// </summary>
    public class ManualCustomerSearchDialogTests
    {
        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
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

        [Fact]
        public void ManualCustomerSearchDialog_InitializesWithDefaultState()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualCustomerSearchDialog())
                {
                    Assert.Contains("Customer", dlg.Text);
                    Assert.NotNull(dlg.SearchBox);
                    Assert.NotNull(dlg.GridView);
                    Assert.NotNull(dlg.SelectButton);
                    Assert.NotNull(dlg.CancelButtonControl);
                    Assert.Null(dlg.SelectedCustomer);
                }
            });
        }

        [Fact]
        public void ManualCustomerSearchDialog_SetCustomers_PopulatesGridAndSelectsFirstItem()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualCustomerSearchDialog())
                {
                    var customers = new List<Customer>
                    {
                        new Customer("C01", "Toko Makmur Sentosa", "Jl. Sudirman 1", "CUST-001", "Jakarta"),
                        new Customer("C02", "Toko Abadi Jaya", "Jl. Malioboro 10", "CUST-002", "Yogyakarta")
                    };

                    dlg.SetCustomers(customers);

                    Assert.Equal(2, dlg.GridView.Rows.Count);
                    Assert.NotNull(dlg.SelectedCustomer);
                    Assert.Equal("C01", dlg.SelectedCustomer.CustomerId);
                    Assert.Equal("Toko Makmur Sentosa", dlg.SelectedCustomer.CustomerName);
                    Assert.Equal("Jakarta", dlg.SelectedCustomer.Wilayah);
                }
            });
        }

        [Fact]
        public void ManualCustomerSearchDialog_PerformSearchAsync_FiltersByKeyword()
        {
            var fakeRepo = new FakeReferenceRepo();
            fakeRepo.Customers.Add(new Customer("C01", "Toko Makmur Sentosa", "Jl. Sudirman 1", "CUST-001", "Jakarta"));
            fakeRepo.Customers.Add(new Customer("C02", "Toko Abadi Jaya", "Jl. Malioboro 10", "CUST-002", "Yogyakarta"));

            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualCustomerSearchDialog(fakeRepo))
                {
                    dlg.PerformSearchAsync("Abadi").GetAwaiter().GetResult();

                    Assert.Single(dlg.FilteredCustomers);
                    Assert.Equal("C02", dlg.SelectedCustomer.CustomerId);
                    Assert.Equal("Toko Abadi Jaya", dlg.SelectedCustomer.CustomerName);
                }
            });
        }

        [Fact]
        public void ManualCustomerSearchDialog_ConfirmSelection_SetsDialogResultOK()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualCustomerSearchDialog())
                {
                    var cust = new Customer("C01", "Toko Sukses", "Jl. Raya", "CUST-001", "Semarang");
                    dlg.SetCustomers(new[] { cust });

                    dlg.ConfirmSelection();

                    Assert.Equal(DialogResult.OK, dlg.DialogResult);
                    Assert.Same(cust, dlg.SelectedCustomer);
                }
            });
        }

        [Fact]
        public void ManualCustomerSearchDialog_OnSearchKeyDown_NavigatesGridSelection()
        {
            ExecuteInStaThread(() =>
            {
                using (var dlg = new ManualCustomerSearchDialog())
                {
                    var customers = new List<Customer>
                    {
                        new Customer("C01", "Toko Pertama", "Jl. 1", "CUST-01", "Bandung"),
                        new Customer("C02", "Toko Kedua", "Jl. 2", "CUST-02", "Surabaya")
                    };
                    dlg.SetCustomers(customers);

                    // Initially row 0 selected
                    Assert.Equal("C01", dlg.SelectedCustomer.CustomerId);

                    // Down arrow moves to row 1
                    var keyDownArgs = new KeyEventArgs(Keys.Down);
                    var method = typeof(ManualCustomerSearchDialog).GetMethod("OnSearchKeyDown",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Assert.NotNull(method);
                    method.Invoke(dlg, new object[] { dlg.SearchBox, keyDownArgs });

                    Assert.Equal("C02", dlg.SelectedCustomer.CustomerId);

                    // Up arrow moves back to row 0
                    var keyUpArgs = new KeyEventArgs(Keys.Up);
                    method.Invoke(dlg, new object[] { dlg.SearchBox, keyUpArgs });

                    Assert.Equal("C01", dlg.SelectedCustomer.CustomerId);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_TriggerCustomerSearch_SetsSelectedCustomerAndPopulatesHeader()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var expectedCustomer = new Customer("C100", "Toko Berkah Utama", "Jl. Kaliurang", "CUST-100", "Sleman");

                using (var view = new ReturnOrderEntryView(orderService, null, null, new FakeAudioHelper(), new FakeRegistryHelper()))
                {
                    view.CustomerSearchDialogLauncher = keyword => expectedCustomer;

                    view.TriggerCustomerSearch("Berkah");

                    Assert.NotNull(view.SelectedCustomer);
                    Assert.Equal("C100", view.SelectedCustomer.CustomerId);
                    Assert.Contains("Toko Berkah Utama", view.CustomerTextBox.Text);
                    Assert.Contains("CUST-100", view.CustomerTextBox.Text);
                    Assert.Contains("Sleman", view.CustomerTextBox.Text);
                    Assert.Equal(view.CustomerTextBox.Text, view.CustomerComboBox.Text);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_SaveDraft_WithSelectedCustomer_PersistsCustomerCodeAndWilayah()
        {
            ExecuteInStaThread(() =>
            {
                var repo = new ValidatingReturnOrderRepo();
                var orderService = new ReturnOrderService(repo);
                var selectedCustomer = new Customer("C001", "Toko Makmur Sentosa", "Jl. Asia Afrika", "CUST-MK01", "Bandung");

                using (var view = new ReturnOrderEntryView(orderService, null, null, new FakeAudioHelper(), new FakeRegistryHelper()))
                {
                    view.SuppressMessageBoxes = true;
                    view.SetSelectedCustomer(selectedCustomer);

                    var lookup = BarcodeLookupResult.Hit("899111", "899111", "BRG01", "Item A", "Dus", 1m, true);
                    view.ApplyActiveProduct(lookup);
                    view.QtyBagusNumeric.Value = 2;
                    view.CommitQuantityStripToGrid();

                    var saved = view.SaveDraftAsync().GetAwaiter().GetResult();

                    Assert.NotNull(saved);
                    Assert.Equal("C001", saved.CustId);
                    Assert.Equal("Toko Makmur Sentosa", saved.CustName);
                    Assert.Equal("CUST-MK01", saved.CustomerCode);
                    Assert.Equal("Bandung", saved.Wilayah);

                    var stored = repo.Store[saved.ReturnOrderId];
                    Assert.Equal("CUST-MK01", stored.CustomerCode);
                    Assert.Equal("Bandung", stored.Wilayah);
                }
            });
        }

        [Fact]
        public void ReturnOrderEntryView_ResetForm_ClearsCustomerState()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderEntryView())
                {
                    var selectedCustomer = new Customer("C001", "Toko Makmur", "Jl. Asia", "CUST-01", "Jakarta");
                    view.SetSelectedCustomer(selectedCustomer);

                    Assert.NotNull(view.SelectedCustomer);
                    Assert.False(string.IsNullOrWhiteSpace(view.CustomerTextBox.Text));

                    view.ResetForm(promptUser: false);

                    Assert.Null(view.SelectedCustomer);
                    Assert.Equal(string.Empty, view.CustomerTextBox.Text);
                    Assert.Equal(string.Empty, view.CustomerComboBox.Text);
                }
            });
        }

        private class FakeReferenceRepo : IReferenceRepo
        {
            public List<Customer> Customers { get; } = new List<Customer>();

            public void BulkUpsertBarang(IEnumerable<Barang> items) { }
            public Task BulkUpsertBarangAsync(IEnumerable<Barang> items) => Task.CompletedTask;
            public IEnumerable<Barang> SearchBarangByNameOrCode(string keyword) => Enumerable.Empty<Barang>();
            public Task<IEnumerable<Barang>> SearchBarangByNameOrCodeAsync(string keyword) => Task.FromResult(Enumerable.Empty<Barang>());
            public Barang GetBarangById(string brgId) => null;
            public Task<Barang> GetBarangByIdAsync(string brgId) => Task.FromResult<Barang>(null);

            public void BulkUpsertCustomer(IEnumerable<Customer> customers) { }
            public Task BulkUpsertCustomerAsync(IEnumerable<Customer> customers) => Task.CompletedTask;

            public IEnumerable<Customer> SearchCustomer(string keyword)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return Customers;
                return Customers.Where(c =>
                    (c.CustomerId != null && c.CustomerId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.CustomerCode != null && c.CustomerCode.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.CustomerName != null && c.CustomerName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Wilayah != null && c.Wilayah.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }

            public Task<IEnumerable<Customer>> SearchCustomerAsync(string keyword) =>
                Task.FromResult(SearchCustomer(keyword));

            public Customer GetCustomerById(string custId) =>
                Customers.FirstOrDefault(c => c.CustomerId == custId);

            public Task<Customer> GetCustomerByIdAsync(string custId) =>
                Task.FromResult(GetCustomerById(custId));

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
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }
            public bool EnableSoundFeedback { get; set; } = true;

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
            public void Beep(int frequency, int durationMs) { }
        }

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-PST";
            public string ServerTargetId { get; set; } = "JOG";
            public string ApiBaseUrl { get; set; } = "http://localhost/";
            public string LastOperatorName { get; set; } = "Operator-1";
            public string LastOperatorEmail { get; set; } = "operator1@example.com";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class ValidatingReturnOrderRepo : IReturnOrderRepo
        {
            public readonly Dictionary<string, ReturnOrder> Store = new Dictionary<string, ReturnOrder>();

            public ReturnOrder GetById(string returnOrderId) => Store.TryGetValue(returnOrderId, out var o) ? o : null;
            public Task<ReturnOrder> GetByIdAsync(string returnOrderId) => Task.FromResult(GetById(returnOrderId));
            public IEnumerable<ReturnOrder> GetFilteredList(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Store.Values;
            public Task<IEnumerable<ReturnOrder>> GetFilteredListAsync(DateTime startDate, DateTime endDate, StatusSync? statusFilter = null) => Task.FromResult(GetFilteredList(startDate, endDate, statusFilter));
            public IEnumerable<ReturnOrder> GetPendingUploadList() => Store.Values;
            public Task<IEnumerable<ReturnOrder>> GetPendingUploadListAsync() => Task.FromResult(GetPendingUploadList());

            public void Save(ReturnOrder returnOrder)
            {
                if (returnOrder == null) throw new ArgumentNullException(nameof(returnOrder));
                returnOrder.Validate();
                Store[returnOrder.ReturnOrderId] = returnOrder;
            }

            public Task SaveAsync(ReturnOrder returnOrder)
            {
                Save(returnOrder);
                return Task.CompletedTask;
            }

            public void MarkSynced(string returnOrderId, DateTime syncedDate) { }
            public Task MarkSyncedAsync(string returnOrderId, DateTime syncedDate) => Task.CompletedTask;
            public void DeleteDraft(string returnOrderId) => Store.Remove(returnOrderId);
            public Task DeleteDraftAsync(string returnOrderId) => Task.CompletedTask;
        }
    }
}
