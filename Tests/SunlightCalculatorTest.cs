namespace Tests;

public class SunlightCalculatorTest {

    private static readonly DateTimeZone LosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
    private static readonly DateTimeZone Berlin     = DateTimeZoneProviders.Tzdb["Europe/Berlin"];
    private static readonly Duration     Tolerance  = Duration.FromMinutes(2);

    [Theory]
    [MemberData(nameof(GetNextSunlightLevelChangeData))]
    public void GetNextSunlightLevelChange(ZonedDateTime start, double latitude, double longitude, ZonedDateTime expectedTime, SolarTimeOfDay expectedSolarTime) {
        (ZonedDateTime actualTime, SolarTimeOfDay actualSolarTime) = SunlightCalculator.GetNextSunlightChange(start, latitude, longitude);

        actualSolarTime.Should().Be(expectedSolarTime);
        actualTime.Should().BeCloseTo(expectedTime, Tolerance);
    }

    public static readonly TheoryData<ZonedDateTime, double, double, ZonedDateTime, SolarTimeOfDay> GetNextSunlightLevelChangeData = new() {
        { ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 2, 5), 37.35, -121.95, ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 5, 27), SolarTimeOfDay.Sunset }
    };

    [Theory]
    [MemberData(nameof(GetAllSunlightLevelChangesForDayData))]
    public void GetAllSunlightLevelChangesForDay(ZonedDateTime start, double latitude, double longitude, IList<SunlightChange> expecteds) {
        List<SunlightChange> actuals = SunlightCalculator.GetSunlightChanges(start, latitude, longitude).TakeWhile(change => change.Time.Date == start.Date).ToList();

        actuals.Should().HaveSameCount(expecteds);
        for (int i = 0; i < actuals.Count; i++) {
            (ZonedDateTime actualTime, SolarTimeOfDay actualSolarTime)     = actuals[i];
            (ZonedDateTime expectedTime, SolarTimeOfDay expectedSolarTime) = expecteds[i];

            actualSolarTime.Should().Be(expectedSolarTime, "index {0}", i);
            actualTime.Should().BeCloseTo(expectedTime, Tolerance, "index {0}", i);
        }
    }

    public static readonly TheoryData<ZonedDateTime, double, double, IList<SunlightChange>> GetAllSunlightLevelChangesForDayData = new() {
        {
            // Normal day in California
            ZoneDateTime(LosAngeles, 2024, 1, 28, 0, 0), 37.35, -121.95, new List<SunlightChange> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 5, 44), SolarTimeOfDay.AstronomicalDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 6, 15), SolarTimeOfDay.NauticalDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 6, 46), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 7, 14), SolarTimeOfDay.Sunrise),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 5, 27), SolarTimeOfDay.Sunset),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 5, 55), SolarTimeOfDay.CivilDusk),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 6, 26), SolarTimeOfDay.NauticalDusk),
                new(ZoneDateTime(LosAngeles, 2024, 1, 28, 12 + 6, 57), SolarTimeOfDay.AstronomicalDusk)
            }
        }, {
            // Civil dusk twice on the same day in Svalbard
            ZoneDateTime(Berlin, 2024, 9, 10, 0, 0), 78.92, 11.93, new List<SunlightChange> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 28), SolarTimeOfDay.CivilDusk),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 53), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 5, 15), SolarTimeOfDay.Sunrise),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 8, 58), SolarTimeOfDay.Sunset),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 57), SolarTimeOfDay.CivilDusk)
            }
        }, {
            // Polar night in Svalbard
            ZoneDateTime(Berlin, 2024, 4, 17, 0, 0), 78.92, 11.93, []
        }
    };

}