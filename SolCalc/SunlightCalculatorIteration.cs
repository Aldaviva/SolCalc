using NodaTime;
using SolCalc.Data;

namespace SolCalc;

/// <summary>
/// <para>Perform calculations about the level of sunlight visible at different times of day for a given date and location on Earth.</para>
/// <para>Based on the NOAA Earth System Research Laboratories' Global Monitoring Laboratory Solar Calculator (<see href="https://gml.noaa.gov/grad/solcalc/"/>).</para>
/// </summary>
/// <remarks>
/// <para>For accuracy, see <see cref="SolarCalculator"/> remarks.</para>
/// </remarks>
[Obsolete]
public static class SunlightCalculatorIteration {

    private static readonly Duration TemporalResolution = Duration.FromMinutes(1);

    /// <summary>
    /// Get the amount of sunlight visible at the given instant and location on Earth.
    /// </summary>
    /// <param name="time">time and date</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>level of sunlight</returns>
    public static SunlightLevel GetSunlightAt(ZonedDateTime time, double latitude, double longitude) => GetSunlightForSolarElevation(SolarCalculator.SolarElevation(time, latitude, longitude));

    /// <summary>
    /// <para>List the sequence of points of time on a given day when the amount of sunlight changes from one <see cref="SunlightLevel"/> to another at a specific location on Earth, including for example the change from <see cref="SunlightLevel.CivilTwilight"/> to <see cref="SunlightLevel.Daylight"/> at <see cref="SolarTimeOfDay.Sunrise"/>.</para>
    /// <para>For example, this will return 8 changes on a typical day, consisting of
    /// <list type="number">
    /// <item><description><see cref="SolarTimeOfDay.AstronomicalDawn"/>, the beginning of <see cref="SunlightLevel.AstronomicalTwilight"/> and the end of <see cref="SunlightLevel.Night"/> (sun rising)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.NauticalDawn"/>, the beginning of <see cref="SunlightLevel.NauticalTwilight"/> and the end of <see cref="SunlightLevel.AstronomicalTwilight"/> (sun rising)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.CivilDawn"/>, the beginning of <see cref="SunlightLevel.CivilTwilight"/> and the end of <see cref="SunlightLevel.NauticalTwilight"/> (sun rising)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.Sunrise"/>, the beginning of <see cref="SunlightLevel.Daylight"/> and the end of <see cref="SunlightLevel.CivilTwilight"/> (sun rising)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.Sunset"/>, the beginning of <see cref="SunlightLevel.CivilTwilight"/> and the end of <see cref="SunlightLevel.Daylight"/> (sun setting)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.CivilDusk"/>, the beginning of <see cref="SunlightLevel.NauticalTwilight"/> and the end of <see cref="SunlightLevel.CivilTwilight"/> (sun setting)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.NauticalDusk"/>, the beginning of <see cref="SunlightLevel.AstronomicalTwilight"/> and the end of <see cref="SunlightLevel.NauticalTwilight"/> (sun setting)</description></item>
    /// <item><description><see cref="SolarTimeOfDay.AstronomicalDusk"/>, the beginning of <see cref="SunlightLevel.Night"/> and the end of <see cref="SunlightLevel.AstronomicalTwilight"/> (sun setting)</description></item>
    /// </list>
    /// </para>
    /// <para>There may be fewer than 8 elements if the location is experiencing polar day/night/twilight on the given date. For example, on a day when extreme latitudes experience midnight sun, this will return an empty <see cref="IEnumerable{T}"/>.</para>
    /// <para>To get daily solar times as blocks of time instead of changes between light levels, use <see cref="ListSunlightIntervals"/>, which lends itself more to object-based reasoning.</para>
    /// </summary>
    /// <param name="date">the day for which to calculate events</param>
    /// <param name="zone">time zone in which to return times</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns><see cref="IEnumerable{T}"/> containing all the instants in a given day when the sun's elevation angle changes to a different <see cref="SunlightLevel"/>, in ascending chronological order, with 1 minute precision, or empty</returns>
    public static IEnumerable<SunlightChange> ListSunlightChanges(LocalDate date, DateTimeZone zone, double latitude, double longitude) {
        ZonedDateTime time                   = zone.AtStartOfDay(date);
        decimal       previousSolarElevation = SolarCalculator.SolarElevation(time.Minus(TemporalResolution), latitude, longitude);
        SunlightLevel previousSunlightLevel  = GetSunlightForSolarElevation(previousSolarElevation);

        while (time.Day == date.Day) {
            decimal       currentSolarElevation = SolarCalculator.SolarElevation(time, latitude, longitude);
            SunlightLevel currentSunlightLevel  = GetSunlightForSolarElevation(currentSolarElevation);

            if (previousSunlightLevel != currentSunlightLevel) {
                yield return new SunlightChange(time, currentSunlightLevel.GetStart(currentSolarElevation > previousSolarElevation));
                previousSunlightLevel = currentSunlightLevel;
            }

            previousSolarElevation = currentSolarElevation;
            time                   = time.Plus(TemporalResolution);
        }
    }

