using static SolCalc.NoaaSolarCalculator;

namespace SolCalc;

/*
 * Ported from Solar Calculator by the NOAA Earth System Research Laboratories' Global Monitoring Laboratory
 * Web application: https://gml.noaa.gov/grad/solcalc/
 * JavaScript source: https://gml.noaa.gov/grad/solcalc/main.js
 * This algorithm used by the webapp is supposed to be more accurate than their spreadsheet versions, especially for Julian date conversions
 */
/// <summary>
/// <para>Perform calculations about the Sun's position relative to a given location and time on Earth.</para>
/// <para>This low-level class calculates the sun's angle. To instead calculate the amount of sunlight (like dusk) and enumerate sunlight changes (like sunset), use <see cref="SunlightCalculator"/>.</para>
/// <para> </para>
/// <para>These calculations are from NOAA Earth System Research Laboratories' Global Monitoring Laboratory Solar Calculator (<see href="https://gml.noaa.gov/grad/solcalc/"/>).</para>
/// </summary>
/// <remarks><para>For research and recreational use only. Not for legal use.</para><para>Accuracy:</para><list type="bullet">
/// <item><description>Time accuracy decreases from &#x2264; ±1 minute at latitudes &#x2264; ±72°, to &#x2264; ±10 minutes at latitudes &gt; ±72°.</description></item>
/// <item><description>Atmospheric refraction is taken in account.</description></item>
/// <item><description>Clouds, air pressure, humidity, dust, other atmospheric conditions, observer's altitude, and solar eclipses are not taken into account.</description></item>
/// <item><description>Years between -2000 and 3000 can be handled.</description></item>
/// <item><description>Dates before October 15, 1582 might not use the correct <see cref="CalendarSystem"/>.</description></item>
/// <item><description>Years between 1800 and 2100 have the highest accuracy results. Years between -1000 and 3000 have medium accuracy results. Years outside those ranges have lower accuracy results.</description></item>
/// </list>
/// </remarks>
public static class SolarCalculator {

    private readonly record struct TimeAndPlace {

        public TimeAndPlace(ZonedDateTime dateTime, double latitude, double longitude) {
            LocalTime      = (decimal) dateTime.TimeOfDay.ToDurationSinceStartOfDay().TotalMinutes;
            TimeZoneOffset = (decimal) dateTime.Offset.ToHours();
            JulianDateTime = CalcTimeJulianCent(GetJd(dateTime.Date) + LocalTime / 1440.0m - TimeZoneOffset / 24.0m);
            Latitude       = (decimal) latitude;
            Longitude      = (decimal) longitude;
        }

        public decimal JulianDateTime { get; }
        public decimal LocalTime { get; }
        public decimal Latitude { get; }
        public decimal Longitude { get; }
        public decimal TimeZoneOffset { get; }

    }

    /// <summary>
    /// <para>Get the vertical angle of the sun above the horizon at a given instant and place.</para>
    /// <para>To also get azimuth and declination faster, call <see cref="SolarPosition"/>.</para>
    /// </summary>
    /// <param name="time">date and time</param>
    /// <param name="latitude">degrees north of the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>The angle of the sun in degrees, where 0° is the horizon and 90° is directly overhead</returns>
    public static decimal SolarElevation(ZonedDateTime time, double latitude, double longitude) {
        TimeAndPlace t = new(time, latitude, longitude);
        return CalcEl(t.JulianDateTime, t.LocalTime, t.Latitude, t.Longitude, t.TimeZoneOffset);
    }

    /// <summary>
    /// <para>Get the horizontal angle of the sun clockwise from true north at a given instant and place.</para>
    /// <para>To also get elevation and declination faster, call <see cref="SolarPosition"/>.</para>
    /// </summary>
    /// <param name="time">date and time</param>
    /// <param name="latitude">degrees north of the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>The angle of the sun in degrees, where 0° is true north, increasing clockwise from above the earth's surface</returns>
    public static decimal SolarAzimuth(ZonedDateTime time, double latitude, double longitude) {
        TimeAndPlace t = new(time, latitude, longitude);
        return CalcAz(t.JulianDateTime, t.LocalTime, t.Latitude, t.Longitude, t.TimeZoneOffset);
    }

    /// <summary>
    /// <para>Get the sun's azimuth, elevation, and declination for a given instant and place.</para>
    /// <para>To get only azimuth faster, call <see cref="SolarAzimuth"/>. To get only elevation faster, call <see cref="SolarElevation"/>.</para>
    /// </summary>
    /// <param name="time">date and time</param>
    /// <param name="latitude">degrees north of the equator</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>The azimuth (angle clockwise from true north), elevation (angle above the horizon), and declination (angle above the equator) of the sun in degrees</returns>
    public static SolarPosition SolarPosition(ZonedDateTime time, double latitude, double longitude) {
        TimeAndPlace t = new(time, latitude, longitude);
        return CalcAzEl(t.JulianDateTime, t.LocalTime, t.Latitude, t.Longitude, t.TimeZoneOffset);
    }

    /// <summary>
    /// Get the instant when the sun is directly overhead the given longitude on the given day, which is not always 12:00 PM noon due to your location within a time zone.
    /// </summary>
    /// <param name="date">day on which solar noon happens</param>
    /// <param name="zone">time zone in which to return results</param>
    /// <param name="longitude">degrees east of the prime meridian</param>
    /// <returns>Instant on the given day when the sun transits given meridian</returns>
    public static ZonedDateTime SolarNoon(LocalDate date, DateTimeZone zone, double longitude) {
        decimal julianDate     = GetJd(date);
        decimal timeZoneOffset = (decimal) zone.AtStartOfDay(date).Offset.ToTimeSpan().TotalHours;
        decimal solarNoon      = CalcSolNoon(julianDate, (decimal) longitude, timeZoneOffset);
        return date.AtStartOfDayInZone(zone) + Duration.FromMinutes((double) solarNoon);
    }

}

/// <summary>
/// The sun's position relative to a point on Earth at a specific time
/// </summary>
/// <param name="Azimuth">horizontal angle of the sun clockwise from true north at 0°</param>
/// <param name="Elevation">vertical angle of the sun above the horizon, where 0° is the horizon and 90° is directly overhead</param>
/// <param name="Declination">vertical angle of the sun above the equator, where 0° is an equinox, &gt; 0° is northern hemisphere winter, and &lt; 0° is southern hemisphere winter</param>
public readonly record struct SolarPosition(decimal Azimuth, decimal Elevation, decimal Declination) {

    /// <summary>
    /// horizontal angle of the sun clockwise from true north at 0°
    /// </summary>
    public decimal Azimuth { get; } = Azimuth;

    /// <summary>
    /// vertical angle of the sun above the horizon, where 0° is the horizon and 90° is directly overhead
    /// </summary>
    public decimal Elevation { get; } = Elevation;

    /// <summary>
    /// vertical angle of the sun above the equator, where 0° is an equinox, &gt; 0° is northern hemisphere winter, and &lt; 0° is southern hemisphere winter
    /// </summary>
    public decimal Declination { get; } = Declination;

}