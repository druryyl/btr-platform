using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.CheckInFeature
{
    public record CheckInType(
        string CheckInId,
        string CheckInDate,        // yyyy-MM-dd
        string CheckInTime,        // HH:mm:ss
        string UserEmail,
        double CheckInLatitude,
        double CheckInLongitude,
        double Accuracy,
        string CustomerId,
        string CustomerCode,
        string CustomerName,
        string CustomerAddress,
        double CustomerLatitude,
        double CustomerLongitude,
        string StatusSync,
        string ServerId,
        string CheckOutTime = "",
        double CheckOutLatitude = 0,
        double CheckOutLongitude = 0,
        double CheckOutAccuracy = 0,
        string CheckOutMode = ""
    ) : ICheckInKey;

    public interface ICheckInKey
    {
        string CheckInId { get; }
    }
}


