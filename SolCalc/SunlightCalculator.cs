using NodaTime;
using SolCalc.Data;
using static SolCalc.Math.DecimalMath;
using static SolCalc.SolarCalculator;

namespace SolCalc;

/// <summary>
/// <para>Perform calculations about the level of sunlight visible at different times of day for a given date and location on Earth.</para>
/// <para>This low-level class calculates the amount of sunlight (like dusk) and enumerate sunlight changes (like sunset). To instead calculate the sun's angle, use <see cref="SolarCalculator"/>.</para>
/// <para> </para>
/// <para>Based on the NOAA Earth System Research Laboratories' Global Monitoring Laboratory Solar Calculator (<see href="https://gml.noaa.gov/grad/solcalc/"/>).</para>
/// </summary>
/// <remarks>
/// <para>For accuracy, see <see cref="SolarCalculator"/> remarks.</para>
/// </remarks>
public static class SunlightCalculator {

    private const int MaxIterations  = 50;
    private const int PaddingMinutes = 1;

    /// <summary>
    /// degrees
    /// </summary>
    internal static decimal ElevationPrecision = 0.0001m;

    /// <summary>
    /// <para>Get the amount of sunlight visible at the given instant and location on Earth.</para>
    /// <para>Does not take weather, observer's altitude, or solar eclipses into account.</para>
    /// </summary>
    /// <param name="time">Time and date. The time zone must be the correct time zone at the given location.</param>
    /// <param name="latitude">Degrees north of the equator</param>
    /// <param name="longitude">Degrees east of the prime meridian</param>
    /// <returns>level of sunlight</returns>
    public static SunlightLevel GetSunlightAt(ZonedDateTime time, double latitude, double longitude) => GetSunlightForSolarElevation(SolarElevation(time, latitude, longitude));

    private static SunlightLevel GetSunlightForSolarElevation(decimal solarElevationDegrees) => solarElevationDegrees switch {
        >= 0   => SunlightLevel.Daylight,
        >= -6  => SunlightLevel.CivilTwilight,
        >= -12 => SunlightLevel.NauticalTwilight,
        >= -18 => SunlightLevel.AstronomicalTwilight,
        _      => SunlightLevel.Night
    };

    /// <summary>
    /// <para>Get an endless series of changes in the amount of sunlight at a given location on Earth, starting at a given instant.</para>
    /// <para>This will tell you all the times the sunlight changes between discrete levels, such as going from civil dusk to daylight at sunrise, or going from astronomical twilight to night at astronomical dusk.</para>
    /// <para> </para>
    /// <para>This series is infinitely large, which avoids day boundary defects and makes multi-day continuous iterations easy. Therefore, if you want to get a bounded list of items, you should filter the result with methods like <see cref="Enumerable.First{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,bool})"/> and <see cref="Enumerable.TakeWhile{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,bool})"/>, rather than just using <see cref="Enumerable.ToList{TSource}"/> or <see cref="Enumerable.Count{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>, which will never terminate.</para>
    /// <para>For example, to get the sunrise and sunset for a particular day:</para>
    /// <para><c>ZonedDateTime now = SystemClock.Instance.InTzdbSystemDefaultZone().GetCurrentZonedDateTime();
    /// IEnumerable&lt;SunlightChange&gt; dailySunriseAndSunset = SunlightCalculator.GetSunlightChanges(now, 37.35, -121.95)
    ///     .TakeWhile(change =&gt; change.Time.Date.Equals(now.Date)).ToList();
    /// 
    /// ZonedDateTime? sunrise = dailySunriseAndSunset.FirstOrNull(s =&gt; s.Name == SolarTimeOfDay.Sunrise)?.Time;
    /// ZonedDateTime? sunset  = dailySunriseAndSunset.FirstOrNull(change =&gt; change.Name == SolarTimeOfDay.Sunset)?.Time;</c></para>
    /// </summary>
    /// <param name="start">Returned instants will occur after this instant. The time zone must be the correct time zone at the given location.</param>
    /// <param name="latitude">Degrees north of the equator</param>
    /// <param name="longitude">Degrees east of the prime meridian</param>
    /// <returns>Infinite series of changes in sunlight levels for the given location, beginning after <paramref name="start"/>.</returns>
    public static IEnumerable<SunlightChange> GetSunlightChanges(ZonedDateTime start, double latitude, double longitude) {
        while (true) {
            SunlightChange nextSunlightLevelChange = GetNextSunlightChange(start, latitude, longitude);
            yield return nextSunlightLevelChange;
            start = nextSunlightLevelChange.Time.PlusMinutes(PaddingMinutes);
        }
        // ReSharper disable once IteratorNeverReturns - the sun will never stop rising
    }

