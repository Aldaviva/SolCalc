namespace SolCalc.Data;

/// <summary>
/// <para>Categories of the amount of sunlight at a given time and location on Earth, based on the angle of the sun above the horizon (solar elevation).</para>
/// <para>This takes atmospheric refraction into account, but not atmospheric conditions, the altitude of the observer above the Earth's surface, or solar eclipses.</para>
/// </summary>
public enum SunlightLevel {

    /// <summary>
    /// <para>No sunlight at all, brightness is nearly zero.</para>
    /// <para>When sun's elevation &lt; −18°</para>
    /// <para>There are typically two periods of night in a single day, one in the morning from midnight to <see cref="SolarTimeOfDay.AstronomicalDawn"/>, and
    /// another in the evening from <see cref="SolarTimeOfDay.AstronomicalDusk"/> to midnight of the following day.</para>
    /// </summary>
    Night,

    /// <summary>
    /// <para>Sky illumination is very faint and mostly undetectable to the naked eye. Useful for taking astronomical observations of stars and planets.</para>
    /// <para>When −18° &#x2264; sun's elevation &lt; −12°</para>
    /// <para>There are typically two periods of astronomical twilight in a single day, one in the morning from <see cref="SolarTimeOfDay.AstronomicalDawn"/> to <see cref="SolarTimeOfDay.NauticalDawn"/>, and
    /// another in the evening from <see cref="SolarTimeOfDay.NauticalDusk"/> to <see cref="SolarTimeOfDay.AstronomicalDusk"/>.</para>
    /// </summary>
    AstronomicalTwilight,

    /// <summary>
    /// <para>You can see the horizon and shapes of objects, but artificial light is needed for outdoor activities. Useful for sailors navigating by observing stars and the horizon.</para>
    /// <para>When −12° &#x2264; sun's elevation &lt; −6°</para>
    /// <para>There are typically two periods of nautical twilight in a single day, one in the morning from <see cref="SolarTimeOfDay.NauticalDawn"/> to <see cref="SolarTimeOfDay.CivilDawn"/>, and another in
    /// the evening from <see cref="SolarTimeOfDay.CivilDusk"/> to <see cref="SolarTimeOfDay.NauticalDusk"/>.</para>
    /// </summary>
    NauticalTwilight,

    /// <summary>
    /// <para>The sun has set, but you can still see enough for outdoor activities.</para>
    /// <para>When −6° &#x2264; sun's elevation &lt; −0°</para>
    /// <para>There are typically two periods of civil twilight in a single day, one in the morning from <see cref="SolarTimeOfDay.CivilDawn"/> to <see cref="SolarTimeOfDay.Sunrise"/>, and another in
    /// the evening from <see cref="SolarTimeOfDay.Sunset"/> to <see cref="SolarTimeOfDay.CivilDusk"/>.</para>
    /// </summary>
    CivilTwilight,

    /// <summary>
    /// <para>Sun is above the horizon.</para>
    /// <para>This does not account for atmospheric conditions or solar eclipses.</para>
    /// <para>When sun's elevation &#x2265; 0°</para>
    /// <para>There is typically one period of daylight in a single day, starting at <see cref="SolarTimeOfDay.Sunrise"/> in the morning and ending at <see cref="SolarTimeOfDay.Sunset"/> in the evening.</para>
    /// </summary>
    Daylight

}