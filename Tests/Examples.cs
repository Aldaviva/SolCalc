/*/

using NodaTime.Extensions;
using Xunit.Abstractions;

namespace Tests;

public class Examples(ITestOutputHelper testOutputHelper) {

    private ITestOutputHelper Console { get; } = testOutputHelper;

    public void GetSunriseAndSunsetForDay() {
        ZonedDateTime now = SystemClock.Instance.InTzdbSystemDefaultZone().GetCurrentZonedDateTime();
        IEnumerable<SunlightChange> dailySunriseAndSunset = SunlightCalculator.GetSunlightChanges(now, 37.35, -121.95)
            .TakeWhile(s => s.Time.Date.Equals(now.Date)).ToList();

        ZonedDateTime? sunrise = dailySunriseAndSunset.FirstOrNull(s => s.Name == SolarTimeOfDay.Sunrise)?.Time;
        ZonedDateTime? sunset  = dailySunriseAndSunset.FirstOrNull(s => s.Name == SolarTimeOfDay.Sunset)?.Time;
    }

    public void GetSunlightNow() {
        DateTimeZone  zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        ZonedDateTime now  = SystemClock.Instance.InZone(zone).GetCurrentZonedDateTime();

        SunlightLevel level = SunlightCalculator.GetSunlightAt(now, latitude: 37.35, longitude: -121.95);
        System.Console.WriteLine($"It is currently {level} in Santa Clara, CA, US.");
    }

    [Fact]
    public void GetNextSunlightChange() {
        DateTimeZone  zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        ZonedDateTime now  = SystemClock.Instance.InZone(zone).GetCurrentZonedDateTime();

        SunlightChange nextChange = SunlightCalculator.GetSunlightChanges(now, latitude: 37.35, longitude: -121.95).First();
        Console.WriteLine($"At {nextChange.Time}, {nextChange.Name} will start when {nextChange.PreviousSunlightLevel} changes to {nextChange.NewSunlightLevel} in Santa Clara, CA, US.");
        // At 2024-04-22T04:48:01 America/Los_Angeles (-07), AstronomicalDawn will start when Night changes to AstronomicalTwilight in Santa Clara, CA, US.
    }

    [Fact]
    public async Task WaitForSunlightLevelChanges() {
        DateTimeZone zone  = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        ZonedClock   clock = SystemClock.Instance.InZone(zone);

        IEnumerable<SunlightChange> sunlightChanges = SunlightCalculator.GetSunlightChanges(clock.GetCurrentZonedDateTime(), 37.35, -121.95);
        foreach (SunlightChange sunlightChange in sunlightChanges) {
            Duration delay = sunlightChange.Time.Minus(clock.GetCurrentZonedDateTime());
            Console.WriteLine($"Waiting {delay} for {sunlightChange.Name} at {sunlightChange.Time}");
            await Task.Delay(delay.ToTimeSpan());

            Console.WriteLine($"It is currently {sunlightChange.Name} at {clock.GetCurrentZonedDateTime()} in Santa Clara, CA, US");
        }
    }

}
/**/

