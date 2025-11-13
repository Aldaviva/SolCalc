namespace Tests;

public class ExtensionsTest {

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

}