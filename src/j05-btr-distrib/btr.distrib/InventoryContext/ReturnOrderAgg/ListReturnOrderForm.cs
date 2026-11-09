using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.distrib.Helpers;
using btr.distrib.InventoryContext.ReturJualAgg;
using btr.distrib.SharedForm;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Domain;
using ClosedXML.Excel;
using MediatR;

namespace btr.distrib.InventoryContext.ReturnOrderAgg
{
    public partial class ListReturnOrderForm : Form
    {
        private readonly IMediator _mediator;
        private readonly IReturJualDal _returJualDal;
        private readonly SortableBindingList<ListReturnOrderDto> _listOrderView;
        private readonly BindingSource _orderViewBindingSource;
        private ContextMenu _gridContextMenu;

        public TextBox SearchText => SearchTextBox;

        public ListReturnOrderForm(IMediator mediator, IReturJualDal returJualDal)
        {
            InitializeComponent();

            _mediator = mediator;
            _returJualDal = returJualDal;

            _listOrderView = new SortableBindingList<ListReturnOrderDto>();
            _orderViewBindingSource = new BindingSource(_listOrderView, null);

            RegisterEventHandler();
            InitDatePicker();
            InitGrid();
            InitContextMenu();
        }

        private void InitDatePicker()
        {
            PeriodeStartDatePicker.Value = DateTime.Today.AddDays(-7);
            PeriodeEndDatePicker.Value = DateTime.Today;
        }

        private void RegisterEventHandler()
        {
            SearchButton.Click += async (s, e) => await RefreshData();
            RefreshButton.Click += async (s, e) => await RefreshData();
            SearchTextBox.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    await RefreshData();
                }
            };

