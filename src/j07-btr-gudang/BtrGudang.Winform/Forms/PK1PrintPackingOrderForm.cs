using BtrGudang.AppTier.PackingOrderFeature;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Infrastructure.PackingOrderFeature;
using BtrGudang.Winform.Helpers;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    public partial class PK1PrintPackingOrderForm : Form
    {
        private readonly IPackingOrderRepo _packingOrderRepo;
        private readonly BindingList<PK1AllFakturView> _allFakturList;
        private readonly BindingSource _allFakturBindingSource;

        private readonly IPrintLogRepo _printLogRepo;
        private readonly IPrintLogDal _printLogDal;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;


        public PK1PrintPackingOrderForm(IPackingOrderRepo packingOrderRepo, IPrintLogRepo printLogRepo, IPrintLogDal printLogDal)
        {
            InitializeComponent();
            _packingOrderRepo = packingOrderRepo;
            _printLogRepo = printLogRepo;

            _allFakturList = new BindingList<PK1AllFakturView>();
            _allFakturBindingSource = new BindingSource(_allFakturList, null);

            RegisterControlEventHandler();
            InitGrid();
            _printLogDal = printLogDal;

            SendMessage(FilterTextBox.Handle, EM_SETCUEBANNER, 0, "Search...");

        }


        private void RegisterControlEventHandler()
        {
            ListFakturButton.Click += ListFakturButton_Click;
            PrintPerFakturButton.Click += PrintPerFakturButton_Click;
            PrintPerSupplierButton.Click += PrintPerSupplierButton_Click;
            PrintFakturListButton.Click += PrintFakturListButton_Click; 
            FilterTextBox.TextChanged += FilterText_TextChanged;
        }

        private void PrintFakturListButton_Click(object sender, EventArgs e)
        {
            var listModel = _allFakturList
                .Where(x => x.PerFaktur || x.PerSupplier)
                .Select(x => _packingOrderRepo.LoadEntity(x).Value)
                .ToList();
            var listFaktur = listModel
                .Select((x, idx) => new PrintPackingOrderFakturListView(
                    idx + 1,
                    x.Faktur.FakturCode,
                    x.Faktur.FakturDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    x.Customer.CustomerName,
                    x.Customer.Alamat,
                    x.Faktur.GrandTotal))
                .ToList();

            ExportFakturListToExcel(listFaktur);
        }

        private void ExportFakturListToExcel(IEnumerable<PrintPackingOrderFakturListView> data)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show(@"No data to export", @"Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Determine file path automatically (My Documents folder) and overwrite if exists
            var fileName = $"packing-order-faktur-list-{DateTime.Now:yyyy-MM-dd-HHmm}.xlsx";
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(folder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (IXLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Packing Order Faktur List");
                var baris = 1;
                ws.Cell($"A{baris}").Value = "CV BINTANG TIMUR RAHAYU";
                ws.Cell($"A{baris}").Style
                    .Font.SetFontSize(12)
                    .Font.SetBold(false)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range(ws.Cell($"A{baris}"), ws.Cell($"F{baris}")).Merge();
                baris++;

                ws.Cell($"A{baris}").Value = "Jl.Kaliurang Km 5.5 Gg. Durmo No.18";
                ws.Cell($"A{baris}").Style
                    .Font.SetFontSize(10)
                    .Font.SetBold(false)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range(ws.Cell($"A{baris}"), ws.Cell($"F{baris}")).Merge();
                baris++;

                ws.Cell($"A{baris}").Value = "LAPORAN PACKING LIST";
                ws.Cell($"A{baris}").Style
                    .Font.SetFontSize(16)
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Range(ws.Cell($"A{baris}"), ws.Cell($"F{baris}")).Merge();
                baris++;

                //  Add headers
                ws.Cell(baris, 1).Value = "No";
                ws.Cell(baris, 2).Value = "Faktur Code";
                ws.Cell(baris, 3).Value = "Faktur Date";
                ws.Cell(baris, 4).Value = "Customer Name";
                ws.Cell(baris, 5).Value = "Alamat";
                ws.Cell(baris, 6).Value = "Grand Total";
                //  Style header row
                ws.Range(baris, 1, baris, 6).Style
                    .Font.SetBold(true)
                    .Fill.SetBackgroundColor(XLColor.LightGray)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetInsideBorder(XLBorderStyleValues.Hair)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                baris++;
                int startTableRow = baris;
                int currentRow = baris;
                foreach (var item in data)
                {
                    ws.Cell(currentRow, 1).Value = item.No;
                    ws.Cell(currentRow, 2).Value = item.FakturCode;
                    ws.Cell(currentRow, 3).Value = item.FakturDate;
                    ws.Cell(currentRow, 4).Value = item.CustomerName;
                    ws.Cell(currentRow, 5).Value = item.Address;
                    ws.Cell(currentRow, 6).Value = item.GrandTotal;
                    //  Apply borders to item row
                    ws.Range(currentRow, 1, currentRow, 6).Style
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetInsideBorder(XLBorderStyleValues.Hair);
                    currentRow++;
                }
                ws.Range(ws.Cell($"F{startTableRow}"), ws.Cell($"F{currentRow}"))
                    .Style.NumberFormat.Format = "#,##";

                ws.Range($"E{startTableRow}:E{currentRow}").Style.Font.SetFontName("Arial Narrow");
                ws.Range($"E{startTableRow}:E{currentRow}").Style.Font.SetFontSize(9);

                ws.Range($"F{startTableRow}:F{currentRow}").Style.Font.SetFontName("Courier New");
                ws.Range($"F{startTableRow}:F{currentRow}").Style.Font.SetFontSize(9);

                ws.Column("A").Width = 3;
                ws.Column("B").Width = 9;
                ws.Column("C").Width = 10;
                ws.Column("D").Width = 25;
                ws.Column("E").Width = 37;
                ws.Column("F").Width = 12;
                wb.SaveAs(filePath);
            }
            System.Diagnostics.Process.Start(filePath);
        }

        #region Proses Per-Supplier
        private void PrintPerSupplierButton_Click(object sender, EventArgs e)
        {
            ProsesPerSupplier();
        }
        private void ProsesPerSupplier()
        {
            var listModel = _allFakturList
                .Where(x => x.PerSupplier)
                .Select(x => _packingOrderRepo.LoadEntity(x).Value)
                .ToList();
            var listPerSupplier = PrintPackingOrderPerSupplierView
                .CreateFrom(listModel)?.ToList() 
                ?? new List<PrintPackingOrderPerSupplierView>();
            ExportPerSupplierToExcel(listPerSupplier);

            var printLog = PrintLogType.Create("PER-SUPPLIER", listModel);
            foreach (var item in listModel)
                item.PrintLogBrg(printLog);

            using (var trans = TransHelper.NewScope())
            {
                _printLogRepo.SaveChanges(printLog);
                foreach (var item in listModel)
                    _packingOrderRepo.SaveChanges(item);
                trans.Complete();
            }
            ListFakturButton_Click(null, null);


        }
        private void ExportPerSupplierToExcel(IEnumerable<PrintPackingOrderPerSupplierView> data)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show(@"No data to export", @"Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Determine file path automatically (My Documents folder) and overwrite if exists
            var fileName = $"packing-order-per-supplier-{DateTime.Now:yyyy-MM-dd-HHmm}.xlsx";
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(folder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (IXLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Packing Order Per Supplier");

                int currentRow = 3;
                int no = 1;

                foreach (var supplier in data)
                {
                    //  Supplier header
                    ws.Cell(currentRow, 1).Value = $"Supplier: {supplier.Supplier}";
                    ws.Range(currentRow, 1, currentRow, 8).Merge().Style
                        .Font.SetBold(true)
                        .Font.SetFontSize(11)
                        .Fill.SetBackgroundColor(XLColor.LightGray);
                    currentRow++;

                    foreach (var kategori in supplier.ListKategori)
                    {
                        //  Kategori header
                        ws.Cell(currentRow, 2).Value = $"Kategori: {kategori.Kategori}";
                        ws.Range(currentRow, 2, currentRow, 8).Merge().Style
                            .Font.SetBold(true)
                            .Font.SetFontColor(XLColor.DarkBlue)
                            .Fill.SetBackgroundColor(XLColor.LightBlue);
                        currentRow++;

                        //  Table headers for items
                        ws.Cell(currentRow, 2).Value = "No";
                        ws.Cell(currentRow, 3).Value = "Brg Code";
                        ws.Cell(currentRow, 4).Value = "Brg Name";
                        ws.Cell(currentRow, 5).Value = "Qty Besar";
                        ws.Cell(currentRow, 6).Value = "Sat Besar";
                        ws.Cell(currentRow, 7).Value = "Qty Kecil";
                        ws.Cell(currentRow, 8).Value = "Sat Kecil";

                        //  Style header row
                        ws.Range(currentRow, 2, currentRow, 8).Style
                            .Font.SetBold(true)
                            .Fill.SetBackgroundColor(XLColor.LightGray)
                            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                            .Border.SetInsideBorder(XLBorderStyleValues.Hair)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        int itemRow = currentRow + 1;
                        int itemNo = 1;

                        foreach (var brg in kategori.ListBrg)
                        {
                            ws.Cell(itemRow, 2).Value = itemNo++;
                            ws.Cell(itemRow, 3).Value = brg.BrgCode;
                            ws.Cell(itemRow, 4).Value = brg.BrgName;
                            ws.Cell(itemRow, 5).Value = brg.SumQtyBesar;
                            ws.Cell(itemRow, 6).Value = brg.SatBesar;
                            ws.Cell(itemRow, 7).Value = brg.SumQtyKecil;
                            ws.Cell(itemRow, 8).Value = brg.SatKecil;

                            //  Apply borders to item row
                            ws.Range(itemRow, 2, itemRow, 8).Style
                                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                .Border.SetInsideBorder(XLBorderStyleValues.Hair);

                            //  Right-align numbers
                            ws.Cell(itemRow, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                            ws.Cell(itemRow, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                            itemRow++;
                        }

                        //  Add empty row between categories
                        if (kategori.ListBrg.Any())
                        {
                            //  Add summary row for kategori
                            ws.Cell(itemRow, 4).Value = $"Total {kategori.Kategori}:";
                            ws.Cell(itemRow, 5).FormulaA1 = $"=SUM(E{itemRow - kategori.ListBrg.Count()}:E{itemRow - 1})";
                            ws.Cell(itemRow, 7).FormulaA1 = $"=SUM(G{itemRow - kategori.ListBrg.Count()}:G{itemRow - 1})";

                            ws.Range(itemRow, 4, itemRow, 5).Style
                                .Font.SetBold(true)
                                .Fill.SetBackgroundColor(XLColor.LightYellow);
                            ws.Range(itemRow, 7, itemRow, 7).Style
                                .Font.SetBold(true)
                                .Fill.SetBackgroundColor(XLColor.LightYellow);

                            itemRow++;
                        }

                        currentRow = itemRow;
                    }

                    //  Add empty row between suppliers
                    currentRow++;
                }

                //  Adjust column widths
                ws.Column(1).Width = 3;    // For grouping indentation
                ws.Column(2).Width = 5;    // No
                ws.Column(3).Width = 15;   // Brg Code
                ws.Column(4).Width = 30;   // Brg Name
                ws.Column(5).Width = 12;   // Qty Besar
                ws.Column(6).Width = 10;   // Sat Besar
                ws.Column(7).Width = 12;   // Qty Kecil
                ws.Column(8).Width = 10;   // Sat Kecil

                //  Add title and date
                ws.Cell(1, 1).Value = "Packing Order Per Supplier Report";
                ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:dd-MM-yyyy HH:mm:ss}";
                ws.Range(1, 1, 2, 8).Style.Font.SetBold(true);

                //  Apply number formats
                ws.RangeUsed().Style.Font.SetFontName("Lucida Console").Font.SetFontSize(9);

                //  Save and open
                wb.SaveAs(filePath);
            }

            System.Diagnostics.Process.Start(filePath);
        }
        #endregion

        #region Proses Per-Faktur
        private void PrintPerFakturButton_Click(object sender, EventArgs e)         
        {
            var listModel = _allFakturList
                .Where(x => x.PerFaktur)
                .Select(x => _packingOrderRepo.LoadEntity(x).Value)
                .ToList();
            var listPerFaktur = PrintPackingOrderPerFakturView
                .CreateFrom(listModel)?.ToList()
                ?? new List<PrintPackingOrderPerFakturView>();

            ExportPerFakturToExcel(listPerFaktur, PisahHalamanCheckBox.Checked);

            var printLog = PrintLogType.Create("PER-FAKTUR", listModel);
            foreach (var item in listModel)
                item.PrintLogFaktur(printLog);

            using (var trans = TransHelper.NewScope())
            {
                _printLogRepo.SaveChanges(printLog);
                foreach (var item in listModel)
                    _packingOrderRepo.SaveChanges(item);
                trans.Complete();
            }
            ListFakturButton_Click(null, null);
        }

        private void ExportPerFakturToExcel(IEnumerable<PrintPackingOrderPerFakturView> data, bool pisahHalaman)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show(@"No data to export", @"Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Determine file path automatically (My Documents folder) and overwrite if exists
            var fileName = $"packing-order-per-faktur-{DateTime.Now:yyyy-MM-dd-HHmm}.xlsx";
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(folder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (IXLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Packing Order Per Faktur");

                int currentRow = 1;
                int startRow = 1; // Track start of current faktur for print area

                //  Title (will be on first page only)
                ws.Cell(currentRow, 1).Value = "PACKING ORDER PER FAKTUR REPORT";
                ws.Range(currentRow, 1, currentRow, 6).Merge().Style
                    .Font.SetBold(true)
                    .Font.SetFontSize(14)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow += 2;
                startRow = currentRow; // Title is separate, start faktur content here

                int fakturCount = 0;
                foreach (var faktur in data)
                {
                    fakturCount++;

                    // Store the starting row of this faktur
                    int fakturStartRow = currentRow;

                    //  Row 1: Customer Code and Name
                    ws.Cell(currentRow, 1).Value = $"{faktur.CustomerName} ({faktur.CustomerCode} )";
                    ws.Range(currentRow, 1, currentRow, 6).Merge()
                        .Style.Font.SetBold(true);
                    currentRow++;

                    //  Faktur Header Section - All headers on the left
                    //  Row 2: Faktur Code and Date
                    ws.Cell(currentRow, 1).Value = $"No.Faktur: {faktur.FakturCode} / {faktur.FakturDate:dd-MM-yyyy}";
                    ws.Range(currentRow, 1, currentRow, 6).Merge();
                    currentRow++;

                    //  Row 3: Customer Address
                    ws.Cell(currentRow, 1).Value = $"Alamat   : {faktur.Alamat}";
                    if (!string.IsNullOrEmpty(faktur.Location) && faktur.Location != "-")
                    {
                        var linkCell = ws.Cell(currentRow, 2);
                        linkCell.SetHyperlink(new XLHyperlink(faktur.Location));
                        linkCell.Style.Font.SetFontColor(XLColor.Blue).Font.SetUnderline();
                    }
                    ws.Range(currentRow, 1, currentRow, 6).Merge();
                    currentRow++;

                    //  Row 4: Driver
                    ws.Cell(currentRow, 1).Value = $"Driver   : {faktur.Driver}";
                    ws.Range(currentRow, 1, currentRow, 6).Merge();

                    //  set faktur title font to monospace
                    ws.Range(currentRow - 4, 1, currentRow, 6)
                        .Style.Font.SetFontName("Lucida Console").Font.SetFontSize(9);

                    //  Style all header labels
                    ws.Range(currentRow - 3, 1, currentRow, 1).Style
                        //.Font.SetBold(true)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    //  Style all header values (merged cells)
                    ws.Range(currentRow - 3, 1, currentRow, 6).Style
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

                    currentRow += 1; // Add space before items

                    //  Items Table Header - Starting at Column B
                    ws.Cell(currentRow, 1).Value = "No";
                    ws.Cell(currentRow, 2).Value = "Brg Code";
                    ws.Cell(currentRow, 3).Value = "Brg Name";
                    ws.Cell(currentRow, 4).Value = "Category - Supplier";
                    ws.Cell(currentRow, 5).Value = "Qty Besar";
                    ws.Cell(currentRow, 6).Value = "Qty Kecil";

                    //  Style table header
                    ws.Range(currentRow, 1, currentRow, 6).Style
                        .Border.SetOutsideBorder(XLBorderStyleValues.Medium)
                        .Border.SetInsideBorder(XLBorderStyleValues.Hair)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center); // Center vertically for headers

                    int itemRow = currentRow + 1;
                    int itemNo = 1;

                    //  Items Data - Starting at Column B
                    foreach (var brg in faktur.ListBrg)
                    {
                        ws.Cell(itemRow, 1).Value = itemNo++;
                        ws.Cell(itemRow, 2).Value = brg.BrgCode;
                        ws.Cell(itemRow, 3).Value = brg.BrgName;
                        ws.Cell(itemRow, 4).Value = brg.KategoriSupplier;
                        ws.Cell(itemRow, 5).Value = brg.QtyBesar;
                        ws.Cell(itemRow, 6).Value = brg.QtyKecil;

                        //  Apply borders to item row
                        ws.Range(itemRow, 1, itemRow, 6).Style
                            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                            .Border.SetInsideBorder(XLBorderStyleValues.Dashed);

                        //  Center align the number column
                        ws.Cell(itemRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        // Set text wrapping and vertical alignment for Brg Name column
                        ws.Cell(itemRow, 3).Style.Alignment.SetWrapText(true)
                            .Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                        // Set text wrapping and vertical alignment for Category - Supplier column
                        ws.Cell(itemRow, 4).Style.Alignment.SetWrapText(true)
                            .Alignment.SetVertical(XLAlignmentVerticalValues.Top);

                        itemRow++;
                    }

                    //  Add summary row for this faktur
                    if (faktur.ListBrg.Any())
                    {
                        ws.Cell(itemRow, 4).Value = $"Grand Total : {faktur.GrandTotal:N0}";
                        ws.Range(itemRow, 4, itemRow, 6).Merge().Style
                            .Font.SetBold(true)
                            //.Fill.SetBackgroundColor(XLColor.LightYellow)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                            .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                        itemRow++;
                    }

                    // Auto-fit rows for the items section to accommodate wrapped text
                    for (int row = fakturStartRow + 5; row < itemRow; row++) // +5 to skip header rows
                    {
                        ws.Row(row).AdjustToContents(); // Adjust height to content
                    }

                    if (faktur.Note.Trim() != "")
                    {
                        ws.Cell(itemRow, 1).Value = $"Note: {faktur.Note}";
                        itemRow++;
                    }

                    if (pisahHalaman)
                    {
                        // Set print area for this faktur
                        int fakturEndRow = itemRow - 1; // Last row of this faktur

                        // Define the print area range (columns 1-6, rows from fakturStartRow to fakturEndRow)
                        string printArea = $"$A${fakturStartRow}:$F${fakturEndRow}";

                        // Add this print area to the worksheet
                        if (fakturCount == 1)
                        {
                            // First faktur - set as main print area
                            ws.PageSetup.PrintAreas.Add(printArea);
                        }
                        else
                        {
                            // Subsequent fakturs - add as additional print areas
                            // Each print area will be on its own page
                            ws.PageSetup.PrintAreas.Add(printArea);
                        }

                        // Add a page break after each faktur (except the last one)
                        if (faktur != data.Last())
                        {
                            ws.PageSetup.AddHorizontalPageBreak(fakturEndRow + 1);
                            ws.Cell(fakturEndRow + 1, 1).Value = "--- Page Break ---";
                            ws.Range(fakturEndRow + 1, 1, fakturEndRow + 1, 6).Merge().Style
                                .Font.SetFontColor(XLColor.Gray)
                                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        }
                    }

                    currentRow = itemRow + 2; // Add 2 empty rows between fakturs
                }

                //  Adjust column widths
                ws.Column(1).Width = 4;   // No 
                ws.Column(2).Width = 10;  // Brg Code
                ws.Column(3).Width = 35;  // Brg Name - width increased slightly for better wrapping
                ws.Column(4).Width = 30;  // Category - Supplier - width increased slightly for better wrapping
                ws.Column(5).Width = 10;  // Qty Besar
                ws.Column(6).Width = 10;  // Qty Kecil
                ws.Column(7).Width = 2;   // Spacing

                // Optional: Additional print settings for better formatting
                // Set page orientation to portrait (or landscape if you prefer)
                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

                // Fit all columns on one page width
                ws.PageSetup.PagesWide = 1;
                double marginInInches = 0.64 / 2.54; // Convert cm to inches
                ws.PageSetup.Margins.SetLeft(marginInInches);
                ws.PageSetup.Margins.SetRight(marginInInches);

                // Optionally set header/footer
                ws.PageSetup.Header.Center.AddText("Packing Order Per Faktur Report", XLHFOccurrence.AllPages);
                ws.PageSetup.Footer.Center.AddText($"Page &P of &N", XLHFOccurrence.AllPages);

                // Scale to fit if needed
                ws.PageSetup.Scale = 90; // Scale to 90% to ensure everything fits

                //  Save and open
                wb.SaveAs(filePath);
            }
            System.Diagnostics.Process.Start(filePath);
        }
        #endregion

        private void InitGrid()
        {
            AllFakturGrid.DataSource = _allFakturBindingSource;
            AllFakturGrid.AutoGenerateColumns = true;
            AllFakturGrid.DataSource = _allFakturBindingSource;
            AllFakturGrid.RowPostPaint += DataGridViewExtension.DataGridView_RowPostPaint;
            //  TODO: Bikin View
            var cols = AllFakturGrid.Columns;
            cols["PackingOrderId"].Width = 80;
            cols["PackingOrderId"].Visible = false;
            cols["FakturCode"].Width = 70;
            cols["FakturDate"].DefaultCellStyle.Format = "dd-MM-yyyy";
            cols["FakturDate"].Width = 70;

            cols["GrandTotal"].DefaultCellStyle.Font = new Font("Lucida Console", 8);
            cols["GrandTotal"].DefaultCellStyle.Format = "N0";
            cols["GrandTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            cols["GrandTotal"].Width = 100;

            cols["Driver"].Width = 70;
            cols["AdminName"].Width = 70;
            cols["CustomerCode"].Width = 80;
            cols["CustomerName"].Width = 150;
            cols["Alamat"].Width = 200;
            cols["Alamat"].DefaultCellStyle.Font = new Font("Arial", 8);
            
            cols["Note"].Width = 70;
            cols["Note"].DefaultCellStyle.Font = new Font("Arial", 8);

            cols["MapLocation"].Width = 50;
            cols["PerSupplier"].Width = 70;
            cols["PerFaktur"].Width = 70;
            
            cols["JamCetak"].Width = 70;
            cols["JamCetak"].DefaultCellStyle.Format = "ddd, HH:mm";
            cols["JenisCetak"].Width = 80;



            foreach (DataGridViewColumn col in AllFakturGrid.Columns)
            {
                if (col.ValueType == typeof(Uri))
                {
                    DataGridViewLinkColumn linkCol = new DataGridViewLinkColumn
                    {
                        Name = col.Name,
                        HeaderText = col.HeaderText,
                        DataPropertyName = col.DataPropertyName,
                        UseColumnTextForLinkValue = false,
                    };
                    int index = col.Index;
                    AllFakturGrid.Columns.Remove(col);
                    AllFakturGrid.Columns.Insert(index, linkCol);
                    break;
                }
            }
            AllFakturGrid.CellClick += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0)
                    return;
                if (e.RowIndex >= 0 && AllFakturGrid.Columns[e.ColumnIndex].Name == "MapLocation")
                {
                    var location = AllFakturGrid.Rows[e.RowIndex].DataBoundItem as PK1AllFakturView;
                    if (location.MapLocation != null)
                    {
                        System.Diagnostics.Process.Start(location.MapLocation.AbsoluteUri);
                    }
                }
            };
            AllFakturGrid.CellContentClick += (s, e) =>
            {
                var grid = (DataGridView)s;
                if (!(grid.CurrentCell is DataGridViewCheckBoxCell))
                    return;

                grid.EndEdit();
            };
            AllFakturGrid.CellFormatting += AllFakturGrid_CellFormatting1;
            _allFakturBindingSource.ListChanged += AllFakturBindingSource_ListChanged;
        }

        private void AllFakturGrid_CellFormatting1(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < AllFakturGrid.Rows.Count)
            {
                DataGridViewRow row = AllFakturGrid.Rows[e.RowIndex];
                if (!row.IsNewRow)
                {
                    object statusValue = row.Cells["JenisCetak"].Value;
                    if (statusValue != null)
                    {
                        switch (statusValue.ToString())
                        {
                            case "PER-SUPPLIER":
                                row.DefaultCellStyle.BackColor = Color.LightGreen;
                                break;
                            case "PER-FAKTUR":
                                row.DefaultCellStyle.BackColor = Color.LightPink;
                                break;
                            default:
                                row.DefaultCellStyle.BackColor = Color.White;
                                break;
                        }
                    }
                }
            }
        }
        private void AllFakturBindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                if (_allFakturBindingSource[e.NewIndex] is PK1AllFakturView item)
                    AllFakturGrid.InvalidateRow(e.NewIndex);
            }
        }

        private void ListFakturButton_Click(object sender, EventArgs e)
        {
            var tgl1 = Tgl1DatePicker.Value;
            var tgl2 = Tgl2DatePicker.Value;
            var periode = new Periode(tgl1, tgl2);
            var listDataView = _packingOrderRepo.ListData(periode)?.ToList() 
                ?? new List<PackingOrderView>();
            var listModel = listDataView.Select(x => _packingOrderRepo
                .LoadEntity(x).Value)?.ToList() ?? new List<PackingOrderModel>();

            _allFakturList.Clear();
            var listViewPK1 = listModel.Select(x => PK1AllFakturView.CreateFrom(x));
            foreach(var item in listViewPK1)
                _allFakturList.Add(item);
        }

        private void FilterText_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filterText = FilterTextBox.Text.Trim();

            if (string.IsNullOrEmpty(filterText))
            {
                _allFakturBindingSource.DataSource = _allFakturList;
            }
            else
            {
                var filtered = _allFakturList.Where(x =>
                    (!string.IsNullOrEmpty(x.CustomerName) && x.CustomerName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.CustomerCode) && x.CustomerCode.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.FakturCode) && x.FakturCode.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.Alamat) && x.Alamat.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.Driver) && x.Alamat.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)

                ).ToList();

                _allFakturBindingSource.DataSource = new BindingList<PK1AllFakturView>(filtered);
            }
        }
    }

    #region Binding Data Source View Structure
    public class PK1AllFakturView : IPackingOrderKey, INotifyPropertyChanged
    {
        private bool? _isPersupplier; // Changed to nullable to allow all false state
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PK1AllFakturView(string packingOrderId,
            string fakturCode, DateTime fakturDate, string adminName, decimal grandTotal, string driver,
            string customerCode, string customerName, string alamat, string note, Uri mapLocation, DateTime jamCetak, string jenisCetak)
        {
            PackingOrderId = packingOrderId;
            FakturCode = fakturCode;
            FakturDate = fakturDate;
            GrandTotal = grandTotal;
            Driver = driver;
            AdminName = adminName;
            CustomerCode = customerCode;
            CustomerName = customerName;
            Alamat = alamat;
            Note = note;
            MapLocation = mapLocation;
            JamCetak = jamCetak;
            JenisCetak = jenisCetak;
            _isPersupplier = null; 
        }

        public string PackingOrderId { get; private set; }
        public string FakturCode { get; private set; }
        public DateTime FakturDate { get; private set; }
        public decimal GrandTotal { get; private set;  }
        public string Driver { get; private set; }
        public string AdminName { get; private set; }
        public string CustomerCode { get; private set; }
        public string CustomerName { get; private set; }
        public string Alamat { get; private set; }
        public string Note { get; private set; }
        public Uri MapLocation { get; private set; }

        public bool PerSupplier
        {
            get => _isPersupplier == true;
            set
            {
                if (value)
                {
                    // Setting PerSupplier to true means PerFaktur must be false
                    _isPersupplier = true;
                }
                else if (_isPersupplier == true)
                {
                    // Turning off PerSupplier when it was true means both become false
                    _isPersupplier = null;
                }
                // If value is false and _isPersupplier wasn't true, do nothing

                OnPropertyChanged(nameof(PerSupplier));
                OnPropertyChanged(nameof(PerFaktur));
            }
        }

        public bool PerFaktur
        {
            get => _isPersupplier == false;
            set
            {
                if (value)
                {
                    // Setting PerFaktur to true means PerSupplier must be false
                    _isPersupplier = false;
                }
                else if (_isPersupplier == false)
                {
                    // Turning off PerFaktur when it was true means both become false
                    _isPersupplier = null;
                }
                // If value is false and _isPersupplier wasn't false, do nothing

                OnPropertyChanged(nameof(PerFaktur));
                OnPropertyChanged(nameof(PerSupplier));
            }
        }

        public DateTime JamCetak { get; private set; }
        public string JenisCetak    { get; private set; }

        public static PK1AllFakturView CreateFrom(PackingOrderModel view)
        {
            Uri mapLocation = null;
            if (view.Location.Latitude != 0 || view.Location.Longitude != 0)
            {
                mapLocation = new Uri(string.Format(CultureInfo.InvariantCulture,
                    "https://www.google.com/maps?q={0},{1}",
                    view.Location.Latitude,
                    view.Location.Longitude));
            }

            var result = new PK1AllFakturView(view.PackingOrderId, view.Faktur.FakturCode,
                    view.Faktur.FakturDate, view.Faktur.AdminName, view.Faktur.GrandTotal, 
                    view.Driver.DriverName, view.Customer.CustomerCode, view.Customer.CustomerName, 
                    view.Customer.Alamat, view.Note, mapLocation, view.PrintTimestamp, view.DocType);
            return result;
        }
    }
