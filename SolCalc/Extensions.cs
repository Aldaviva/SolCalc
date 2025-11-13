using SolCalc.Data;

namespace SolCalc;

/// <summary>
/// Helper methods for <see cref="SunlightLevel"/> and <see cref="SolarTimeOfDay"/>.
/// </summary>
public static class Extensions {

    /// <summary>
    /// <para>For a period of sunlight at a given level, get the name for the time on a typical day when that period starts, which can depend on whether the sun is rising or setting during that
    /// period.</para>
    /// <para>For example, the period of <see cref="SunlightLevel.CivilTwilight"/> when the sun is rising starts at a time called <see cref="SolarTimeOfDay.CivilDawn"/>.</para>
    /// <para>However, the other period of <see cref="SunlightLevel.CivilTwilight"/> when the sun is setting instead starts at a time called <see cref="SolarTimeOfDay.Sunset"/>.</para>
    /// </summary>
    /// <param name="sunlight">The amount of light based on the sun's elevation above the horizon</param>
    /// <param name="isSunRising"><c>true</c> if the sun rose to reach this level of light, or <c>false</c> if it set instead. Ignored when <paramref name="sunlight"/> is
    /// <see cref="SunlightLevel.Daylight"/> or <see cref="SunlightLevel.Night"/>, which each start only once in a solar day instead of potentially multiple times like each of the twilight
    /// periods.</param>
    /// <returns>Astronomical, nautical, or civil dawn or dusk; or sunrise or sunset. </returns>
    public static SolarTimeOfDay GetStart(this SunlightLevel sunlight, bool isSunRising) => sunlight switch {
        SunlightLevel.AstronomicalTwilight when isSunRising  => SolarTimeOfDay.AstronomicalDawn,
        SunlightLevel.NauticalTwilight when isSunRising      => SolarTimeOfDay.NauticalDawn,
        SunlightLevel.CivilTwilight when isSunRising         => SolarTimeOfDay.CivilDawn,
        SunlightLevel.Daylight                               => SolarTimeOfDay.Sunrise,
        SunlightLevel.CivilTwilight when !isSunRising        => SolarTimeOfDay.Sunset,
        SunlightLevel.NauticalTwilight when !isSunRising     => SolarTimeOfDay.CivilDusk,
        SunlightLevel.AstronomicalTwilight when !isSunRising => SolarTimeOfDay.NauticalDusk,
        SunlightLevel.Night                                  => SolarTimeOfDay.AstronomicalDusk
    };

    /// <summary>
    /// <para>For a period of sunlight at a given level, get the name for the time on a typical day when that period ends, which can depend on whether the sun is rising or setting during that
    /// period.</para>
    /// <para>For example, the period of <see cref="SunlightLevel.CivilTwilight"/> when the sun is rising ends at <see cref="SolarTimeOfDay.Sunrise"/>.</para>
    /// <para>However, the other period of <see cref="SunlightLevel.CivilTwilight"/> when the sun is setting instead ends at a time called <see cref="SolarTimeOfDay.CivilDusk"/>.</para>
    /// </summary>
    /// <param name="sunlight">The amount of light based on the sun's elevation above the horizon</param>
    /// <param name="isSunRising"><c>true</c> if the sun rose to leave this level of light, or <c>false</c> if it set instead. Ignored when <paramref name="sunlight"/> is
    /// <see cref="SunlightLevel.Daylight"/> or <see cref="SunlightLevel.Night"/>, which each end only once in a solar day instead of potentially multiple times like each of the twilight
    /// periods.</param>
    /// <returns>Astronomical, nautical, or civil dawn or dusk; or sunrise or sunset. </returns>
    public static SolarTimeOfDay GetEnd(this SunlightLevel sunlight, bool isSunRising) => sunlight switch {
        SunlightLevel.AstronomicalTwilight when isSunRising  => SolarTimeOfDay.NauticalDawn,
        SunlightLevel.NauticalTwilight when isSunRising      => SolarTimeOfDay.CivilDawn,
        SunlightLevel.CivilTwilight when isSunRising         => SolarTimeOfDay.Sunrise,
        SunlightLevel.Daylight                               => SolarTimeOfDay.Sunset,
        SunlightLevel.CivilTwilight when !isSunRising        => SolarTimeOfDay.CivilDusk,
        SunlightLevel.NauticalTwilight when !isSunRising     => SolarTimeOfDay.NauticalDusk,
        SunlightLevel.AstronomicalTwilight when !isSunRising => SolarTimeOfDay.AstronomicalDusk,
        SunlightLevel.Night                                  => SolarTimeOfDay.AstronomicalDawn
    };

