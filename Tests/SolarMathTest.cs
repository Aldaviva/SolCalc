namespace Tests;

public class SolarMathTest {

    private static readonly DateTimeZone LosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];

    [Theory]
    [MemberData(nameof(ElevationData))]
    public void solarPosition(ZonedDateTime time, double latitude, double longitude, decimal expectedAzimuthDegrees, decimal expectedElevationDegrees, decimal expectedDeclination) {
        const decimal tolerance = 0.01m;
        (decimal azimuth, decimal elevation, decimal declination) = SolarCalculator.SolarPosition(time, latitude, longitude);
        azimuth.Should().BeApproximately(expectedAzimuthDegrees, tolerance);
        elevation.Should().BeApproximately(expectedElevationDegrees, tolerance);
        declination.Should().BeApproximately(expectedDeclination, tolerance);

        SolarCalculator.SolarAzimuth(time, latitude, longitude).Should().BeApproximately(expectedAzimuthDegrees, tolerance);
        SolarCalculator.SolarElevation(time, latitude, longitude).Should().BeApproximately(expectedElevationDegrees, tolerance);
    }

    public static readonly TheoryData<ZonedDateTime, double, double, decimal, decimal, decimal> ElevationData = new() {
        { ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 0), 37.77, -122.42, 102.59m, -15.87m, -19.48m }
    };

    [Theory]
    [MemberData(nameof(SolarNoonData))]
    public void solarNoon(LocalDate date, DateTimeZone zone, double longitude, ZonedDateTime expected) {
        Duration tolerance = Duration.FromSeconds(1);
        SolarCalculator.SolarNoon(date, zone, longitude).Should().BeCloseTo(expected, tolerance);
    }

    public static readonly TheoryData<LocalDate, DateTimeZone, double, ZonedDateTime> SolarNoonData = new() {
        { new LocalDate(2024, 1, 25), LosAngeles, -122.42, new LocalDateTime(2024, 1, 25, 12, 21, 51).InZoneStrictly(LosAngeles) }
    };

}