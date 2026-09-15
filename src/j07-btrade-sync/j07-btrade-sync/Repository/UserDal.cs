using Dapper;
using j07_btrade_sync.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Repository
{
    public class UserDal
    {
        public IEnumerable<UserType> ListData()
        {
            //  IR-05 — the credential projection replicates BTR_User verbatim.
            //  BTR_User has no active flag, so every replicated account is
            //  active; ServerId is assigned by the Cloud from the JWT (ADR-007).
            const string sql = @"
                SELECT
                    UserId, UserName, Password, RoleId,
                    CAST(1 AS BIT) AS IsAktif,
                    '' AS ServerId
                FROM
                    BTR_User ";

            using (var conn = new SqlConnection(ConnStringHelper.Get()))
            {
                var result = conn.Query<UserType>(sql).ToList();
                return result;
            }
        }
    }
}