            ExcelButton.Click += ExcelButton_Click;
            OrderGrid.RowPostPaint += DataGridViewExtensions.DataGridView_RowPostPaint;
            OrderGrid.MouseClick += OrderGrid_MouseClick;
            OrderGrid.CellDoubleClick += OrderGrid_CellDoubleClick;
            this.Shown += async (s, e) => await RefreshData();
        }

        private void InitGrid()
        {
            OrderGrid.AutoGenerateColumns = false;
            OrderGrid.AllowUserToAddRows = false;
            OrderGrid.AllowUserToDeleteRows = false;
            OrderGrid.ReadOnly = true;
            OrderGrid.MultiSelect = false;
            OrderGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            OrderGrid.Columns.Clear();
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReturnOrderId",
                Name = "ReturnOrderId",
                HeaderText = @"ID",
                Visible = false
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReturnOrderNo",
                Name = "ReturnOrderNo",
                HeaderText = @"No Order",
                Width = 110
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReturnOrderDate",
                Name = "ReturnOrderDate",
                HeaderText = @"Tgl Order",
                Width = 85,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd-MM-yyyy" }
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CustomerName",
                Name = "CustomerName",
                HeaderText = @"Customer",
                Width = 200
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "WarehouseName",
                Name = "WarehouseName",
                HeaderText = @"Gudang",
                Width = 90
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SalesPersonName",
                Name = "SalesPersonName",
                HeaderText = @"Salesman",
                Width = 110
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DriverName",
                Name = "DriverName",
                HeaderText = @"Driver",
                Width = 110
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                Name = "Status",
                HeaderText = @"Status",
                Width = 80
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalItem",
                Name = "TotalItem",
                HeaderText = @"Item",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReturJualBagusCode",
                Name = "ReturJualBagusCode",
                HeaderText = @"No Retur Bagus",
                Width = 120
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReturJualRusakCode",
                Name = "ReturJualRusakCode",
                HeaderText = @"No Retur Rusak",
                Width = 120
            });
            OrderGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Note",
                Name = "Note",
                HeaderText = @"Catatan",
                Width = 180
            });

            OrderGrid.DataSource = _orderViewBindingSource;
            OrderGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            OrderGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            foreach (DataGridViewColumn col in OrderGrid.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
                col.DefaultCellStyle.Font = new Font("Segoe UI", 8.25f);
            }

            OrderGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= OrderGrid.Rows.Count)
                    return;

                var row = OrderGrid.Rows[e.RowIndex];
                if (row.DataBoundItem is ListReturnOrderDto item)
                {
                    if (string.Equals(item.Status, ReturnOrderStatus.Imported, StringComparison.OrdinalIgnoreCase))
                    {
                        row.DefaultCellStyle.BackColor = Color.PaleGreen;
                    }
                    else if (string.Equals(item.Status, ReturnOrderStatus.Partial, StringComparison.OrdinalIgnoreCase))
                    {
                        row.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }
            };
        }

        private void InitContextMenu()
        {
            _gridContextMenu = new ContextMenu();
            _gridContextMenu.MenuItems.Add(new MenuItem("Realisasi Retur Jual", RealisasiReturJual_OnClick));
            _gridContextMenu.MenuItems.Add(new MenuItem("Buka Retur Jual Terkait", BukaReturJual_OnClick));
            OrderGrid.ContextMenu = _gridContextMenu;
        }

        private void OrderGrid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = OrderGrid.HitTest(e.X, e.Y);
                if (hti.RowIndex >= 0)
                {
                    OrderGrid.ClearSelection();
                    OrderGrid.Rows[hti.RowIndex].Selected = true;
                    if (hti.ColumnIndex >= 0)
                    {
                        OrderGrid.CurrentCell = OrderGrid.Rows[hti.RowIndex].Cells[hti.ColumnIndex];
                    }
                }
                _gridContextMenu.Show(OrderGrid, e.Location);
            }
        }

        private void OrderGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= OrderGrid.Rows.Count)
                return;

            if (OrderGrid.Rows[e.RowIndex].DataBoundItem is ListReturnOrderDto item)
            {
                OpenReturJual(item);
            }
        }

        private ListReturnOrderDto GetSelectedOrder()
        {
            if (OrderGrid.CurrentRow != null && OrderGrid.CurrentRow.DataBoundItem is ListReturnOrderDto item)
            {
                return item;
            }
            return null;
        }

        private void RealisasiReturJual_OnClick(object sender, EventArgs e)
        {
            var item = GetSelectedOrder();
            if (item == null) return;

            if (string.Equals(item.Status, ReturnOrderStatus.Imported, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($@"Order Retur '{item.ReturnOrderNo}' sudah berstatus Imported (seluruh retur telah terbit).", @"Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mainForm = GetMainForm();
            if (mainForm == null)
            {
                MessageBox.Show(@"Main form tidak ditemukan.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mainForm.RT1ReturJualButton_Click(null, null);
            var returJualForm = Application.OpenForms.OfType<ReturJualForm>().FirstOrDefault();
            if (returJualForm != null)
            {
                returJualForm.BringToFront();
                returJualForm.LoadReturnOrder(item.ReturnOrderId);
            }
        }

        private void BukaReturJual_OnClick(object sender, EventArgs e)
        {
            var item = GetSelectedOrder();
            if (item == null) return;
            OpenReturJual(item);
        }

        private void OpenReturJual(ListReturnOrderDto item)
        {
            if (item == null) return;

            var mainForm = GetMainForm();
            if (mainForm == null)
            {
                MessageBox.Show(@"Main form tidak ditemukan.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.Equals(item.Status, ReturnOrderStatus.Imported, StringComparison.OrdinalIgnoreCase))
            {
                // Status Imported: buka dokumen Retur Jual yang telah terbit
                var activeReturs = (_returJualDal.ListByReturnOrderId(item.ReturnOrderId) ?? Enumerable.Empty<ReturJualModel>())
                    .Where(x => !x.IsVoid)
                    .ToList();

                if (activeReturs.Count == 0)
                {
                    MessageBox.Show($@"Tidak ditemukan dokumen Retur Jual aktif untuk Order Retur '{item.ReturnOrderNo}'.", @"Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ReturJualModel targetRetur;
                if (activeReturs.Count == 1)
                {
                    targetRetur = activeReturs[0];
                }
                else
                {
                    var bagus = activeReturs.FirstOrDefault(x => string.Equals(x.JenisRetur, "BAGUS", StringComparison.OrdinalIgnoreCase));
                    var rusak = activeReturs.FirstOrDefault(x => string.Equals(x.JenisRetur, "RUSAK", StringComparison.OrdinalIgnoreCase));

                    var result = MessageBox.Show(
                        $@"Order Retur memiliki 2 dokumen Retur Jual:
- BAGUS: {bagus?.ReturJualCode ?? "-"}
- RUSAK: {rusak?.ReturJualCode ?? "-"}

Pilih 'Yes' untuk membuka retur BAGUS, atau 'No' untuk membuka retur RUSAK.",
                        @"Pilih Dokumen Retur Jual",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        targetRetur = bagus ?? activeReturs[0];
                    }
                    else if (result == DialogResult.No)
                    {
                        targetRetur = rusak ?? activeReturs[activeReturs.Count - 1];
                    }
                    else
                    {
                        return;
                    }
                }

                mainForm.RT1ReturJualButton_Click(null, null);
                var returJualForm = Application.OpenForms.OfType<ReturJualForm>().FirstOrDefault();
                if (returJualForm != null && targetRetur != null)
                {
                    returJualForm.BringToFront();
                    returJualForm.ShowReturJual(targetRetur.ReturJualId);
                }
            }
            else
            {
                // Status Synced atau Partial: buka ReturJualForm dan panggil LoadReturnOrder
                mainForm.RT1ReturJualButton_Click(null, null);
                var returJualForm = Application.OpenForms.OfType<ReturJualForm>().FirstOrDefault();
                if (returJualForm != null)
                {
                    returJualForm.BringToFront();
                    returJualForm.LoadReturnOrder(item.ReturnOrderId);
                }
            }
        }

        private MainForm GetMainForm()
        {
            return this.MdiParent as MainForm
                ?? this.Parent?.Parent as MainForm
                ?? Application.OpenForms.OfType<MainForm>().FirstOrDefault();
        }

        public async Task RefreshData()
        {
            var diff = (PeriodeEndDatePicker.Value.Date - PeriodeStartDatePicker.Value.Date).TotalDays;
            if (diff < 0)
            {
                MessageBox.Show(@"Tanggal awal tidak boleh lebih besar dari tanggal akhir", @"Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (diff > 31)
            {
                MessageBox.Show(@"Periode maksimal 31 hari", @"Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tgl1 = PeriodeStartDatePicker.Value.ToString("yyyy-MM-dd");
            var tgl2 = PeriodeEndDatePicker.Value.ToString("yyyy-MM-dd");

            var response = await _mediator.Send(new ListReturnOrderMonitoringQuery(tgl1, tgl2));
            var listOrders = response?.ToList() ?? new List<ListReturnOrderDto>();

            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                var keyword = SearchTextBox.Text.Trim().ToLower();
                listOrders = listOrders
                    .Where(x => (x.ReturnOrderNo ?? string.Empty).ToLower().Contains(keyword)
                        || (x.ReturnOrderId ?? string.Empty).ToLower().Contains(keyword)
                        || (x.CustomerName ?? string.Empty).ContainMultiWord(keyword)
                        || (x.SalesPersonName ?? string.Empty).ContainMultiWord(keyword)
                        || (x.DriverName ?? string.Empty).ContainMultiWord(keyword)
                        || (x.ReturJualBagusCode ?? string.Empty).ToLower().Contains(keyword)
                        || (x.ReturJualRusakCode ?? string.Empty).ToLower().Contains(keyword))
                    .ToList();
            }

            _listOrderView.RaiseListChangedEvents = false;
            _listOrderView.Clear();
            foreach (var item in listOrders)
            {
                _listOrderView.Add(item);
            }
            _listOrderView.RaiseListChangedEvents = true;
            _listOrderView.ResetBindings();

            OrderGrid.Refresh();
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            ExportOrdersToExcel(_listOrderView.ToList());
        }

        private void ExportOrdersToExcel(List<ListReturnOrderDto> listOrders)
        {
            if (listOrders == null || !listOrders.Any())
            {
                MessageBox.Show(@"No data to export.", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(@"Export to Excel?", @"Confirmation", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            string filePath;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = @"Excel Files|*.xlsx";
                saveFileDialog.Title = @"Save Excel File";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = $"return-orders-{DateTime.Now:yyyy-MM-dd-HHmm}";
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                filePath = saveFileDialog.FileName;
            }

            using (IXLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("return-orders");

                var headers = new[]
                {
                    "No", "No Order", "Tgl Order", "Customer", "Gudang",
                    "Salesman", "Driver", "Status", "Item", "Retur Bagus",
                    "Retur Rusak", "Catatan"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = headers[i];
                }

                for (int i = 0; i < listOrders.Count; i++)
                {
                    var order = listOrders[i];
                    int row = i + 2;

                    ws.Cell(row, 1).Value = i + 1;
                    ws.Cell(row, 2).Value = order.ReturnOrderNo;
                    ws.Cell(row, 3).Value = order.ReturnOrderDate.ToString("yyyy-MM-dd");
                    ws.Cell(row, 4).Value = order.CustomerName;
                    ws.Cell(row, 5).Value = order.WarehouseName;
                    ws.Cell(row, 6).Value = order.SalesPersonName;
                    ws.Cell(row, 7).Value = order.DriverName;
                    ws.Cell(row, 8).Value = order.Status;
                    ws.Cell(row, 9).Value = order.TotalItem;
                    ws.Cell(row, 10).Value = order.ReturJualBagusCode;
                    ws.Cell(row, 11).Value = order.ReturJualRusakCode;
                    ws.Cell(row, 12).Value = order.Note;
                }

                var headerRange = ws.Range($"A1:L{listOrders.Count + 1}");
                var headerRow = ws.Range("A1:L1");

                headerRow.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                headerRow.Style.Font.SetBold();
                headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

                ws.Range($"A2:L{listOrders.Count + 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range($"A1:L{listOrders.Count + 1}").Style.Border.InsideBorder = XLBorderStyleValues.Hair;

                headerRange.Style.Font.SetFontName("Segoe UI");
                headerRange.Style.Font.SetFontSize(9f);

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();

                wb.SaveAs(filePath);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}
