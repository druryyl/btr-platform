using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Domain.PackingOrderFeature
{
    public class PackingOrderModel : IPackingOrderKey
    {
        private readonly List<PackingOrderItemModel> _listItem;

        public PackingOrderModel(
            string packingOrderId,
            DateTime packingOrderDate,
            CustomerReff customer,
            LocationReff location,
            FakturReff faktur,
            DriverReff driver,
            DateTime downloadTimestamp,
            string offoiceCode,
            string printLogId,
            DateTime printTimestamp,
            string docType,
            string note,
            IEnumerable<PackingOrderItemModel> listItem)
        {
            PackingOrderId = packingOrderId;
            PackingOrderDate = packingOrderDate;
            Customer = customer;
            Location = location;
            Faktur = faktur;
            Driver = driver;
            DownloadTimestamp = downloadTimestamp;
            OfficeCode = offoiceCode;
            PrintLogId = printLogId;
            PrintTimestamp = printTimestamp;
            DocType = docType;
            Note = note;
            _listItem = listItem.ToList();
        }

        public static PackingOrderModel Default => new PackingOrderModel(
            "-",
            new DateTime(3000, 1, 1),
            CustomerReff.Default,
            LocationReff.Default,
            FakturReff.Default,
            DriverReff.Default,
            new DateTime(3000,1,1),
            "-",
            string.Empty,
            new DateTime(3000,1,1),
            string.Empty,
            string.Empty,
            Enumerable.Empty<PackingOrderItemModel>());

        public static IPackingOrderKey Key(string id)
        {
            var result = Default;
            result.PackingOrderId = id;
            return result;
        }

        public string PackingOrderId { get; private set; }
        public DateTime PackingOrderDate { get; private set; }
        public CustomerReff Customer { get; private set; }
        public LocationReff Location { get; private set; }
        public FakturReff Faktur { get; private set; }
        public DriverReff Driver { get; private set; } 
        public DateTime DownloadTimestamp { get; private set; }
        public string OfficeCode { get; private set;  }
        public string PrintLogId { get; private set;  }
        public DateTime PrintTimestamp { get; private set; }
        public string DocType { get; private set; }
        public string Note { get; private set; }
        
        public void PrintLogFaktur(PrintLogType printLog)
        {
            if (printLog.DocType != "PER-FAKTUR")
                return;
            PrintLogId = printLog.PrintLogId;
        }
        public void PrintLogBrg(PrintLogType printLog) //, string brgId)
        {
            if (printLog.DocType != "PER-SUPPLIER")
                return;

            PrintLogId = printLog.PrintLogId;

            //foreach (var item in _listItem)
            //  if (item.Brg.BrgId == brgId)
            //    item.PrintLogBrg(printLog);

        }

        public IEnumerable<PackingOrderItemModel> ListItem => _listItem;
    }

    public interface IPackingOrderKey
    {
        string PackingOrderId { get; }
    }
}
