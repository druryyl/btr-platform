using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts;
using btr.infrastructure.Helpers;
using btr.nuna.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardFieldActivityAgg
{
    public class CustomerCoordinateDal : ICustomerCoordinateDal
    {
        private readonly DatabaseOptions _opt;

        public CustomerCoordinateDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public IReadOnlyDictionary<string, CustomerCoordinateRow> ListByCustomerIds(
            IEnumerable<string> customerIds)
        {
            var ids = (customerIds ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<string, CustomerCoordinateRow>();

            const string sql = @"
SELECT CustomerId, Latitude, Longitude
FROM BTR_Customer
WHERE CustomerId IN @CustomerIds";

            var dp = new DynamicParameters();
            dp.Add("@CustomerIds", ids);

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Read<CustomerCoordinateRow>(sql, dp) ?? Enumerable.Empty<CustomerCoordinateRow>();
                return rows.ToDictionary(x => x.CustomerId, x => x);
            }
        }
    }
}
