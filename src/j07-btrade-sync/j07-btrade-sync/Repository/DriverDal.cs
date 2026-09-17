using Dapper;
using j07_btrade_sync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Repository
{
    public class DriverDal
    {
        public IEnumerable<DriverType> ListData()
        {
            const string sql = @"
                SELECT
                    aa.DriverId, aa.DriverName, aa.IsAktif, '     ' AS ServerId
                FROM
                    BTR_Driver aa";
            using (var conn = new System.Data.SqlClient.SqlConnection(ConnStringHelper.Get()))
            {
                var result = conn.Query<DriverType>(sql).ToList();
                return result;
            }
        }
    }
}