#endregion

    #region PrintOut Per-Supplier View Structure
    public class PrintPackingOrderPerSupplierView
    {
        public string Supplier { get; set; }
        public IEnumerable<PrintPackingOrderPerSupplierKategoriView> ListKategori { get; set; }

        public static IEnumerable<PrintPackingOrderPerSupplierView> CreateFrom(IEnumerable<PackingOrderModel> packingOrders)
        {
            if (packingOrders == null)
                return Enumerable.Empty<PrintPackingOrderPerSupplierView>();

            // Flatten all items from all packing orders and group by Supplier and Kategori
            var result = packingOrders
                .Where(po => po != null) // Skip null packing orders
                .SelectMany(po => po.ListItem.Select(item => new
                {
                    po.PackingOrderId,
                    po.PackingOrderDate,
                    item,
                    item.Brg.Supplier,
                    item.Brg.Kategori,
                    item.Brg.BrgId,
                    item.Brg.BrgCode,
                    item.Brg.BrgName,
                    QtyBesar = item.QtyBesar.Qty,
                    SatBesar = item.QtyBesar.Satuan,
                    QtyKecil = item.QtyKecil.Qty,
                    SatKecil = item.QtyKecil.Satuan
                }))
                .GroupBy(x => x.Supplier) // First group by Supplier
                .Select(supplierGroup => new PrintPackingOrderPerSupplierView
                {
                    Supplier = supplierGroup.Key,
                    ListKategori = supplierGroup
                        .GroupBy(x => x.Kategori) // Then group by Kategori within each Supplier
                        .Select(kategoriGroup => new PrintPackingOrderPerSupplierKategoriView
                        {
                            Kategori = kategoriGroup.Key,
                            ListBrg = kategoriGroup
                                .GroupBy(x => new { x.BrgId, x.BrgCode, x.BrgName, x.SatBesar, x.SatKecil }) // Group by item details for summation
                                .Select(brgGroup => new PrintPackingOrderPerSupplierBrgView
                                {
                                    BrgId = brgGroup.Key.BrgId,
                                    BrgCode = brgGroup.Key.BrgCode,
                                    BrgName = brgGroup.Key.BrgName,
                                    SatBesar = brgGroup.Key.SatBesar,
                                    SatKecil = brgGroup.Key.SatKecil,
                                    SumQtyBesar = brgGroup.Sum(x => x.QtyBesar),
                                    SumQtyKecil = brgGroup.Sum(x => x.QtyKecil)
                                })
                                .OrderBy(brg => brg.BrgCode) // Optional ordering
                                .ToList()
                        })
                        .OrderBy(kat => kat.Kategori) // Optional ordering
                        .ToList()
                })
                .OrderBy(supp => supp.Supplier) // Optional ordering
                .ToList();

            return result;
        }
    }

    public class PrintPackingOrderPerSupplierKategoriView
    {
        public string Kategori { get; set; }
        public IEnumerable<PrintPackingOrderPerSupplierBrgView> ListBrg { get; set; }
    }

    public class PrintPackingOrderPerSupplierBrgView
    {
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public int SumQtyBesar { get; set; }
        public string SatBesar { get; set; }
        public int SumQtyKecil { get; set; }
        public string SatKecil { get; set; }
    }
