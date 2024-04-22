namespace Tests;

#pragma warning disable xUnit1000 // Test classes must be public

[Obsolete]
internal class SunlightCalculatorIterationTest {

    private static readonly DateTimeZone LosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
    private static readonly DateTimeZone Berlin     = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    // precision is worse at latitudes farther from the equator because the reference values from timeanddate.com don't seem to take atmospheric refraction of sunlight into account, but the NOAA code under test does
    private static readonly Duration TimeanddateDotComPrecision = Duration.FromMinutes(7);

    [Theory]
    [MemberData(nameof(GetDailySunlightChangesData))]
    public void GetDailySunlightChanges(LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightChange> expecteds) {
        IList<SunlightChange> actuals = SunlightCalculatorIteration.ListSunlightChanges(date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightChange actual   = actuals[i];
            SunlightChange expected = expecteds[i];

            actual.Time.Date.Should().Be(date, "within requested day");
            actual.Time.Should().BeCloseTo(expected.Time, TimeanddateDotComPrecision, "event {0} time", i);
            actual.NewSunlightLevel.Should().Be(expected.NewSunlightLevel, "event {0} lightPeriod", i);
            actual.IsSunRising.Should().Be(expected.IsSunRising, "event {0} isDawn", i);
        }
    }

    public static readonly TheoryData<LocalDate, DateTimeZone, double, double, IList<SunlightChange>> GetDailySunlightChangesData = new() {
        {
            new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, new List<SunlightChange> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 5, 46), SolarTimeOfDay.AstronomicalDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 17), SolarTimeOfDay.NauticalDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 7, 17), SolarTimeOfDay.Sunrise),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 22), SolarTimeOfDay.Sunset),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 21), SolarTimeOfDay.NauticalDusk),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 52), SolarTimeOfDay.AstronomicalDusk)
            }
        }, {
            new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, new List<SunlightChange> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 5, 8), SolarTimeOfDay.Sunrise),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 9, 3), SolarTimeOfDay.Sunset),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk)
            }
        }, {
            new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

    [Theory]
    [MemberData(nameof(GetDailySunlightIntervalsData))]
    public void GetDailySunlightIntervals(LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightInterval> expecteds) {
        IList<SunlightInterval> actuals = SunlightCalculatorIteration.ListSunlightIntervals(date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightInterval actual   = actuals[i];
            SunlightInterval expected = expecteds[i];

            actual.Start.Date.Should().Be(date, "within requested day");
            actual.End.Date.Should().Be(i == actuals.Count - 1 ? date.PlusDays(1) : date, "within requested day");

            actual.Start.Should().BeCloseTo(expected.Start, TimeanddateDotComPrecision, "interval at index {0}, start of {1}", i, expected.LightLevel);
            actual.End.Should().BeCloseTo(expected.End, TimeanddateDotComPrecision, "interval at index {0}, end of {1}", i, expected.LightLevel);
            actual.Interval.Start.Should().Be(actual.Start.ToInstant());
            actual.Interval.End.Should().Be(actual.End.ToInstant());
            actual.LightLevel.Should().Be(expected.LightLevel, "interval at index {0}", i);
        }

        actuals.Last().End.TimeOfDay.Should().Be(LocalTime.Midnight);
    }

    public static readonly TheoryData<LocalDate, DateTimeZone, double, double, IList<SunlightInterval>> GetDailySunlightIntervalsData = new() {
        {
            new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, new List<SunlightInterval> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 0, 0), null, ZoneDateTime(LosAngeles, 2024, 1, 23, 5, 46), SolarTimeOfDay.AstronomicalDawn, SunlightLevel.Night),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 5, 46), SolarTimeOfDay.AstronomicalDawn, ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 17), SolarTimeOfDay.NauticalDawn,
                    SunlightLevel.AstronomicalTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 17), SolarTimeOfDay.NauticalDawn, ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn, SunlightLevel.NauticalTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn, ZoneDateTime(LosAngeles, 2024, 1, 23, 7, 17), SolarTimeOfDay.Sunrise, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 7, 17), SolarTimeOfDay.Sunrise, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 22), SolarTimeOfDay.Sunset, SunlightLevel.Daylight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 22), SolarTimeOfDay.Sunset, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 21), SolarTimeOfDay.NauticalDusk,
                    SunlightLevel.NauticalTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 21), SolarTimeOfDay.NauticalDusk, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 52), SolarTimeOfDay.AstronomicalDusk,
                    SunlightLevel.AstronomicalTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 6, 52), SolarTimeOfDay.AstronomicalDusk, ZoneDateTime(LosAngeles, 2024, 1, 24, 0, 0), null, SunlightLevel.Night)
            }
        }, {
            new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, new List<SunlightInterval> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 0), null, ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk, ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn, SunlightLevel.NauticalTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn, ZoneDateTime(Berlin, 2024, 9, 10, 5, 8), SolarTimeOfDay.Sunrise, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 5, 8), SolarTimeOfDay.Sunrise, ZoneDateTime(Berlin, 2024, 9, 10, 12 + 9, 3), SolarTimeOfDay.Sunset, SunlightLevel.Daylight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 9, 3), SolarTimeOfDay.Sunset, ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk, ZoneDateTime(Berlin, 2024, 9, 11, 0, 0), null, SunlightLevel.NauticalTwilight)
            }
        }, {
            new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, new[] {
                new SunlightInterval(ZoneDateTime(Berlin, 2024, 4, 17, 0, 0), null, ZoneDateTime(Berlin, 2024, 4, 18, 0, 0), null, SunlightLevel.Daylight)
            }
        }
    };

    [Theory]
    [MemberData(nameof(ListSunlightStartTimesData))]
    public void GetStartTimes(SunlightLevel level, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<ZonedDateTime> expecteds) {
        IList<ZonedDateTime> actuals = SunlightCalculatorIteration.ListSunlightLevelBeginnings(level, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            ZonedDateTime actual   = actuals[i];
            ZonedDateTime expected = expecteds[i];

            actual.Should().BeCloseTo(expected, TimeanddateDotComPrecision, "index {0}", i);
        }
    }

    public static readonly TheoryData<SunlightLevel, LocalDate, DateTimeZone, double, double, IList<ZonedDateTime>> ListSunlightStartTimesData = new() {
        {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, [
                ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49),
                ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 22)
            ]
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, [
                ZoneDateTime(Berlin, 2024, 9, 10, 1, 58),
                ZoneDateTime(Berlin, 2024, 9, 10, 12 + 9, 3)
            ]
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

    [Theory]
    [MemberData(nameof(ListSunlightEndTimesData))]
    public void GetEndTimes(SunlightLevel level, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<ZonedDateTime> expecteds) {
        IList<ZonedDateTime> actuals = SunlightCalculatorIteration.ListSunlightLevelEnds(level, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            ZonedDateTime actual   = actuals[i];
            ZonedDateTime expected = expecteds[i];

            actual.Should().BeCloseTo(expected, TimeanddateDotComPrecision, "index {0}", i);
        }
    }

    public static readonly TheoryData<SunlightLevel, LocalDate, DateTimeZone, double, double, IList<ZonedDateTime>> ListSunlightEndTimesData = new() {
        {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, [
                ZoneDateTime(LosAngeles, 2024, 1, 23, 7, 17),
                ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50)
            ]
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, [
                ZoneDateTime(Berlin, 2024, 9, 10, 0, 22),
                ZoneDateTime(Berlin, 2024, 9, 10, 5, 8),
                ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53)
            ]
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

    [Theory]
    [MemberData(nameof(GetTimesOfDayData))]
    public void GetTimesOfDay(SolarTimeOfDay solarTimeOfDay, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<ZonedDateTime> expecteds) {
        IList<ZonedDateTime> actuals = SunlightCalculatorIteration.ListSolarEventTimes(solarTimeOfDay, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            ZonedDateTime actual   = actuals[i];
            ZonedDateTime expected = expecteds[i];

            actual.Should().BeCloseTo(expected, TimeanddateDotComPrecision, "index {0}", i);
        }
    }

    public static readonly TheoryData<SolarTimeOfDay, LocalDate, DateTimeZone, double, double, IList<ZonedDateTime>> GetTimesOfDayData = new() {
        {
            SolarTimeOfDay.CivilDawn, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, [
                ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49)
            ]
        }, {
            SolarTimeOfDay.CivilDusk, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, [
                ZoneDateTime(Berlin, 2024, 9, 10, 0, 22),
                ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53)
            ]
        }, {
            SolarTimeOfDay.CivilDusk, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

    [Theory]
    [MemberData(nameof(ListSunlightIntervalsWithExactLevelData))]
    public void ListSunlightIntervalsWithExactLevel(SunlightLevel minimumSunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightInterval> expecteds) {
        List<SunlightInterval> actuals = SunlightCalculatorIteration.ListSunlightIntervalsWithLevelExactly(minimumSunlight, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightInterval actual   = actuals[i];
            SunlightInterval expected = expecteds[i];

            actual.LightLevel.Should().Be(minimumSunlight, "index {0}", i);
            actual.Interval.Should().Be(new Interval(actual.Start.ToInstant(), actual.End.ToInstant()), "index {0}", i);
            actual.Start.Should().BeCloseTo(expected.Start, TimeanddateDotComPrecision, "index {0}", i);
            actual.End.Should().BeCloseTo(expected.End, TimeanddateDotComPrecision, "index {0}", i);
            actual.StartName.Should().Be(expected.StartName, "index {0}", i);
            actual.EndName.Should().Be(expected.EndName, "index {0}", i);
        }
    }

    public static readonly TheoryData<SunlightLevel, LocalDate, DateTimeZone, double, double, IList<SunlightInterval>> ListSunlightIntervalsWithExactLevelData = new() {
        {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, new List<SunlightInterval> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn, ZoneDateTime(LosAngeles, 2024, 1, 23, 7, 17), SolarTimeOfDay.Sunrise, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 22), SolarTimeOfDay.Sunset, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, new List<SunlightInterval> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 0), null, ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn, ZoneDateTime(Berlin, 2024, 9, 10, 5, 8), SolarTimeOfDay.Sunrise, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 9, 3), SolarTimeOfDay.Sunset, ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

    [Theory]
    [MemberData(nameof(ListSunlightIntervalsWithLevelBrighterOrEqualToData))]
    public void GetIntervalsOfSunlightBrighterOrEqualTo(SunlightLevel minimumSunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightInterval> expecteds) {
        List<SunlightInterval> actuals = SunlightCalculatorIteration.ListSunlightIntervalsWithLevelBrighterOrEqualTo(minimumSunlight, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightInterval actual   = actuals[i];
            SunlightInterval expected = expecteds[i];

            actual.LightLevel.Should().Be(minimumSunlight, "index {0}", i);
            actual.Interval.Should().Be(new Interval(actual.Start.ToInstant(), actual.End.ToInstant()), "index {0}", i);
            actual.Start.Should().BeCloseTo(expected.Start, TimeanddateDotComPrecision, "index {0}", i);
            actual.End.Should().BeCloseTo(expected.End, TimeanddateDotComPrecision, "index {0}", i);
            actual.StartName.Should().Be(expected.StartName, "index {0}", i);
            actual.EndName.Should().Be(expected.EndName, "index {0}", i);
        }
    }

    public static readonly TheoryData<SunlightLevel, LocalDate, DateTimeZone, double, double, IList<SunlightInterval>> ListSunlightIntervalsWithLevelBrighterOrEqualToData = new() {
        {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, new List<SunlightInterval> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn, ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight)
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, new List<SunlightInterval> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 0), null, ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn, ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk, SunlightLevel.CivilTwilight)
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, new List<SunlightInterval> {
                new(ZoneDateTime(Berlin, 2024, 4, 17, 0, 0), null, ZoneDateTime(Berlin, 2024, 4, 18, 0, 0), null, SunlightLevel.Daylight)
            }
        }
    };

    [Theory]
    [MemberData(nameof(GetSunlightAtData))]
    public void GetSunlightAt(ZonedDateTime time, double latitude, double longitude, SunlightLevel expected) {
        SunlightCalculatorIteration.GetSunlightAt(time, latitude, longitude).Should().Be(expected);
    }

    public static readonly TheoryData<ZonedDateTime, double, double, SunlightLevel> GetSunlightAtData = new() {
        { ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 0), 37.77, -122.42, SunlightLevel.AstronomicalTwilight }
    };

    [Theory]
    [MemberData(nameof(GetSunlightForSolarElevationData))]
    public void GetSunlightForSolarElevation(double elevation, SunlightLevel expected) {
        SunlightCalculatorIteration.GetSunlightForSolarElevation(elevation).Should().Be(expected);
    }

    public static readonly TheoryData<double, SunlightLevel> GetSunlightForSolarElevationData = new() {
        { -19, SunlightLevel.Night },
        { -15.87, SunlightLevel.AstronomicalTwilight },
        { -10, SunlightLevel.NauticalTwilight },
        { -4, SunlightLevel.CivilTwilight },
        { 33, SunlightLevel.Daylight }
    };

    [Theory]
    [MemberData(nameof(ListSunlightChangesBrighterOrEqualToData))]
    public void ListSunlightChangesBrighterOrEqualTo(SunlightLevel minLevel, LocalDate date, DateTimeZone zone, double latitude, double longitude, IList<SunlightChange> expecteds) {
        List<SunlightChange> actuals = SunlightCalculatorIteration.ListSunlightChangesRisingIntoOrSettingOutOf(minLevel, date, zone, latitude, longitude).ToList();
        actuals.Should().HaveSameCount(expecteds);

        for (int i = 0; i < actuals.Count; i++) {
            SunlightChange actual   = actuals[i];
            SunlightChange expected = expecteds[i];

            actual.Time.Date.Should().Be(date, "within requested day");
            actual.Time.Should().BeCloseTo(expected.Time, TimeanddateDotComPrecision, "event {0} time", i);
            actual.NewSunlightLevel.Should().Be(expected.NewSunlightLevel, "event {0} lightPeriod", i);
            actual.IsSunRising.Should().Be(expected.IsSunRising, "event {0} isDawn", i);
        }
    }

    public static readonly TheoryData<SunlightLevel, LocalDate, DateTimeZone, double, double, IList<SunlightChange>> ListSunlightChangesBrighterOrEqualToData = new() {
        {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 1, 23), LosAngeles, 37.35, -121.95, new List<SunlightChange> {
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 6, 49), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(LosAngeles, 2024, 1, 23, 12 + 5, 50), SolarTimeOfDay.CivilDusk)
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 9, 10), Berlin, 78.92, 11.93, new List<SunlightChange> {
                new(ZoneDateTime(Berlin, 2024, 9, 10, 0, 22), SolarTimeOfDay.CivilDusk),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 1, 58), SolarTimeOfDay.CivilDawn),
                new(ZoneDateTime(Berlin, 2024, 9, 10, 12 + 11, 53), SolarTimeOfDay.CivilDusk)
            }
        }, {
            SunlightLevel.CivilTwilight, new LocalDate(2024, 4, 17), Berlin, 78.92, 11.93, []
        }
    };

}