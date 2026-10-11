using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class ReturnOrderListViewTests
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
        public void ReturnOrderListView_InitializesWithCorrectDefaults()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderListView())
                {
                    Assert.NotNull(view.OrdersGrid);
                    Assert.NotNull(view.ItemsGrid);
                    Assert.NotNull(view.LockedBanner);
                    Assert.NotNull(view.EditButton);
                    Assert.NotNull(view.DeleteButton);
                    Assert.NotNull(view.UploadButton);
                    Assert.NotNull(view.StatusFilterBox);
                    Assert.NotNull(view.StartDatePicker);
                    Assert.NotNull(view.EndDatePicker);

                    // Defaults
                    Assert.Equal(0, view.StatusFilterBox.SelectedIndex); // "Semua (All)"
                    Assert.False(view.LockedBanner.Visible);
                    Assert.False(view.EditButton.Enabled);
                    Assert.False(view.DeleteButton.Enabled);
                    Assert.False(view.UploadButton.Enabled);
                }
            });
        }

        [Fact]
        public void ReturnOrderListView_PostSyncImmutability_LockedWhenSynced()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderListView())
                {
                    var syncedOrder = new ReturnOrderDto
                    {
                        ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
                        ReturnOrderDate = DateTime.Today,
                        CustId = "CUST-001",
                        CustName = "Toko Makmur Sentosa",
                        WarehouseCode = "GDG-PST",
                        OperatorName = "Budi Santoso",
                        StatusSync = "Synced",
                        SyncedDate = DateTime.Now,
                        Items = new List<ReturnOrderItemDto>
                        {
                            new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAV", 1, "BRG-01", "Kopi ABC", "8992753123456", "BAGUS", 5, "Dus"),
                            new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAV", 2, "BRG-02", "Teh Botol", "8992753654321", "RUSAK", 2, "Dus")
                        }
                    };

                    view.InspectOrder(syncedOrder);

                    // Assert Post-Sync Immutability Rule (TD-010, TD-011)
                    Assert.True(view.LockedBanner.Visible, "Read-Only locked banner must be displayed for Synced document.");
                    Assert.False(view.EditButton.Enabled, "Edit button must be permanently disabled for Synced document.");
                    Assert.False(view.DeleteButton.Enabled, "Delete button must be permanently disabled for Synced document.");
                    Assert.False(view.UploadButton.Enabled, "Upload button must be disabled for already Synced document.");

                    // Verify detail items populated
                    Assert.Equal(2, view.ItemsGrid.Rows.Count);
                }
            });
        }

        [Fact]
        public void ReturnOrderListView_PostSyncImmutability_UnlockedWhenDraft()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderListView())
                {
                    var draftOrder = new ReturnOrderDto
                    {
                        ReturnOrderId = "01ARZ3NDEKTSV4RRFFQ69G5FAW",
                        ReturnOrderDate = DateTime.Today,
                        CustId = "CUST-002",
                        CustName = "Warung Bu Siti",
                        WarehouseCode = "GDG-PST",
                        OperatorName = "Joko Widodo",
                        StatusSync = "Draft",
                        SyncedDate = null,
                        Items = new List<ReturnOrderItemDto>
                        {
                            new ReturnOrderItemDto("01ARZ3NDEKTSV4RRFFQ69G5FAW", 1, "BRG-03", "Indomie Goreng", "899886612345", "BAGUS", 10, "Dus")
                        }
                    };

                    view.InspectOrder(draftOrder);

                    // Draft document allows editing, deletion, and upload
                    Assert.False(view.LockedBanner.Visible, "Locked banner must remain hidden for Draft document.");
                    Assert.True(view.EditButton.Enabled, "Edit button must be enabled for Draft document.");
                    Assert.True(view.DeleteButton.Enabled, "Delete button must be enabled for Draft document.");
                    Assert.True(view.UploadButton.Enabled, "Upload button must be enabled for Draft document.");

                    Assert.Equal(1, view.ItemsGrid.Rows.Count);
                }
            });
        }

        [Fact]
        public void ReturnOrderListView_SetOrders_RendersMasterGridCorrectly()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderListView())
                {
                    var orders = new List<ReturnOrderDto>
                    {
                        new ReturnOrderDto
                        {
                            ReturnOrderId = "ORD-001",
                            ReturnOrderDate = DateTime.Today,
                            CustName = "Customer A",
                            StatusSync = "Draft",
                            Items = new List<ReturnOrderItemDto>
                            {
                                new ReturnOrderItemDto("ORD-001", 1, "B1", "Barang 1", "BAR1", "BAGUS", 5, "Dus")
                            }
                        },
                        new ReturnOrderDto
                        {
                            ReturnOrderId = "ORD-002",
                            ReturnOrderDate = DateTime.Today,
                            CustName = "Customer B",
                            StatusSync = "Synced",
                            Items = new List<ReturnOrderItemDto>
                            {
                                new ReturnOrderItemDto("ORD-002", 1, "B2", "Barang 2", "BAR2", "RUSAK", 3, "Pak")
                            }
                        }
                    };

                    view.SetOrders(orders);

                    Assert.Equal(2, view.OrdersGrid.Rows.Count);
                    Assert.Equal("ORD-001", view.OrdersGrid.Rows[0].Cells["colOrderId"].Value);
                    Assert.Equal("Draft", view.OrdersGrid.Rows[0].Cells["colStatus"].Value);
                    Assert.Equal("ORD-002", view.OrdersGrid.Rows[1].Cells["colOrderId"].Value);
                    Assert.Equal("Synced", view.OrdersGrid.Rows[1].Cells["colStatus"].Value);
                }
            });
        }

        [Fact]
        public void ReturnOrderListView_EditDraftRequested_FiresEventOnEditClick()
        {
            ExecuteInStaThread(() =>
            {
                using (var view = new ReturnOrderListView())
                {
                    string editedId = null;
                    view.EditDraftRequested += (s, id) => editedId = id;

                    var draftOrder = new ReturnOrderDto
                    {
                        ReturnOrderId = "ORD-DRAFT-123",
                        CustName = "Cust X",
                        StatusSync = "Draft"
                    };

                    view.InspectOrder(draftOrder);
                    view.EditButton.PerformClick();

                    Assert.Equal("ORD-DRAFT-123", editedId);
                }
            });
        }
    }
}
