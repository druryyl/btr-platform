using btr.application.ReportingContext.DashboardFieldActivityAgg.Models;
using btr.application.ReportingContext.DashboardFieldActivityAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class GpsValidationClassifierTest
    {
        private const double BaseLat = -6.2;
        private const double BaseLng = 106.8;

        [Fact]
        public void Classify_WhenBothCoordsZero_ReturnsInvalid()
        {
            GpsValidationClassifier.Classify(0, 0, 0, 0, 10)
                .Should().Be(GpsValidationClass.Invalid);
        }

        [Fact]
        public void Classify_WhenOneCoordZero_ReturnsSuspicious()
        {
            GpsValidationClassifier.Classify(BaseLat, BaseLng, 0, 0, 10)
                .Should().Be(GpsValidationClass.Suspicious);
        }

        [Fact]
        public void Classify_WhenDistance40Meters_ReturnsValid()
        {
            var checkInLat = BaseLat + MetersToLatitudeDelta(40);
            GpsValidationClassifier.Classify(checkInLat, BaseLng, BaseLat, BaseLng, 10)
                .Should().Be(GpsValidationClass.Valid);
        }

        [Fact]
        public void Classify_WhenDistance75Meters_ReturnsWarning()
        {
            var checkInLat = BaseLat + MetersToLatitudeDelta(75);
            GpsValidationClassifier.Classify(checkInLat, BaseLng, BaseLat, BaseLng, 10)
                .Should().Be(GpsValidationClass.Warning);
        }

        [Fact]
        public void Classify_WhenDistance150Meters_ReturnsSuspicious()
        {
            var checkInLat = BaseLat + MetersToLatitudeDelta(150);
            GpsValidationClassifier.Classify(checkInLat, BaseLng, BaseLat, BaseLng, 10)
                .Should().Be(GpsValidationClass.Suspicious);
        }

        [Fact]
        public void Classify_WhenAccuracy31_ReturnsWarning()
        {
            GpsValidationClassifier.Classify(BaseLat, BaseLng, BaseLat, BaseLng, 31)
                .Should().Be(GpsValidationClass.Warning);
        }

        [Fact]
        public void Classify_WhenAccuracy51_ReturnsSuspicious()
        {
            GpsValidationClassifier.Classify(BaseLat, BaseLng, BaseLat, BaseLng, 51)
                .Should().Be(GpsValidationClass.Suspicious);
        }

        [Fact]
        public void Classify_WhenDistanceExactly50Meters_ReturnsValid()
        {
            var checkInLat = BaseLat + MetersToLatitudeDelta(50);
            GpsValidationClassifier.Classify(checkInLat, BaseLng, BaseLat, BaseLng, 10)
                .Should().Be(GpsValidationClass.Valid);
        }

        [Fact]
        public void Classify_WhenDistanceJustOver50Meters_ReturnsWarning()
        {
            var checkInLat = BaseLat + MetersToLatitudeDelta(50.1);
            GpsValidationClassifier.Classify(checkInLat, BaseLng, BaseLat, BaseLng, 10)
                .Should().Be(GpsValidationClass.Warning);
        }

        private static double MetersToLatitudeDelta(double meters)
        {
            return meters / 111_320d;
        }
    }
}
