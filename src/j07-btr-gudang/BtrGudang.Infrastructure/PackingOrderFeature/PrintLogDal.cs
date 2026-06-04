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

namespace BtrGudang.Infrastructure.PackingOrderFeature
{
    public interface IPrintLogDal :
        IInsert<PrintLogDto>,
        IUpdate<PrintLogDto>,
        IDelete<IPrintLogKey>,
        IGetData<PrintLogDto, IPrintLogKey>,
        IListData<PrintLogDto>
    {
    }

    public class PrintLogDal : IPrintLogDal
    {
        private readonly DatabaseOptions _opt;

        public PrintLogDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public void Insert(PrintLogDto dto)
        {
            const string sql = @"
               INSERT INTO BTRG_PrintLog(
                   PrintLogId, PrintLogTimestamp, DocType)
               VALUES( 
                   @PrintLogId, @PrintLogTimestamp, @DocType)
               ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", dto.PrintLogId, SqlDbType.VarChar);
            dp.AddParam("@PrintLogTimestamp", dto.PrintLogTimestamp, SqlDbType.DateTime);
            dp.AddParam("@DocType", dto.DocType, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Update(PrintLogDto dto)
        {
            const string sql = @"
                   UPDATE 
                       BTRG_PrintLog
                   SET
                       PrintLogTimestamp = @PrintLogTimestamp,
                       DocType = @DocType
                   WHERE
                       PrintLogId = @PrintLogId
                   ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", dto.PrintLogId, SqlDbType.VarChar);
            dp.AddParam("@PrintLogTimestamp", dto.PrintLogTimestamp, SqlDbType.DateTime);
            dp.AddParam("@DocType", dto.DocType, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public void Delete(IPrintLogKey key)
        {
            const string sql = @"
               DELETE FROM 
                    BTRG_PrintLog
               WHERE
                   PrintLogId = @PrintLogId
               ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", key.PrintLogId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, dp);
            }
        }

        public PrintLogDto GetData(IPrintLogKey key)
        {
           const string sql = @"
               SELECT
                   PrintLogId,
                   PrintLogTimestamp,
                   DocType
               FROM 
                   BTRG_PrintLog
               WHERE
                   PrintLogId = @PrintLogId
               ";

            var dp = new DynamicParameters();
            dp.AddParam("@PrintLogId", key.PrintLogId, SqlDbType.VarChar);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var result = conn.ReadSingle<PrintLogDto>(sql, dp);
                return result;
            }
        }

        public IEnumerable<PrintLogDto> ListData()
        {
            const string sql = @"
                SELECT
                    PrintLogId,
                    PrintLogTimestamp,
                    DocType
                FROM 
                    BTRG_PrintLog
                ";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Read<PrintLogDto>(sql);
            }
        }
    }

    public class PrintLogDto
    {
        public PrintLogDto(string printLogId, 
            DateTime printLogTimestamp, string docType)
        {
            PrintLogId = printLogId;
            PrintLogTimestamp = printLogTimestamp;
            DocType = docType;
        }

        public PrintLogType ToModel(IEnumerable<PrintLogPackingOrderType> listItem)
        {
            var result = new PrintLogType(PrintLogId, PrintLogTimestamp, DocType, listItem);
            return result;
        }

        public static PrintLogDto FromModel(PrintLogType model)
        {
            var result = new PrintLogDto(model.PrintLogId, model.PrintLogTimestamp, model.DocType);
            return result;
        }

        public string PrintLogId { get; private set; }
        public DateTime PrintLogTimestamp { get; private set; }
        public string DocType { get; private set; }
    }
}