    /// <summary>
    /// <para>List the sequence of chunks of time on a given day for each of the <see cref="SunlightLevel"/>s at a specific location on Earth, including for example the period of <see cref="SunlightLevel.CivilTwilight"/> between <see cref="SolarTimeOfDay.CivilDawn"/> and <see cref="SolarTimeOfDay.Sunrise"/>.</para>
    /// <para>For example, this will return 9 elements on a typical day, consisting of
    /// <list type="number">
    /// <item><description><see cref="SunlightLevel.Night"/>, starting at midnight and ending at <see cref="SolarTimeOfDay.AstronomicalDawn"/></description></item>
    /// <item><description><see cref="SunlightLevel.AstronomicalTwilight"/>, starting at <see cref="SolarTimeOfDay.AstronomicalDawn"/> and ending at <see cref="SolarTimeOfDay.NauticalDawn"/></description></item>
    /// <item><description><see cref="SunlightLevel.NauticalTwilight"/>, starting at <see cref="SolarTimeOfDay.NauticalDawn"/> and ending at <see cref="SolarTimeOfDay.CivilDawn"/></description></item>
    /// <item><description><see cref="SunlightLevel.CivilTwilight"/>, starting at <see cref="SolarTimeOfDay.CivilDawn"/> and ending at <see cref="SolarTimeOfDay.Sunrise"/></description></item>
    /// <item><description><see cref="SunlightLevel.Daylight"/>, starting at <see cref="SolarTimeOfDay.Sunrise"/> and ending at <see cref="SolarTimeOfDay.Sunset"/></description></item>
    /// <item><description><see cref="SunlightLevel.CivilTwilight"/>, starting at <see cref="SolarTimeOfDay.Sunset"/> and ending at <see cref="SolarTimeOfDay.CivilDusk"/></description></item>
    /// <item><description><see cref="SunlightLevel.NauticalTwilight"/>, starting at <see cref="SolarTimeOfDay.CivilDusk"/> and ending at <see cref="SolarTimeOfDay.NauticalDusk"/></description></item>
    /// <item><description><see cref="SunlightLevel.AstronomicalTwilight"/>, starting at <see cref="SolarTimeOfDay.NauticalDusk"/> and ending at <see cref="SolarTimeOfDay.AstronomicalDusk"/></description></item>
    /// <item><description><see cref="SunlightLevel.Night"/>, starting at <see cref="SolarTimeOfDay.AstronomicalDusk"/> and ending at midnight of the following day</description></item>
    /// </list>
    /// </para>
    /// <para>There may be fewer than 9 elements if the location is experiencing polar day/night/twilight on the given date. For example, on a day when extreme latitudes experience midnight sun, this will return exactly 1 element, and it will be <see cref="SunlightLevel.Daylight"/>.</para>
    /// <para>The start times of the intervals are inclusive, but the end times are exclusive (not included in the interval).</para>
    /// <para>The intervals do not span day boundaries, so a typical night will be represented by two <see cref="SunlightInterval"/> instances of <see cref="SunlightLevel.Night"/>, one starting on the first day and ending at midnight, and the second starting at midnight and ending at sunrise.</para>
    /// <para>To get daily solar times as changes between light levels instead of blocks of time, use <see cref="ListSunlightChanges"/>, which lends itself more to event-based reasoning.</para>
    /// </summary>
    /// <param name="date">the day for which to calculate events</param>
    /// <param name="zone">time zone in which to return times</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns><see cref="IEnumerable{T}"/> containing all the intervals in a given day when the sun's elevation angle stays the same, in ascending chronological order, with 1 minute precision, but never empty</returns>
    public static IEnumerable<SunlightInterval> ListSunlightIntervals(LocalDate date, DateTimeZone zone, double latitude, double longitude) {
        ZonedDateTime   time                   = zone.AtStartOfDay(date);
        ZonedDateTime   intervalStart          = time;
        decimal         previousSolarElevation = SolarCalculator.SolarElevation(time, latitude, longitude);
        SunlightLevel   previousSunlightLevel  = GetSunlightForSolarElevation(previousSolarElevation);
        SolarTimeOfDay? intervalSolarStart     = null;
        time = time.Plus(TemporalResolution);

        while (time.Day == date.Day) {
            decimal       currentSolarElevation = SolarCalculator.SolarElevation(time, latitude, longitude);
            SunlightLevel currentSunlightLevel  = GetSunlightForSolarElevation(currentSolarElevation);

            if (currentSunlightLevel != previousSunlightLevel) {
                SolarTimeOfDay intervalSolarEnd = currentSunlightLevel.GetStart(previousSolarElevation < currentSolarElevation);
                yield return new SunlightInterval(intervalStart, intervalSolarStart, time, intervalSolarEnd, previousSunlightLevel);
                intervalStart         = time;
                previousSunlightLevel = currentSunlightLevel;
                intervalSolarStart    = intervalSolarEnd;
            }

            previousSolarElevation = currentSolarElevation;
            time                   = time.Plus(TemporalResolution);
        }

        yield return new SunlightInterval(intervalStart, intervalSolarStart, time.AtStartOfDay(), null, previousSunlightLevel);
    }

