using System.Collections.Generic;
using System.Linq;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.distrib.Helpers;
using btr.distrib.SharedForm;
using btr.domain.BrgContext.BrgBarcodeAgg;
using MediatR;

namespace btr.distrib.Browsers
{
    public class BrgBarcodeBrowser : IBrowser<BrgBarcodeBrowserView>
    {
        private readonly IMediator _mediator;

        public BrgBarcodeBrowser(IMediator mediator)
        {
            _mediator = mediator;
            Filter = new BrowseFilter
            {
                IsDate = false
            };
        }

        public string Browse(string defaultValue)
        {
            var form = new BrowserForm<BrgBarcodeBrowserView>(this);

            var dialogResult = form.ShowDialog();
            return dialogResult == System.Windows.Forms.DialogResult.OK 
                ? form.Result 
                : defaultValue;
        }

        public BrowseFilter Filter { get; set; }

        public IEnumerable<BrgBarcodeBrowserView> GenDataSource()
        {
            //  SCR-DESK-003 read-only source: ListBrgBarcodeQuery (barcode/Item code/Item name)
            var listData = _mediator
                               .Send(new ListBrgBarcodeQuery(Filter.UserKeyword))
                               .GetAwaiter().GetResult()
                           ?? Enumerable.Empty<BrgBarcodeModel>();

            return listData
                .OrderBy(x => x.BarcodeValue)
                .Select(x => new BrgBarcodeBrowserView
                {
                    BrgBarcodeId = x.BrgBarcodeId,
                    BarcodeValue = x.BarcodeValue,
                    BrgCode = x.BrgCode,
                    BrgName = x.BrgName,
                    Satuan = x.Satuan,
                    IsAktif = x.IsAktif
                }).ToList();
        }
    }

    public class BrgBarcodeBrowserView
    {
        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public bool IsAktif { get; set; }
    }
}
