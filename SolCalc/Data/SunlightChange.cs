using NodaTime;

namespace SolCalc.Data;

/// <summary>
/// An instant in time when the amount of sunlight changes to a different level at a specific location on a specific day, based on the sun's angle above the horizon.
/// </summary>
/// <param name="Time">The time when the solar elevation crosses an angular threshold</param>
/// <param name="Name">The name for this time of day, for example <see cref="SolarTimeOfDay.Sunrise"/> when the sun rises to 0° above the horizon</param>
public readonly record struct SunlightChange(ZonedDateTime Time, SolarTimeOfDay Name) {

    /// <summary>
    /// The time when the solar elevation crosses an angular threshold
    /// </summary>
    public ZonedDateTime Time { get; } = Time;

    /// <summary>
    /// The name for this time of day, for example <see cref="SolarTimeOfDay.Sunrise"/> when the sun rises to 0° above the horizon
    /// </summary>
    public SolarTimeOfDay Name { get; } = Name;

    /// <summary>
    /// <para>The amount of sunlight visible on and after the <see cref="Time"/> of this <see cref="SunlightChange"/>.</para>
    /// <para>For example, when <see cref="Name"/> is <see cref="SolarTimeOfDay.Sunset"/>, the <see cref="NewSunlightLevel"/> will be <see cref="SunlightLevel.CivilTwilight"/>.</para>
    /// </summary>
    public SunlightLevel NewSunlightLevel { get; } = Name.NewSunlight();

    /// <summary>
    /// <para>The amount of sunlight that had been visible before the <see cref="Time"/> of this <see cref="SunlightChange"/>.</para>
    /// <para>For example, when <see cref="Name"/> is <see cref="SolarTimeOfDay.Sunset"/>, the <see cref="PreviousSunlightLevel"/> will be <see cref="SunlightLevel.Daylight"/>.</para>
    /// </summary>
    public SunlightLevel PreviousSunlightLevel { get; } = Name.PreviousSunlight();

    /// <summary>
    /// <c>true</c> if the sun was rising (increasing its solar elevation) at the time of this change, or <c>false</c> if it was setting
    /// </summary>
    public bool IsSunRising { get; } = Name.IsSunRising();

}