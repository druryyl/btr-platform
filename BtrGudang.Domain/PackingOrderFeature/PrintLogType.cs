using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Domain.PackingOrderFeature
{
    public class PrintLogType : IPrintLogKey
    {
        private readonly List<PrintLogPackingOrderType> _listPackingOrder;

        public PrintLogType(string printLogId, DateTime printLogTimestamp, string docType, 
            IEnumerable<PrintLogPackingOrderType> listPackingOrder)
        {
            PrintLogId = printLogId;
            PrintLogTimestamp = printLogTimestamp;
            DocType = docType;
            _listPackingOrder = listPackingOrder?.ToList() ?? new List<PrintLogPackingOrderType>();
        }

        public static PrintLogType Create(string docType, IEnumerable<PackingOrderModel> listPackingOrder)
        {
            var newId = Ulid.NewUlid().ToString();
            var dateTime = DateTime.Now;
            var listDetil = listPackingOrder.Select(x => new PrintLogPackingOrderType(x.PackingOrderId, x.Faktur.FakturId));
            var result = new PrintLogType(newId, dateTime, docType, listDetil);
            return result;
        }

        public static IPrintLogKey Key(string id)
        {
            var result = new PrintLogType(id, new DateTime(3000,1,1), "", new List<PrintLogPackingOrderType>());
            return result;
        }

        public string PrintLogId { get; private set; }
        public DateTime PrintLogTimestamp { get; private set;  }
        public string DocType { get; private set;  }
        public IEnumerable<PrintLogPackingOrderType> ListPackingOrder => _listPackingOrder;

    }

    public interface IPrintLogKey
    {
        string PrintLogId { get; }
    }

    public class PrintLogPackingOrderType
    {
        public PrintLogPackingOrderType(string packingOrderId, string fakturId)
        {
            PackingOrderId = packingOrderId;
            FakturId = fakturId;
        }
        public string PackingOrderId { get; private set; }
        public string FakturId { get; private set;  }
    }
}
