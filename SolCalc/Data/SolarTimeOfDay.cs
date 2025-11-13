namespace SolCalc.Data;

/// <summary>
/// A well-known time of day as defined by the sun's elevation hitting a certain angle above the horizon.
/// </summary>
public enum SolarTimeOfDay {

    /// <summary>
    /// <para>The time when the sun rises to 18° below the horizon, typically in the morning.</para>
    /// <para>This is the end of <see cref="SunlightLevel.Night"/> and the beginning of <see cref="SunlightLevel.AstronomicalTwilight"/>.</para>
    /// </summary>
    AstronomicalDawn,

    /// <summary>
    /// <para>The time when the sun rises to 12° below the horizon, typically in the morning.</para>
    /// <para>This is the end of <see cref="SunlightLevel.AstronomicalTwilight"/> and the beginning of <see cref="SunlightLevel.NauticalTwilight"/>.</para>
    /// </summary>
    NauticalDawn,

    /// <summary>
    /// <para>The time when the sun rises to 6° below the horizon, typically in the morning.</para>
    /// <para>This is the end of <see cref="SunlightLevel.NauticalTwilight"/> and the beginning of <see cref="SunlightLevel.CivilTwilight"/>.</para>
    /// </summary>
    CivilDawn,

    /// <summary>
    /// <para>The time when the sun rises to 0° above the horizon, typically in the morning.</para>
    /// <para>This is the end of <see cref="SunlightLevel.CivilTwilight"/> and the beginning of <see cref="SunlightLevel.Daylight"/>.</para>
    /// </summary>
    Sunrise,

    /// <summary>
    /// <para>The time when the sun sets to 0° the horizon, typically in the evening.</para>
    /// <para>This is the end of <see cref="SunlightLevel.Daylight"/> and the beginning of <see cref="SunlightLevel.CivilTwilight"/>.</para>
    /// </summary>
    Sunset,

    /// <summary>
    /// <para>The time when the sun sets to 6° below the horizon, typically in the evening.</para>
    /// <para>This is the end of <see cref="SunlightLevel.CivilTwilight"/> and the beginning of <see cref="SunlightLevel.NauticalTwilight"/>.</para>
    /// </summary>
    CivilDusk,

    /// <summary>
    /// <para>The time when the sun sets to 12° below the horizon, typically in the evening.</para>
    /// <para>This is the end of <see cref="SunlightLevel.NauticalTwilight"/> and the beginning of <see cref="SunlightLevel.AstronomicalTwilight"/>.</para>
    /// </summary>
    NauticalDusk,

    /// <summary>
    /// <para>The time when the sun sets to 18° below the horizon, typically in the evening.</para>
    /// <para>This is the end of <see cref="SunlightLevel.AstronomicalTwilight"/> and the beginning of <see cref="SunlightLevel.Night"/>.</para>
    /// </summary>
    AstronomicalDusk

}