    /// <inheritdoc cref="GetSunlightForSolarElevation(decimal)"/>
    public static SunlightLevel GetSunlightForSolarElevation(double solarElevationDegrees) => GetSunlightForSolarElevation((decimal) solarElevationDegrees);

    /// <summary>
    /// <para>Get the amount of sunlight visible when the sun is at the given angle above the horizon.</para>
    /// <para>For example, when the sun is above or at the horizon, the light level is <see cref="SunlightLevel.Daylight"/>, but when the sun's elevation is in the range [-6°,0°), the light level is <see cref="SunlightLevel.CivilTwilight"/>.</para>
    /// </summary>
    /// <param name="solarElevation">angle of the sun above the horizon, in degrees, where 0° is the horizon and 90° is directly overhead</param>
    /// <returns>sunlight level for the given <paramref name="solarElevation"/></returns>
    public static SunlightLevel GetSunlightForSolarElevation(decimal solarElevation) => solarElevation switch {
        >= 0   => SunlightLevel.Daylight,
        >= -6  => SunlightLevel.CivilTwilight,
        >= -12 => SunlightLevel.NauticalTwilight,
        >= -18 => SunlightLevel.AstronomicalTwilight,
        _      => SunlightLevel.Night
    };

    /// <summary>
    /// <para>Get all the times on the given date that the given location enters the given level of sunlight.</para>
    /// <para>On a typical day, each twilight will start twice, once in the morning and once in the evening. For example, <see cref="SunlightLevel.CivilTwilight"/> usually starts twice, at <see cref="SolarTimeOfDay.CivilDawn"/> and <see cref="SolarTimeOfDay.Sunset"/>.</para>
    /// <para><see cref="SunlightLevel.Daylight"/> and <see cref="SunlightLevel.Night"/> start once per day, because the previous day's <see cref="SunlightLevel.Night"/> continuing into today does not count as starting at midnight. Each <see cref="SunlightLevel"/> may occur as few as 0 times in a day, and as many as 3 times at extreme latitudes.</para>
    /// <para>To get the times when the sun leaves the given <paramref name="sunlight"/> instead of entering it, see <see cref="ListSunlightLevelEnds"/>.</para>
    /// <para>To get the times when the sun starts and ends shining at least this brightly, instead of exactly this brightly, see <see cref="ListSunlightChangesRisingIntoOrSettingOutOf"/>.</para>
    /// <para>To get the times when the sun either enters or leaves any <see cref="SunlightLevel"/> instead of only entering a specific one, see <see cref="ListSunlightChanges"/>.</para>
    /// </summary>
    /// <param name="sunlight">the amount of sunlight to find, defined by the angle of the sun above the horizon</param>
    /// <param name="date">day on which to find start times</param>
    /// <param name="zone">time zone of the times to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of instants when periods of the given <paramref name="sunlight"/> begin, either when the sun rises or sets to that range of elevations, or empty</returns>
    public static IEnumerable<ZonedDateTime> ListSunlightLevelBeginnings(SunlightLevel sunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude) =>
        from change in ListSunlightChanges(date, zone, latitude, longitude)
        where change.NewSunlightLevel == sunlight
        select change.Time;