    /// <summary>
    /// <para>Shows whether the sun hits a given time of day while it is rising or setting.</para>
    /// <para>For example, <see cref="SolarTimeOfDay.Sunrise"/> happens when the sun is rising, and <see cref="SolarTimeOfDay.CivilDusk"/> happens when the sun is setting.</para>
    /// </summary>
    /// <param name="time">a well-known time of day based on the sun's elevation reaching a certain angle above the horizon</param>
    /// <returns><c>true</c> if the sun rose to reach the <paramref name="time"/>'s corresponding solar elevation, or <c>if it set to reach that elevation</c></returns>
    public static bool IsSunRising(this SolarTimeOfDay time) => time is SolarTimeOfDay.Sunrise or SolarTimeOfDay.CivilDawn or SolarTimeOfDay.NauticalDawn or SolarTimeOfDay.AstronomicalDawn;

    /// <summary>
    /// <para>For a given time of day, get the amount of sunlight that just stopped being visible before this time.</para>
    /// <para>For example, directly before <see cref="SolarTimeOfDay.Sunset"/>, the amount of sunlight was <see cref="SunlightLevel.Daylight"/> (and at sunset it changes to
    /// <see cref="SunlightLevel.CivilTwilight"/>, see <see cref="NewSunlight"/>).</para>
    /// </summary>
    /// <param name="time">a well-known time of day based on the sun's elevation reaching a certain angle above the horizon</param>
    /// <returns>amount of sunlight that was visible just before this time</returns>
    public static SunlightLevel PreviousSunlight(this SolarTimeOfDay time) => time switch {
        SolarTimeOfDay.AstronomicalDawn => SunlightLevel.Night,
        SolarTimeOfDay.NauticalDawn     => SunlightLevel.AstronomicalTwilight,
        SolarTimeOfDay.CivilDawn        => SunlightLevel.NauticalTwilight,
        SolarTimeOfDay.Sunrise          => SunlightLevel.CivilTwilight,
        SolarTimeOfDay.Sunset           => SunlightLevel.Daylight,
        SolarTimeOfDay.CivilDusk        => SunlightLevel.CivilTwilight,
        SolarTimeOfDay.NauticalDusk     => SunlightLevel.NauticalTwilight,
        SolarTimeOfDay.AstronomicalDusk => SunlightLevel.AstronomicalTwilight
    };

    /// <summary>
    /// <para>For a given time of day, get the amount of sunlight that begins starting at this time.</para>
    /// <para>For example, when <see cref="SolarTimeOfDay.Sunset"/> occurs, the amount of sunlight becomes <see cref="SunlightLevel.CivilTwilight"/> (and it was previously
    /// <see cref="SunlightLevel.Daylight"/>, see <see cref="PreviousSunlight"/>).</para>
    /// </summary>
    /// <param name="time">a well-known time of day based on the sun's elevation reaching a certain angle above the horizon</param>
    /// <returns>amount of sunlight that was visible just before this time</returns>
    public static SunlightLevel NewSunlight(this SolarTimeOfDay time) => time switch {
        SolarTimeOfDay.AstronomicalDawn => SunlightLevel.AstronomicalTwilight,
        SolarTimeOfDay.NauticalDawn     => SunlightLevel.NauticalTwilight,
        SolarTimeOfDay.CivilDawn        => SunlightLevel.CivilTwilight,
        SolarTimeOfDay.Sunrise          => SunlightLevel.Daylight,
        SolarTimeOfDay.Sunset           => SunlightLevel.CivilTwilight,
        SolarTimeOfDay.CivilDusk        => SunlightLevel.NauticalTwilight,
        SolarTimeOfDay.NauticalDusk     => SunlightLevel.AstronomicalTwilight,
        SolarTimeOfDay.AstronomicalDusk => SunlightLevel.Night
    };

    /// <summary>
    /// <para>Get the solar elevation angle at which the given solar time of day begins.</para>
    /// <para>For example, <see cref="SolarTimeOfDay.Sunrise"/> starts when the sun is at the horizon (<c>0.0</c>).</para>
    /// <para>Civil dusk starts when the sun is 6° below the horizon (<c>-6.0</c>).</para>
    /// </summary>
    /// <param name="time">a well-known time of day based on the sun's elevation reaching a certain angle above the horizon</param>
    /// <returns>the angular elevation of the sun above the horizon in degrees, where <c>0</c> is at the horizon, <c>90</c> is directly overhead, and negative values are below the horizon</returns>
    public static double StartingSolarElevationAngle(this SolarTimeOfDay time) => time switch {
        SolarTimeOfDay.AstronomicalDawn => -18,
        SolarTimeOfDay.NauticalDawn     => -12,
        SolarTimeOfDay.CivilDawn        => -6,
        SolarTimeOfDay.Sunrise          => 0,
        SolarTimeOfDay.Sunset           => 0,
        SolarTimeOfDay.CivilDusk        => -6,
        SolarTimeOfDay.NauticalDusk     => -12,
        SolarTimeOfDay.AstronomicalDusk => -18
    };

}