using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Winform.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BtrGudang.Winform.Forms
{
    public partial class DL2DownloadPackingOrderItemForm : Form
    {
        private BindingList<DL2DownloadPackingOrderItemDto> _content;
        private PackingOrderModel _packingOrder;
        private BindingSource _bindingSource;
        public DL2DownloadPackingOrderItemForm(PackingOrderModel packingOrder)
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.CancelButton = new Button();
            _content = new BindingList<DL2DownloadPackingOrderItemDto>(packingOrder.ListItem
                .Select(x => DL2DownloadPackingOrderItemDto.FromModel(x)).ToList());
            _packingOrder = packingOrder;

            _bindingSource = new BindingSource
            {
                DataSource = _content
            };

            InitGrid();
            InitLabel();
        }

        private void InitLabel()
        {
            FakturCodeLabel.Text = _packingOrder.Faktur.FakturCode;
            FakturDateLabel.Text = _packingOrder.Faktur.FakturDate.ToString("dd-MM-yyyy");
            CustomerNameLabel.Text = $"{_packingOrder.Customer.CustomerName} ({_packingOrder.Customer.CustomerCode})";
        }

        public void InitGrid()
        {
            PackingOrderItemGrid.AutoGenerateColumns = true;
            PackingOrderItemGrid.DataSource = _bindingSource;
            PackingOrderItemGrid.Columns.SetDefaultCellStyle(Color.Azure);
            var col = PackingOrderItemGrid.Columns;
            col["BrgCode"].Width = 70;
            col["BrgCode"].DefaultCellStyle.Font = new Font("Arial", 8.25f);
            col["BrgName"].Width = 225;
            col["BrgName"].DefaultCellStyle.Font = new Font("Arial", 8.25f);
            col["Kategori"].Width = 180;
            col["Kategori"].DefaultCellStyle.Font = new Font("Arial", 8.25f);
            col["QtyBesar"].Width = 50;
            col["QtyBesar"].Width = 50;
            PackingOrderItemGrid.RowPostPaint += DataGridViewExtension.DataGridView_RowPostPaint;
        }
    }

    internal class DL2DownloadPackingOrderItemDto
    {
        public static DL2DownloadPackingOrderItemDto FromModel(PackingOrderItemModel model)
        {
            var qtyBesar = model.QtyBesar.Qty == 0 ? "-" : $"{model.QtyBesar.Qty} {model.QtyBesar.Satuan}";
            var qtyKecil = model.QtyKecil.Qty == 0 ? "-" : $"{model.QtyKecil.Qty} {model.QtyKecil.Satuan}";

            var result = new DL2DownloadPackingOrderItemDto
            {
                BrgCode = model.Brg.BrgCode,
                BrgName = model.Brg.BrgName,
                Kategori = $"{model.Brg.Kategori} - {model.Brg.Supplier}",
                QtyBesar = qtyBesar,
                QtyKecil = qtyKecil,
            };
            return result;
        }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Kategori { get; set; }
        public string QtyBesar { get; set; }
        public string QtyKecil { get; set; }
    }
}
