using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Contracts
{
    public interface IFieldActivityCheckInDal
    {
        IReadOnlyList<FieldActivityCheckInRow> ListBySalesPersonDate(
            string salesPersonEmail, DateTime visitDate);
    }

    public class FieldActivityCheckInRow
    {
        public string CheckInId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CheckInTime { get; set; }
        public double CheckInLatitude { get; set; }
        public double CheckInLongitude { get; set; }
        public double CustomerLatitude { get; set; }
        public double CustomerLongitude { get; set; }
        public float Accuracy { get; set; }
    }
}