#endregion

    #region PrintOut Per-Faktur View Structure
    public class PrintPackingOrderPerFakturView
    {
        public string FakturCode { get; set; }
        public string FakturDate { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Alamat { get; set; }
        public string Note { get; set;  }
        public string Location { get; set; }
        public string Driver { get; set; }
        public decimal GrandTotal { get; set; }
        public IEnumerable<PrintPackingOrderPerFakturBrgView> ListBrg { get; set; }

        public static IEnumerable<PrintPackingOrderPerFakturView> CreateFrom(IEnumerable<PackingOrderModel> packingOrders)
        {
            if (packingOrders == null)
                return Enumerable.Empty<PrintPackingOrderPerFakturView>();

            var culture = CultureInfo.InvariantCulture;

            var result = packingOrders
                .Where(po => po != null)
                .Select(po => new PrintPackingOrderPerFakturView
                {
                    FakturCode = po.Faktur?.FakturCode ?? "-",
                    FakturDate = po.Faktur?.FakturDate.ToString("dd-MM-yyyy") ?? "-",
                    CustomerCode = po.Customer?.CustomerCode ?? "-",
                    CustomerName = po.Customer?.CustomerName ?? "-",
                    Alamat = po.Customer?.Alamat ?? "-",
                    Location = GenerateMapLocation(po.Location, culture),
                    Driver = po.Driver.DriverName,
                    GrandTotal = po.Faktur.GrandTotal,
                    Note = po.Note,
                    ListBrg = po.ListItem?
                        .Where(item => item?.Brg != null)
                        .Select(item => new PrintPackingOrderPerFakturBrgView
                        {
                            BrgId = item.Brg.BrgId ?? "-",
                            BrgCode = item.Brg.BrgCode ?? "-",
                            BrgName = item.Brg.BrgName ?? "-",
                            KategoriSupplier = $"{item.Brg.Kategori ?? "-"} - {item.Brg.Supplier ?? "-"}",
                            QtyBesar = $"{item.QtyBesar?.Qty ?? 0} {item.QtyBesar?.Satuan ?? "-"}",
                            QtyKecil = $"{item.QtyKecil?.Qty ?? 0} {item.QtyKecil?.Satuan ?? "-"}"
                        })
                        .ToList() ?? Enumerable.Empty<PrintPackingOrderPerFakturBrgView>()
                })
                .OrderBy(faktur => faktur.FakturCode)
                .ToList();

            return result;
        }

        private static string GenerateMapLocation(LocationReff location, CultureInfo culture)
        {
            if (location == null)
                return "-";

            // Check if location has valid coordinates (assuming 0,0 means no location)
            if (location.Latitude == 0 && location.Longitude == 0)
                return "-";

            try
            {
                // Create Google Maps link
                var mapUri = new Uri(string.Format(culture,
                    "https://www.google.com/maps?q={0},{1}",
                    location.Latitude,
                    location.Longitude));

                return mapUri.ToString();
            }
            catch
            {
                return "-";
            }
        }
    }

    public class PrintPackingOrderPerFakturBrgView
    {
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string KategoriSupplier { get; set; }
        public string QtyBesar { get; set; }
        public string QtyKecil { get; set; }
    }
    #endregion

    public class PrintPackingOrderFakturListView
    {
        public PrintPackingOrderFakturListView(int no, string fakturCode, 
            string fakturDate, string customerName, string address, 
            decimal grandTotal)
        {
            No = no;
            FakturCode = fakturCode;
            FakturDate = fakturDate;
            CustomerName = customerName;
            Address = address;
            GrandTotal = grandTotal;
        }

        public int No { get; set; }
        public string FakturCode { get; set; }
        public string FakturDate { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
