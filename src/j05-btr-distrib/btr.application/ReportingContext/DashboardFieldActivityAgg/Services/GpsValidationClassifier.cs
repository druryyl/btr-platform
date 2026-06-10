using System;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;

namespace btr.application.ReportingContext.DashboardFieldActivityAgg.Services
{
    public static class GpsValidationClassifier
    {
        private const double EarthRadiusMeters = 6371000;

        public static GpsValidationClass Classify(
            double checkInLat, double checkInLng,
            double customerLat, double customerLng,
            float accuracy)
        {
            if (IsZeroCoord(checkInLat, checkInLng) && IsZeroCoord(customerLat, customerLng))
                return GpsValidationClass.Invalid;

            if (IsZeroCoord(checkInLat, checkInLng) || IsZeroCoord(customerLat, customerLng))
                return GpsValidationClass.Suspicious;

            var distanceMeters = HaversineMeters(checkInLat, checkInLng, customerLat, customerLng);

            if (distanceMeters > 100 || accuracy > 50)
                return GpsValidationClass.Suspicious;

            if (distanceMeters > 50 || accuracy > 30)
                return GpsValidationClass.Warning;

            return GpsValidationClass.Valid;
        }

        public static double? DistanceMeters(
            double checkInLat, double checkInLng,
            double customerLat, double customerLng)
        {
            if (IsZeroCoord(checkInLat, checkInLng) || IsZeroCoord(customerLat, customerLng))
                return null;

            return HaversineMeters(checkInLat, checkInLng, customerLat, customerLng);
        }

        public static double HaversineMeters(
            double lat1, double lng1,
            double lat2, double lng2)
        {
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lng2 - lng1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusMeters * c;
        }

        public static bool HasNonZeroCoordinates(double latitude, double longitude)
        {
            return !IsZeroCoord(latitude, longitude);
        }

        private static bool IsZeroCoord(double latitude, double longitude)
        {
            return Math.Abs(latitude) < 0.000001 && Math.Abs(longitude) < 0.000001;
        }
    }
}