    /// <summary>
    /// <para>Get all the times on the given date that the given location exits the given level of sunlight.</para>
    /// <para>On a typical day, each twilight will end twice, once in the morning and once in the evening. For example, <see cref="SunlightLevel.CivilTwilight"/> usually ends twice, at <see cref="SolarTimeOfDay.Sunrise"/> and <see cref="SolarTimeOfDay.CivilDusk"/>.</para>
    /// <para><see cref="SunlightLevel.Daylight"/> and <see cref="SunlightLevel.Night"/> end once per day, because the day ending at midnight does not count as night stopping. Each <see cref="SunlightLevel"/> may occur as few as 0 times in a day, and as many as 3 times at extreme latitudes.</para>
    /// <para>To get the times when the sun enters the given <paramref name="sunlight"/> instead of leaving it, see <see cref="ListSunlightLevelBeginnings"/>.</para>
    /// <para>To get the times when the sun starts and ends shining at least this brightly, instead of exactly this brightly, see <see cref="ListSunlightChangesRisingIntoOrSettingOutOf"/>.</para>
    /// <para>To get the times when the sun either enters or leaves any <see cref="SunlightLevel"/> instead of only exiting a specific one, see <see cref="ListSunlightChanges"/>.</para>
    /// </summary>
    /// <param name="sunlight">the amount of sunlight to find the end of, defined by the angle of the sun above the horizon</param>
    /// <param name="date">day on which to find start times</param>
    /// <param name="zone">time zone of the times to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of instants when periods of the given <paramref name="sunlight"/> end, either when the sun rises or sets outside of that range of elevations, or empty</returns>
    public static IEnumerable<ZonedDateTime> ListSunlightLevelEnds(SunlightLevel sunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude) =>
        from change in ListSunlightChanges(date, zone, latitude, longitude)
        where change.PreviousSunlightLevel == sunlight
        select change.Time;

