☀ SolCalc
===

[![NuGet](https://img.shields.io/nuget/v/SolCalc?logo=nuget)](https://www.nuget.org/packages/SolCalc)

Find when sunrise, sunset, and different twilights happen for a given location, based on the [NOAA ESRL Solar Calculator](https://gml.noaa.gov/grad/solcalc/) web page.

Features high accuracy across several millenia, atmospheric refraction, a simple enumeration-based API, and multiple/missing events during polar night/day/twilight at extreme latitudes.

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2,3" bullets="1.,-" -->

1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Concepts](#concepts)
    - [Sunlight levels](#sunlight-levels)
    - [Solar times of day](#solar-times-of-day)
1. [Usage](#usage)
    - [Get the sunlight level at a time and location](#get-the-sunlight-level-at-a-time-and-location)
    - [Get the series of sunlight level changes at a location starting after a time](#get-the-series-of-sunlight-level-changes-at-a-location-starting-after-a-time)
1. [Algorithm](#algorithm)
    - [Accuracy](#accuracy)

<!-- /MarkdownTOC -->

![Sun](https://raw.githubusercontent.com/Aldaviva/SolCalc/master/.github/images/readme-header.jpg)

## Prerequisites
- .NET runtime that conforms to [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) or later
    - .NET 5 or later
    - .NET Core 2.0 or later
    - .NET Framework 4.6.1 or later

## Installation
The [**`SolCalc`**](https://www.nuget.org/packages/SolCalc/) package is available on NuGet Gallery.

```bat
dotnet add package SolCalc
```

## Concepts

For more information and diagrams, see ["Twilight" on Wikipedia](https://en.wikipedia.org/wiki/Twilight).

### Sunlight levels

Quantized levels of sunlight brightness based on the sun's angle above the horizon at 0°. These represent **durations** from one [solar time of day](#solar-times-of-day) to the next. Each twilight generally occurs twice per day.

|Level|Solar elevation range|Description|
|-|-|-|
|**Daylight**|[0°, 90°]|Sun is visible|
|**Civil twilight**|[−6°, 0°)|Objects are visible|
|**Nautical twilight**|[−12°, −6°)|Silhouettes are visible|
|**Astronomical twilight**|[−18°, −12°)|Sunlight is only detectable with instruments|
|**Night**|[−90°, −18°)|No sunlight|

### Solar times of day

**Instants** in a day when the [sunlight level](#sunlight-levels) changes from one level to another.

|Time of day|Sun direction|Previous light level|New light level|Solar elevation|
|-|-|-|-|-|
|**Astronomical dawn**|Rising|Night|Astronomical twilight|−18°|
|**Nautical dawn**|Rising|Astronomical twilight|Nautical twilight|−12°|
|**Civil dawn**|Rising|Nautical twilight|Civil twilight|−6°|
|**Sunrise**|Rising|Civil twilight|Daylight|0°|
|**Sunset**|Setting|Daylight|Civil twilight|0°|
|**Civil dusk**|Setting|Civil twilight|Nautical twilight|−6°|
|**Nautical dusk**|Setting|Nautical twilight|Astronomical twilight|−12°|
|**Astronomical dusk**|Setting|Astronomical twilight|Night|−18°|

## Usage

The main entry point into this library is the `SolCalc.SunlightCalculator` static class.

### Get the sunlight level at a time and location

Use `SunlightCalculator.GetSunlightAt(ZonedDateTime, double, double)` to return the [sunlight level](#sunlight-levels) at the given geographic coordinates at the given instant.

The instant's time zone must be the same zone that the given location observes, otherwise the result will be wrong. In the example below, Santa Clara, CA is in the `America/Los_Angeles` time zone.

```cs
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;

DateTimeZone  zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
ZonedDateTime now  = SystemClock.Instance.InZone(zone).GetCurrentZonedDateTime();

SunlightLevel level = SunlightCalculator.GetSunlightAt(now, latitude: 37.35, longitude: -121.95);
Console.WriteLine($"It is currently {level} in Santa Clara, CA, US.");
// It is currently Night in Santa Clara, CA, US.
```

### Get the series of sunlight level changes at a location starting after a time

Use `SunlightCalculator.GetSunlightChanges(ZonedDateTime, double, double)` to get an infinite series of all future sunlight level changes at a particular location, starting after a given time.

The instant's time zone must be the same zone that the given location observes, otherwise the result will be wrong. In the example below, Santa Clara, CA is in the `America/Los_Angeles` time zone.

The returned `IEnumerable<SunlightChange>` is infinitely long because the sun never stops rising. It is not bounded by the end of the day. This means you should not try to call `.ToList()`, `.Count()`, or any other method that fully enumerates it, because they will never end. Instead, use filtering to get just the items you want, using methods like `.TakeWhile()`, `.SkipWhile()`, `.Where()`, and `.First()`.

#### Get the next sunlight level change at a location

```cs
DateTimeZone  zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
ZonedDateTime now  = SystemClock.Instance.InZone(zone).GetCurrentZonedDateTime();

SunlightChange nextChange = SunlightCalculator.GetSunlightChanges(now, latitude: 37.35, longitude: -121.95).First();
Console.WriteLine($"At {nextChange.Time}, {nextChange.Name} will start when {nextChange.PreviousSunlightLevel} changes to {nextChange.NewSunlightLevel} in Santa Clara, CA, US.");
// At 2024-04-22T04:48:01 America/Los_Angeles (-07), AstronomicalDawn will start when Night changes to AstronomicalTwilight in Santa Clara, CA, US.
```

#### Get sunrise and sunset on a date at a location

```cs
DateTimeZone  zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
ZonedDateTime now  = SystemClock.Instance.InZone(zone).GetCurrentZonedDateTime();
IEnumerable<SunlightChange> dailySunriseAndSunset = SunlightCalculator.GetSunlightChanges(now, 37.35, -121.95)
    .TakeWhile(s => s.Time.Date == now.Date).ToList();

ZonedDateTime? sunrise = dailySunriseAndSunset.FirstOrNull(s => s.Name == SolarTimeOfDay.Sunrise)?.Time;
ZonedDateTime? sunset  = dailySunriseAndSunset.FirstOrNull(s => s.Name == SolarTimeOfDay.Sunset)?.Time;
```

#### Wait for each sunlight level change

```cs
DateTimeZone zone  = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
ZonedClock   clock = SystemClock.Instance.InZone(zone);

IEnumerable<SunlightChange> sunlightChanges = SunlightCalculator.GetSunlightChanges(clock.GetCurrentZonedDateTime(), 37.35, -121.95);
foreach (SunlightChange sunlightChange in sunlightChanges) {
    Duration delay = sunlightChange.Time.Minus(clock.GetCurrentZonedDateTime());
    Console.WriteLine($"Waiting {delay} for {sunlightChange.Name} at {sunlightChange.Time}");
    await Task.Delay(delay.ToTimeSpan());

    Console.WriteLine($"It is currently {sunlightChange.Name} at {clock.GetCurrentZonedDateTime()} in Santa Clara, CA, US");
}
```

## Algorithm

This library uses the solar position calculation algorithm from the National Oceanic and Atmospheric Administration's Earth System Research Laboratories' Global Monitoring Laboratory's web-based [Solar Calculator](https://gml.noaa.gov/grad/solcalc/).

> [!WARNING]  
> For research and recreational use only. Not for legal use.

If you want to calculate solar position or solar noon, you can use the `SolarCalculator` static class.

### Accuracy

- Time accuracy decreases from &#x2264; ±1 minute at latitudes &#x2264; ±72°, to &#x2264; ±10 minutes at latitudes &gt; ±72°.
- Atmospheric refraction is taken in account.
- Clouds, air pressure, humidity, dust, other atmospheric conditions, observer's altitude, and solar eclipses are not taken into account.
- Years between −2000 and 3000 can be handled.
- Dates before October 15, 1582 might not use the correct calendar system.
- Years between 1800 and 2100 have the highest accuracy results. Years between −1000 and 3000 have medium accuracy results. Years outside those ranges have lower accuracy results.