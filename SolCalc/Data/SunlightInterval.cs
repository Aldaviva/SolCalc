using NodaTime;

namespace SolCalc.Data;

/// <summary>
/// <para>A non-overlapping block of time wholly contained within a single day (in a given time zone) based on the level of sunlight is the same or has the same minimum level.</para>
/// <para>For example, on January 25, 2024 in Santa Clara, CA, US, a <see cref="SunlightInterval"/> with <see cref="LightLevel"/> of <see cref="SunlightLevel.Daylight"/> would start at <see cref="SolarTimeOfDay.Sunrise"/> 7:16 AM and end at <see cref="SolarTimeOfDay.Sunset"/> at 5:24 PM Pacific Standard Time.</para>
/// <para>Instances do not span day boundaries, so a typical <see cref="SunlightLevel.Night"/> will be split into two <see cref="SunlightLevel"/> instances, one whose <see cref="End"/> is midnight and another whose <see cref="Start"/> is midnight.</para>
/// </summary>
/// <param name="Start">The time when this interval begins, inclusive – it does include the start time.</param>
/// <param name="End">The time when this interval finishes, exclusive – it does not include the end time.</param>
/// <param name="StartName">The name of the time of day at which this interval starts.</param>
/// <param name="EndName">The name of the time of day at which this interval ends.</param>
/// <param name="LightLevel">The amount of sunlight visible during this interval.</param>
[Obsolete]
public readonly record struct SunlightInterval(
    ZonedDateTime   Start,
    SolarTimeOfDay? StartName, //TODO make non-nullable
    ZonedDateTime   End,
    SolarTimeOfDay? EndName,
    SunlightLevel   LightLevel
) {

    /// <summary>
    /// The time when this interval begins, inclusive – it does include the start time.
    /// </summary>
    public ZonedDateTime Start { get; } = Start;

    /// <summary>
    /// The time when this interval finishes, exclusive – it does not include the end time.
    /// </summary>
    public ZonedDateTime End { get; } = End;

    /// <summary>
    /// <see cref="Start"/> to <see cref="End"/>
    /// </summary>
    public Interval Interval { get; } = new(Start.ToInstant(), End.ToInstant());

    /// <summary>
    /// <para>The amount of sunlight visible during this interval.</para>
    /// <para>For example, this would be <see cref="SunlightLevel.CivilTwilight"/> from <see cref="StartName"/>=<see cref="SolarTimeOfDay.CivilDawn"/> until <see cref="EndName"/>=<see cref="SolarTimeOfDay.Sunrise"/>, and it would be <see cref="SunlightLevel.Daylight"/> from <see cref="StartName"/>=<see cref="SolarTimeOfDay.Sunrise"/> until <see cref="EndName"/>=<see cref="SolarTimeOfDay.Sunset"/>.</para>
    /// <para>Does not account for clouds or solar eclipses.</para>
    /// </summary>
    public SunlightLevel LightLevel { get; } = LightLevel;

    /// <summary>
    /// <para>The name of the time of day at which this interval starts.</para>
    /// <para>For example, typical morning civil twilight starts at <see cref="SolarTimeOfDay.CivilDawn"/>, and typical evening civil twilight starts at <see cref="SolarTimeOfDay.Sunset"/>.</para>
    /// <para>Will be <c>null</c> if this is the first interval of the day, for example, a <see cref="SunlightLevel.Night"/> interval starting at midnight because this struct does not span multiple days, instead of continuing from astronomical dusk previous next day.</para>
    /// </summary>
    public SolarTimeOfDay? StartName { get; } = StartName;

    /// <summary>
    /// <para>The name of the time of day at which this interval ends.</para>
    /// <para>For example, typical morning civil twilight ends at <see cref="SolarTimeOfDay.Sunrise"/>, and typical evening civil twilight ends at <see cref="SolarTimeOfDay.CivilDusk"/>.</para>
    /// <para>Will be <c>null</c> if the interval was cut off by the day ending before the next sunlight level could start, for example, a <see cref="SunlightLevel.Night"/> interval ending at midnight because this struct does not span multiple days, instead of continuing until astronomical dawn the next day.</para>
    /// </summary>
    public SolarTimeOfDay? EndName { get; } = EndName;

}