    /// <summary>
    /// <para>Get all the times on the given date that the given location begins or finishes seeing at least the given amount of sunlight. Exceeding this amount does not count as another event.</para>
    /// <para>On a typical day, a location will experience at least some given amount sunlight once, starting in the morning and ending in the evening, except for <see cref="SunlightLevel.Night"/>, because it never gets darker than night (ignoring lunar eclipses).</para>
    /// <para>For example, <see cref="SunlightLevel.CivilTwilight"/> starts when the sun rises into <see cref="SolarTimeOfDay.CivilDawn"/>, typically in the morning, and the sunlight level does not decrease below <see cref="SunlightLevel.CivilTwilight"/> until the sun sets out of <see cref="SolarTimeOfDay.CivilDusk"/>, typically in the evening.</para>
    /// <para>Each <see cref="SunlightLevel"/> may start and end as few as 0 times in a day, and as many as twice at extreme latitudes.</para>
    /// <para>To get the times when the sun enters the given <paramref name="minimumSunlight"/> either through rising or setting instead of only entering through rising, see <see cref="ListSunlightLevelBeginnings"/>.</para>
    /// <para>To get the times when the sun exits the given <paramref name="minimumSunlight"/> either through rising or setting instead of only exiting through setting, see <see cref="ListSunlightLevelEnds"/>.</para>
    /// <para>To get the times when the sun either enters or leaves any <see cref="SunlightLevel"/> instead of only exiting a specific one, see <see cref="ListSunlightChanges"/>.</para>
    /// </summary>
    /// <param name="minimumSunlight">all returned changes will represent times when the amount of sunlight started or stopped being at least this bright</param>
    /// <param name="date">day on which to find sunlight level changes</param>
    /// <param name="zone">time zone of the times to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of times when the sun's elevation angle made the light level at the given location start or stop being at least as bright as <paramref name="minimumSunlight"/>, or empty</returns>
    public static IEnumerable<SunlightChange> ListSunlightChangesRisingIntoOrSettingOutOf(SunlightLevel minimumSunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude) =>
        from change in ListSunlightChanges(date, zone, latitude, longitude)
        where (change.NewSunlightLevel == minimumSunlight && change.IsSunRising) || (change.PreviousSunlightLevel == minimumSunlight && !change.IsSunRising)
        select change;

    /// <summary>
    /// <para>Get all the times on the given date that the given location experiences the given solar time of day event.</para>
    /// <para>For example, a location will experience a given <see cref="SolarTimeOfDay"/> such as <see cref="SolarTimeOfDay.Sunrise"/> exactly once, although it can happen as few as 0 times and as often as twice at extreme latitudes.</para>
    /// </summary>
    /// <param name="solarTimeOfDay">the solar time of day event to find</param>
    /// <param name="date">day on which to find events</param>
    /// <param name="zone">time zone of the times to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of times when the given solar time of day event occurs at the given location on the given day, or empty</returns>
    public static IEnumerable<ZonedDateTime> ListSolarEventTimes(SolarTimeOfDay solarTimeOfDay, LocalDate date, DateTimeZone zone, double latitude, double longitude) =>
        from change in ListSunlightChanges(date, zone, latitude, longitude)
        where change.Name == solarTimeOfDay
        select change.Time;

    /// <summary>
    /// <para>Get a sequence of chunks of time during a given day in a location when the level of sunlight reaches but does not exceed a given <see cref="SunlightLevel"/>.</para>
    /// <para>For example, a typical day will have two intervals of light levels like <see cref="SunlightLevel.CivilTwilight"/> or <see cref="SunlightLevel.Night"/>, and only one interval of <see cref="SunlightLevel.Daylight"/>.</para>
    /// <para>However, at extreme latitudes, a given <see cref="SunlightLevel"/> may never be reached in a given day, or it may occur up to three times.</para>
    /// <para>Returned <see cref="SunlightInterval"/>s can begin or end at midnight, as long as the <paramref name="exactSunlightLevel"/> matches.</para>
    /// <para>To get all intervals of all <see cref="SunlightLevel"/> in the given day, instead of filtering by one <see cref="SunlightLevel"/>, call <see cref="ListSunlightIntervals"/>.</para>
    /// <para>To get wider intervals where the level of sunlight stays at or above a given <see cref="SunlightLevel"/>, instead of ending when it goes above this level, call <see cref="ListSunlightIntervalsWithLevelBrighterOrEqualTo"/>.</para>
    /// </summary>
    /// <param name="exactSunlightLevel">only return intervals of time when the amount of sunlight is completely contained by the range of elevation angles of this <see cref="SunlightLevel"/></param>
    /// <param name="date">day on which to find events</param>
    /// <param name="zone">time zone of the intervals to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of blocks of time when the amount of sunlight is wholly within the bounds of <paramref name="exactSunlightLevel"/>'s range of solar elevation angles, or empty</returns>
    public static IEnumerable<SunlightInterval> ListSunlightIntervalsWithLevelExactly(SunlightLevel exactSunlightLevel, LocalDate date, DateTimeZone zone, double latitude, double longitude) =>
        from interval in ListSunlightIntervals(date, zone, latitude, longitude)
        where interval.LightLevel == exactSunlightLevel
        select interval;