    internal static SunlightChange GetNextSunlightChange(ZonedDateTime start, double latitude, double longitude) {
        decimal        elevation             = SolarElevation(start, latitude, longitude);
        SunlightLevel  currentSunlightLevel  = GetSunlightForSolarElevation(elevation);
        decimal        elevationRateOfChange = SolarElevation(start.PlusSeconds(1), latitude, longitude) - elevation; // degrees per second
        bool           isSunRising           = elevationRateOfChange > 0;
        SolarTimeOfDay nextSolarTimeOfDay    = currentSunlightLevel.GetEnd(isSunRising);
        decimal        desiredElevation      = (decimal) nextSolarTimeOfDay.StartingSolarElevationAngle();

        (ZonedDateTime previousSolarNoon, ZonedDateTime previousSolarMidnight, ZonedDateTime nextSolarNoon, ZonedDateTime nextSolarMidnight) = GetSurroundingSolarNoonsAndMidnights(start, longitude);

        // TODO instead of the 12-hour check, check if next solar midnight is before next solar noon
        if (!isSunRising && GetSunlightAt(nextSolarMidnight, latitude, longitude) == currentSunlightLevel && nextSolarMidnight - start <= Duration.FromHours(12)) {
            return GetNextSunlightChange(nextSolarMidnight.PlusMinutes(PaddingMinutes), latitude, longitude);
        } else if (isSunRising && GetSunlightAt(nextSolarNoon, latitude, longitude) == currentSunlightLevel && nextSolarNoon - start <= Duration.FromHours(12)) {
            return GetNextSunlightChange(nextSolarNoon.PlusMinutes(PaddingMinutes), latitude, longitude);
        }

        ZonedDateTime estimatedResultTime = start;
        if (isSunRising) {
            decimal maxElevationAtSolarNoon     = SolarElevation(nextSolarNoon, latitude, longitude);
            decimal minElevationAtSolarMidnight = SolarElevation(previousSolarMidnight, latitude, longitude);
            if (desiredElevation >= minElevationAtSolarMidnight && desiredElevation <= maxElevationAtSolarNoon) {
                Interval solarHalfDay          = new(previousSolarMidnight.ToInstant(), nextSolarNoon.ToInstant());
                decimal  desiredElevationRatio = (desiredElevation - minElevationAtSolarMidnight) / (maxElevationAtSolarNoon - minElevationAtSolarMidnight);

                decimal estimatedTimeRatioOfDesiredElevation = 2 * Asin(Sqrt(desiredElevationRatio)) / Pi;
                estimatedResultTime = previousSolarMidnight + solarHalfDay.Duration * (double) estimatedTimeRatioOfDesiredElevation;
            } // otherwise the next solar time of day will never happen before the next solar noon because of polar noon/midnight
        } else {
            decimal maxElevationAtSolarNoon     = SolarElevation(previousSolarNoon, latitude, longitude);
            decimal minElevationAtSolarMidnight = SolarElevation(nextSolarMidnight, latitude, longitude);
            if (desiredElevation >= minElevationAtSolarMidnight && desiredElevation <= maxElevationAtSolarNoon) {
                Interval solarHalfDay          = new(previousSolarNoon.ToInstant(), nextSolarMidnight.ToInstant());
                decimal  desiredElevationRatio = (desiredElevation - minElevationAtSolarMidnight) / (maxElevationAtSolarNoon - minElevationAtSolarMidnight);

                decimal estimatedTimeRatioOfDesiredElevation = 1 - 2 * Asin(Sqrt(desiredElevationRatio)) / Pi;
                estimatedResultTime = previousSolarNoon + solarHalfDay.Duration * (double) estimatedTimeRatioOfDesiredElevation;
            } // otherwise the next solar time of day will never happen before the next solar midnight because of polar noon/midnight
        }

        elevation = SolarElevation(estimatedResultTime, latitude, longitude);

        for (int iterations = 1; iterations < MaxIterations; iterations++) {
            elevationRateOfChange =  SolarElevation(estimatedResultTime.PlusSeconds(1), latitude, longitude) - elevation;
            estimatedResultTime   += Duration.FromSeconds((double) ((desiredElevation - elevation) / elevationRateOfChange));
            elevation             =  SolarElevation(estimatedResultTime, latitude, longitude);

            if (System.Math.Abs(elevation - desiredElevation) <= ElevationPrecision) {
                break;
            }
        }

        return new SunlightChange(estimatedResultTime, nextSolarTimeOfDay);
    }

    private static (ZonedDateTime previousSolarNoon, ZonedDateTime previousSolarMidnight, ZonedDateTime nextSolarNoon, ZonedDateTime nextSolarMidnight) GetSurroundingSolarNoonsAndMidnights(
        ZonedDateTime start, double longitude) {
        ZonedDateTime? previousSolarNoon     = null;
        ZonedDateTime? previousSolarMidnight = null;
        ZonedDateTime? nextSolarNoon         = null;
        ZonedDateTime? nextSolarMidnight     = null;

        for (int daysFromStart = 0;
             previousSolarNoon == null || previousSolarMidnight == null || nextSolarNoon == null || nextSolarMidnight == null;
             daysFromStart = -daysFromStart + (daysFromStart <= 0 ? 1 : 0)) {

            ZonedDateTime candidateNoon = SolarNoon(start.Date.PlusDays(daysFromStart), start.Zone, longitude);

            // I'm not sure if solar midnight is supposed to be noon + 12h or if it should be 0.5 * length of solar day instead. On https://timeanddate.com it seems to always be previous noon + 12h.
            ZonedDateTime candidateMidnight = candidateNoon.PlusHours(12);

            if (candidateNoon.IsBefore(start) && (previousSolarNoon == null || candidateNoon.IsAfter(previousSolarNoon.Value))) {
                previousSolarNoon = candidateNoon;
            }

            if (candidateMidnight.IsBefore(start) && (previousSolarMidnight == null || candidateMidnight.IsAfter(previousSolarMidnight.Value))) {
                previousSolarMidnight = candidateMidnight;
            }

            if (candidateNoon.IsAfter(start) && (nextSolarNoon == null || candidateNoon.IsBefore(nextSolarNoon.Value))) {
                nextSolarNoon = candidateNoon;
            }

            if (candidateMidnight.IsAfter(start) && (nextSolarMidnight == null || candidateMidnight.IsBefore(nextSolarMidnight.Value))) {
                nextSolarMidnight = candidateMidnight;
            }
        }

        return (previousSolarNoon.Value, previousSolarMidnight.Value, nextSolarNoon.Value, nextSolarMidnight.Value);
    }

}