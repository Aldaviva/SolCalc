namespace Tests;

public class ExtensionsTest {

    private static readonly DateTimeZone LosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];

    [Theory]
    [MemberData(nameof(AbsTimespanData))]
    public void AbsTimespan(Duration input, Duration expected) {
        input.Abs().Should().Be(expected);
    }

    public static TheoryData<Duration, Duration> AbsTimespanData => new() {
        { Duration.FromHours(1), Duration.FromHours(1) },
        { Duration.Zero, Duration.Zero },
        { Duration.FromSeconds(-1), Duration.FromSeconds(1) }
    };

    [Theory]
    [MemberData(nameof(AtStartOfDayData))]
    public void AtStartOfDay(ZonedDateTime input, ZonedDateTime expected) {
        input.AtStartOfDay().Should().Be(expected);
    }

    public static TheoryData<ZonedDateTime, ZonedDateTime> AtStartOfDayData => new() {
        { ZoneDateTime(LosAngeles, 2024, 4, 22, 12 + 7, 46), ZoneDateTime(LosAngeles, 2024, 4, 22, 0, 0) },
        { ZoneDateTime(LosAngeles, 2024, 4, 22, 0, 0), ZoneDateTime(LosAngeles, 2024, 4, 22, 0, 0) },
        { ZoneDateTime(LosAngeles, 2024, 3, 10, 3, 0), ZoneDateTime(LosAngeles, 2024, 3, 10, 0, 0) },
        { ZoneDateTime(LosAngeles, 2024, 11, 3, 0, 0), ZoneDateTime(LosAngeles, 2024, 11, 3, 0, 0) },
    };

    [Theory]
    [InlineData(SunlightLevel.Night, true, SolarTimeOfDay.AstronomicalDusk)]
    [InlineData(SunlightLevel.Night, false, SolarTimeOfDay.AstronomicalDusk)]
    [InlineData(SunlightLevel.AstronomicalTwilight, true, SolarTimeOfDay.AstronomicalDawn)]
    [InlineData(SunlightLevel.AstronomicalTwilight, false, SolarTimeOfDay.NauticalDusk)]
    [InlineData(SunlightLevel.NauticalTwilight, true, SolarTimeOfDay.NauticalDawn)]
    [InlineData(SunlightLevel.NauticalTwilight, false, SolarTimeOfDay.CivilDusk)]
    [InlineData(SunlightLevel.CivilTwilight, true, SolarTimeOfDay.CivilDawn)]
    [InlineData(SunlightLevel.CivilTwilight, false, SolarTimeOfDay.Sunset)]
    [InlineData(SunlightLevel.Daylight, true, SolarTimeOfDay.Sunrise)]
    [InlineData(SunlightLevel.Daylight, false, SolarTimeOfDay.Sunrise)]
    public void GetStart(SunlightLevel level, bool isSunRising, SolarTimeOfDay expected) {
        level.GetStart(isSunRising).Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(FirstOrNullData))]
    public void FirstOrNull(IEnumerable<SolarTimeOfDay> sequence, SolarTimeOfDay? expected) {
        sequence.FirstOrNull().Should().Be(expected);
    }

    public static TheoryData<IEnumerable<SolarTimeOfDay>, SolarTimeOfDay?> FirstOrNullData => new() {
        { new List<SolarTimeOfDay>(), null },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunrise }, SolarTimeOfDay.Sunrise },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunrise, SolarTimeOfDay.Sunset }, SolarTimeOfDay.Sunrise },
        { new[] { SolarTimeOfDay.AstronomicalDusk }.Where(day => day != SolarTimeOfDay.AstronomicalDusk), null },
        { new[] { SolarTimeOfDay.Sunrise }.Select(Identity), SolarTimeOfDay.Sunrise },
        { new[] { SolarTimeOfDay.Sunrise, SolarTimeOfDay.Sunset }.Select(Identity), SolarTimeOfDay.Sunrise },
    };

    private static T Identity<T>(T input) => input;

    [Theory]
    [MemberData(nameof(FirstOrNullFilteredData))]
    public void FirstOrNullFiltered(IEnumerable<SolarTimeOfDay> sequence, SolarTimeOfDay? expected) {
        sequence.FirstOrNull(timeOfDay => timeOfDay is SolarTimeOfDay.Sunrise or SolarTimeOfDay.CivilDusk).Should().Be(expected);
    }

    public static TheoryData<IEnumerable<SolarTimeOfDay>, SolarTimeOfDay?> FirstOrNullFilteredData => new() {
        { Enumerable.Empty<SolarTimeOfDay>(), null },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunrise }, SolarTimeOfDay.Sunrise },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunset }, null },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunset, SolarTimeOfDay.Sunrise }, SolarTimeOfDay.Sunrise },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunrise, SolarTimeOfDay.Sunset }, SolarTimeOfDay.Sunrise },
        { new List<SolarTimeOfDay> { SolarTimeOfDay.Sunrise, SolarTimeOfDay.CivilDusk }, SolarTimeOfDay.Sunrise },
    };

}