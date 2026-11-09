using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using btr.application.InventoryContext.ReturnOrderAgg.Contracts;
using btr.distrib.Helpers;
using btr.distrib.SharedForm;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Domain;

namespace btr.distrib.Browsers
{
    public class ReturnOrderBrowser : IBrowser<ReturnOrderBrowserView>
    {
        private readonly IReturnOrderDal _returnOrderDal;

        public ReturnOrderBrowser(IReturnOrderDal returnOrderDal)
        {
            _returnOrderDal = returnOrderDal;
            Filter = new BrowseFilter
            {
                IsDate = true,
                HideAllRows = false,
                Date = new Periode(DateTime.Today.AddDays(-30), DateTime.Today)
            };
        }

        public string Browse(string defaultValue)
        {
            var form = new BrowserForm<ReturnOrderBrowserView>(this);

            var dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK)
                return form.Result;
            else
                return defaultValue;
        }

        public BrowseFilter Filter { get; set; }

        public IEnumerable<ReturnOrderBrowserView> GenDataSource()
        {
            var listData = _returnOrderDal.ListData(Filter.Date)?.ToList() ?? new List<ReturnOrderModel>();
            var result = listData
                .OrderByDescending(x => x.ReturnOrderDate)
                .Select(x => new ReturnOrderBrowserView
                {
                    Id = x.ReturnOrderId,
                    Tgl = x.ReturnOrderDate.ToString("dd-MMM HH:mm"),
                    OrderNo = x.ReturnOrderNo,
                    Customer = x.CustomerName,
                    SalesPerson = x.SalesPersonName,
                    Driver = x.DriverName,
                    Status = x.Status
                }).ToList();

            if (!string.IsNullOrEmpty(Filter.UserKeyword))
            {
                var listCustomer = result.Where(x => (x.Customer ?? string.Empty).ContainMultiWord(Filter.UserKeyword)).ToList();
                var listSales = result.Where(x => (x.SalesPerson ?? string.Empty).ContainMultiWord(Filter.UserKeyword)).ToList();
                var listOrderNo = result.Where(x => (x.OrderNo ?? string.Empty).Contains(Filter.UserKeyword)).ToList();
                var listId = result.Where(x => (x.Id ?? string.Empty).ToLower().StartsWith(Filter.UserKeyword.ToLower())).ToList();

                result = listCustomer
                    .Union(listSales)
                    .Union(listOrderNo)
                    .Union(listId)
                    .ToList();
            }

            return result;
        }
    }

    public class ReturnOrderBrowserView
    {
        public string Id { get; set; }
        public string Tgl { get; set; }
        public string OrderNo { get; set; }
        public string Customer { get; set; }
        public string SalesPerson { get; set; }
        public string Driver { get; set; }
        public string Status { get; set; }
    }
}