    /// <summary>
    /// <para>Get a sequence of chunks of time during a given day in a location when the level of sunlight stays at or above a given <see cref="SunlightLevel"/>.</para>
    /// <para>For example, a typical day will have exactly one interval of time when it is at least as bright as <see cref="SunlightLevel.CivilTwilight"/> or <see cref="SunlightLevel.Daylight"/>, and two intervals of <see cref="SunlightLevel.Night"/>.</para>
    /// <para>However, at extreme latitudes, a given <see cref="SunlightLevel"/> may never be reached in a given day, and it may be reached or exceeded twice.</para>
    /// <para>Returned <see cref="SunlightInterval"/>s can begin or end at midnight, as long as the <paramref name="minimumSunlight"/> matches.</para>
    /// <para>To get all intervals of all <see cref="SunlightLevel"/> in the given day, instead of filtering by a lower bound <see cref="SunlightLevel"/>, call <see cref="ListSunlightIntervals"/>.</para>
    /// <para>To get narrower intervals where the level of sunlight stays at exactly one given <see cref="SunlightLevel"/> without exceeding it, instead of merging intervals that exceed this level, call <see cref="ListSunlightIntervalsWithLevelExactly"/>.</para>
    /// </summary>
    /// <param name="minimumSunlight">only return intervals of time when the amount of sunlight meets or exceeds this <see cref="SunlightLevel"/></param>
    /// <param name="date">day on which to find events</param>
    /// <param name="zone">time zone of the intervals to return</param>
    /// <param name="latitude">degrees above the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>chronologically ascending ordered sequence of blocks of time when the amount of sunlight is at least as high <paramref name="minimumSunlight"/>'s minimum solar elevation angles, or empty</returns>
    public static IEnumerable<SunlightInterval> ListSunlightIntervalsWithLevelBrighterOrEqualTo(SunlightLevel minimumSunlight, LocalDate date, DateTimeZone zone, double latitude, double longitude) {
        SunlightInterval? startSuperInterval  = null;
        SunlightInterval? previousSubInterval = null;

        foreach (SunlightInterval subInterval in ListSunlightIntervals(date, zone, latitude, longitude)) {
            if (subInterval.LightLevel >= minimumSunlight) {
                startSuperInterval  ??= subInterval;
                previousSubInterval =   subInterval;
            } else if (startSuperInterval.HasValue) {
                yield return new SunlightInterval(startSuperInterval.Value.Start, startSuperInterval.Value.StartName, previousSubInterval!.Value.End, previousSubInterval.Value.EndName,
                    minimumSunlight);
                startSuperInterval  = null;
                previousSubInterval = null;
            }
        }

        if (startSuperInterval.HasValue && previousSubInterval.HasValue) {
            yield return new SunlightInterval(startSuperInterval.Value.Start, startSuperInterval.Value.StartName, previousSubInterval.Value.End, previousSubInterval.Value.EndName,
                minimumSunlight);
        }
    }

}