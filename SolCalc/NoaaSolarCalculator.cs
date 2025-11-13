using System.Diagnostics.CodeAnalysis;
using static System.Math;
using static Unfucked.DecimalMath;

namespace SolCalc;

/// <inheritdoc cref="SolarCalculator"/>
[ExcludeFromCodeCoverage]
internal static class NoaaSolarCalculator {

    public static decimal CalcTimeJulianCent(decimal julianDate) => (julianDate - 2451545.0m) / 36525.0m;

    public static decimal GetJd(LocalDate date) {
        (int year, int month, int day) = date;

        if (month <= 2) {
            year  -= 1;
            month += 12;
        }

        decimal century = Floor(year / 100m);
        return Floor(365.25m * (year + 4716m)) + Floor(30.6001m * (month + 1)) + day + (2 - century) + Floor(century / 4) - 1524.5m;
    }

    public static decimal CalcEl(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) => CalcEl(CalcAzElCommon(t, localtime, latitude, longitude, zone).zenith);

    private static decimal CalcEl(decimal zenith) => 90.0m - (zenith - CalcRefraction(90.0m - zenith));

    public static decimal CalcAz(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle, decimal _) = CalcAzElCommon(t, localtime, latitude, longitude, zone);
        return CalcAz(zenith, latitudeRad, thetaRad, hourAngle);
    }

    private static decimal CalcAz(decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle) {
        decimal zenithRad = DegToRad(zenith);
        decimal azDenom   = Cos(latitudeRad) * Sin(zenithRad);
        decimal azimuth;
        if (Abs(azDenom) > 0.001m) {
            decimal azRad = (Sin(latitudeRad) * Cos(zenithRad) - Sin(thetaRad)) / azDenom;
            if (Abs(azRad) > 1.0m) {
                azRad = azRad < 0 ? -1.0m : 1.0m;
            }

            azimuth = 180.0m - RadToDeg(Acos(azRad));
            if (hourAngle > 0.0m) {
                azimuth = -azimuth;
            }
        } else {
            azimuth = RadToDeg(latitudeRad) > 0.0m ? 180.0m : 0.0m;
        }

        if (azimuth < 0.0m) {
            azimuth += 360.0m;
        }

        return azimuth;
    }

    public static SolarPosition CalcAzEl(decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle, decimal sunDeclination) = CalcAzElCommon(t, localtime, latitude, longitude, zone);
        return new SolarPosition(Azimuth: CalcAz(zenith, latitudeRad, thetaRad, hourAngle), Elevation: CalcEl(zenith), Declination: sunDeclination);
    }

    private static (decimal zenith, decimal latitudeRad, decimal thetaRad, decimal hourAngle, decimal sunDeclination) CalcAzElCommon(
        decimal t, decimal localtime, decimal latitude, decimal longitude, decimal zone) {
        decimal latitudeRad    = DegToRad(latitude);
        decimal sunDeclination = CalcSunDeclination(t);
        decimal thetaRad       = DegToRad(sunDeclination);
        decimal trueSolarTime  = localtime + (CalcEquationOfTime(t) + 4.0m * longitude - 60.0m * zone);
        while (trueSolarTime > 1440) {
            trueSolarTime -= 1440;
        }

        decimal hourAngle = trueSolarTime / 4.0m - 180.0m;

        if (hourAngle < -180) {
            hourAngle += 360.0m;
        }

        decimal csz = Sin(latitudeRad) * Sin(thetaRad) + Cos(latitudeRad) * Cos(thetaRad) * Cos(DegToRad(hourAngle));
        switch (csz) {
            case > 1.0m:
                csz = 1.0m;
                break;
            case < -1.0m:
                csz = -1.0m;
                break;
        }

        decimal zenith = RadToDeg(Acos(csz));

        return (zenith, latitudeRad, thetaRad, hourAngle, sunDeclination);
    }

    /// Atmospheric Refraction correction
    private static decimal CalcRefraction(decimal elev) {
        decimal correction;
        if (elev > 85.0m) {
            correction = 0.0m;
        } else {
            decimal te = Tan(DegToRad(elev));
            correction = elev switch {
                > 5.0m    => 58.1m / te - 0.07m / (te * te * te) + 0.000086m / (te * te * te * te * te),
                > -0.575m => 1735.0m + elev * (-518.2m + elev * (103.4m + elev * (-12.79m + elev * 0.711m))),
                _         => -20.774m / te
            } / 3600.0m;
        }

        return correction;
    }

    private static decimal CalcEccentricityEarthOrbit(decimal t) => 0.016708634m - t * (0.000042037m + 0.0000001267m * t); // unitless

    private static decimal CalcSunEqOfCenter(decimal t) {
        decimal mrad = DegToRad(CalcGeomMeanAnomalySun(t));
        return Sin(mrad) * (1.914602m - t * (0.004817m + 0.000014m * t)) + Sin(mrad + mrad) * (0.019993m - 0.000101m * t) + Sin(mrad + mrad + mrad) * 0.000289m; // in degrees
    }

    private static decimal CalcGeomMeanAnomalySun(decimal t) => 357.52911m + t * (35999.05029m - 0.0001537m * t); // in degrees

    private static decimal CalcSunDeclination(decimal t) {
        decimal x = DegToRad(125.04m - 1934.136m * t);
        return RadToDeg(Asin(Sin(DegToRad(CalcObliquityCorrection(t, x))) * Sin(DegToRad(CalcSunApparentLong(t, x)))));
        // in degrees
    }

    private static decimal CalcSunApparentLong(decimal t, decimal x) => CalcSunTrueLong(t) - 0.00569m - 0.00478m * Sin(x); // in degrees

    private static decimal CalcSunTrueLong(decimal t) => CalcGeomMeanLongSun(t) + CalcSunEqOfCenter(t); // in degrees

    private static decimal CalcGeomMeanLongSun(decimal t) {
        decimal l0 = 280.46646m + t * (36000.76983m + t * 0.0003032m);
        while (l0 > 360.0m) {
            l0 -= 360.0m;
        }

        while (l0 < 0.0m) {
            l0 += 360.0m;
        }

        return l0; // in degrees
    }

    private static decimal CalcObliquityCorrection(decimal t, decimal x) => CalcMeanObliquityOfEcliptic(t) + 0.00256m * Cos(x); // in degrees

    private static decimal CalcMeanObliquityOfEcliptic(decimal t) => 23.0m + (26.0m + (21.448m - t * (46.8150m + t * (0.00059m - t * 0.001813m))) / 60.0m) / 60.0m; // in degrees

    public static decimal RadToDeg(decimal angleRad) => 180.0m * angleRad / DecimalMath.PI;

    public static decimal DegToRad(decimal angleDeg) => DecimalMath.PI * angleDeg / 180.0m;

    private static decimal CalcEquationOfTime(decimal t) {
        decimal l0Rad = DegToRad(CalcGeomMeanLongSun(t));
        decimal e     = CalcEccentricityEarthOrbit(t);
        decimal mRad  = DegToRad(CalcGeomMeanAnomalySun(t));
        decimal sinm  = Sin(mRad);
        decimal y     = Tan(DegToRad(CalcObliquityCorrection(t, DegToRad(125.04m - 1934.136m * t))) / 2.0m);
        y *= y;

        return RadToDeg(y * Sin(2.0m * l0Rad) - 2.0m * e * sinm + 4.0m * e * y * sinm * Cos(2.0m * l0Rad) - 0.5m * y * y * Sin(4.0m * l0Rad)
            - 1.25m * e * e * Sin(2.0m * mRad)) * 4.0m; // in minutes of time
    }

    public static decimal CalcSolNoon(decimal jd, decimal longitude, decimal timezone) {
        decimal tnoon         = CalcTimeJulianCent(jd - longitude / 360.0m);
        decimal eqTime        = CalcEquationOfTime(tnoon);
        decimal solNoonOffset = 720.0m - longitude * 4m - eqTime; // in minutes
        decimal newt          = CalcTimeJulianCent(jd - 0.5m + solNoonOffset / 1440.0m);
        eqTime = CalcEquationOfTime(newt);
        decimal solNoonLocal = 720 - longitude * 4 - eqTime + timezone * 60.0m; // in minutes
        while (solNoonLocal < 0.0m) {
            solNoonLocal += 1440.0m;
        }

        while (solNoonLocal >= 1440.0m) {
            solNoonLocal -= 1440.0m;
        }

        return solNoonLocal;
    }

}