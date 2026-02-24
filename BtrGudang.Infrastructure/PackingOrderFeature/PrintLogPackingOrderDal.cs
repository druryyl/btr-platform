using Bilreg.Infrastructure.Shared.Helpers;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Winform.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BtrGudang.Infrastructure.PackingOrderFeature
{
    public interface IPrintLogPackingOrderDal :
        IInsertBulk<PrintLogPackingOrderDto>,
        IDelete<IPrintLogKey>,
        IListData<PrintLogPackingOrderDto, IPrintLogKey>
    {
    }
    public class PrintLogPackingOrderDal : IPrintLogPackingOrderDal
    {
        private readonly DatabaseOptions _opt;

        public PrintLogPackingOrderDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(IEnumerable<PrintLogPackingOrderDto> listDto)
        {
            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            using (var bcp = new SqlBulkCopy(conn))
            {
                conn.Open();
                bcp.AddMap("PrintLogId", "PrintLogId");
                bcp.AddMap("PackingOrderId", "PackingOrderId");
                bcp.AddMap("FakturId", "FakturId");

                var fetched = listDto.ToList();
                bcp.BatchSize = fetched.Count;
                bcp.DestinationTableName = "BTRG_PrintLogPackingOrder";
                bcp.WriteToServer(fetched.AsDataTable());
            }
        }

        public void Delete(IPrintLogKey key)
        {
            const string sql = @"
                DELETE FROM BTRG_PrintLogPackingOrder
                WHERE PrintLogId = @PrintLogId ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", key.PrintLogId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }

        }

        public IEnumerable<PrintLogPackingOrderDto> ListData(IPrintLogKey filter)
        {
            const string sql = @"
                SELECT PrintLogId, PackingOrderId, FakturId
                FROM BTRG_PrintLogPackingOrder
                WHERE PrintLogId = @PrintLogId ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", filter.PrintLogId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PrintLogPackingOrderDto>(sql, dp);
            }
        }
    }

    public class PrintLogPackingOrderDto
    {
        public PrintLogPackingOrderDto(string printLogId, 
            string packingOrderId, string fakturId)
        {
            PrintLogId = printLogId;
            PackingOrderId = packingOrderId;
            FakturId = fakturId;
        }

        public PrintLogPackingOrderType ToModel()
        {
            return new PrintLogPackingOrderType(PackingOrderId, FakturId);
        }
        public static PrintLogPackingOrderDto FromModel(PrintLogPackingOrderType model, IPrintLogKey key)
        {
            var result = new PrintLogPackingOrderDto(key.PrintLogId, model.PackingOrderId, model.FakturId);
            return result;
        }

        public string PrintLogId { get; private set;  }
        public string PackingOrderId { get; private set; }
        public string FakturId { get; private set; }
    }
}
