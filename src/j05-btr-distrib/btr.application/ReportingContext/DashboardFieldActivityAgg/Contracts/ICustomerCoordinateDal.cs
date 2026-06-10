using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts
{
    public interface ICustomerCoordinateDal
    {
        IReadOnlyDictionary<string, CustomerCoordinateRow> ListByCustomerIds(
            IEnumerable<string> customerIds);
    }

    public class CustomerCoordinateRow
    {
        public string CustomerId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
