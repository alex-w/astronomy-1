/*
    Astronomy Engine for C# / .NET.
    https://github.com/cosinekitty/astronomy

    MIT License

    Copyright (c) 2019 Don Cross <cosinekitty@gmail.com>

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using System;

namespace CosineKitty
{
    /// <summary>
    /// This exception is thrown by certain Astronomy Engine functions
    /// when an invalid attempt is made to use the Earth as the observed
    /// celestial body. Usually this happens for cases where the Earth itself
    /// is the location of the observer.
    /// </summary>
    public class EarthNotAllowedException: ArgumentException
    {
        /// <summary>Creates an exception indicating that the Earth is not allowed as a target body.</summary>
        public EarthNotAllowedException():
            base("The Earth is not allowed as the body parameter.")
            {}
    }

    /// <summary>
    /// The enumeration of celestial bodies supported by Astronomy Engine.
    /// </summary>
    public enum Body
    {
        /// <summary>
        /// A placeholder value representing an invalid or unknown celestial body.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// The planet Mercury.
        /// </summary>
        Mercury,

        /// <summary>
        /// The planet Venus.
        /// </summary>
        Venus,

        /// <summary>
        /// The planet Earth.
        /// Some functions that accept a `Body` parameter will fail if passed this value
        /// because they assume that an observation is being made from the Earth,
        /// and therefore the Earth is not a target of observation.
        /// </summary>
        Earth,

        /// <summary>
        /// The planet Mars.
        /// </summary>
        Mars,

        /// <summary>
        /// The planet Jupiter.
        /// </summary>
        Jupiter,

        /// <summary>
        /// The planet Saturn.
        /// </summary>
        Saturn,

        /// <summary>
        /// The planet Uranus.
        /// </summary>
        Uranus,

        /// <summary>
        /// The planet Neptune.
        /// </summary>
        Neptune,

        /// <summary>
        /// The planet Pluto.
        /// </summary>
        Pluto,

        /// <summary>
        /// The Sun.
        /// </summary>
        Sun,

        /// <summary>
        /// The Earth's natural satellite, the Moon.
        /// </summary>
        Moon,
    }

    /// <summary>
    /// A date and time used for astronomical calculations.
    /// </summary>
    public class AstroTime
    {
        private static readonly DateTime Origin = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// UT1/UTC number of days since noon on January 1, 2000.
        /// </summary>
        /// <remarks>
        /// The floating point number of days of Universal Time since noon UTC January 1, 2000.
        /// Astronomy Engine approximates UTC and UT1 as being the same thing, although they are
        /// not exactly equivalent; UTC and UT1 can disagree by up to plus or minus 0.9 seconds.
        /// This approximation is sufficient for the accuracy requirements of Astronomy Engine.
        ///
        /// Universal Time Coordinate (UTC) is the international standard for legal and civil
        /// timekeeping and replaces the older Greenwich Mean Time (GMT) standard.
        /// UTC is kept in sync with unpredictable observed changes in the Earth's rotation
        /// by occasionally adding leap seconds as needed.
        ///
        /// UT1 is an idealized time scale based on observed rotation of the Earth, which
        /// gradually slows down in an unpredictable way over time, due to tidal drag by the Moon and Sun,
        /// large scale weather events like hurricanes, and internal seismic and convection effects.
        /// Conceptually, UT1 drifts from atomic time continuously and erratically, whereas UTC
        /// is adjusted by a scheduled whole number of leap seconds as needed.
        ///
        /// The value in `ut` is appropriate for any calculation involving the Earth's rotation,
        /// such as calculating rise/set times, culumination, and anything involving apparent
        /// sidereal time.
        ///
        /// Before the era of atomic timekeeping, days based on the Earth's rotation
        /// were often known as *mean solar days*.
        /// </remarks>
        public readonly double ut;

        /// <summary>
        /// Terrestrial Time days since noon on January 1, 2000.
        /// </summary>
        /// <remarks>
        /// Terrestrial Time is an atomic time scale defined as a number of days since noon on January 1, 2000.
        /// In this system, days are not based on Earth rotations, but instead by
        /// the number of elapsed [SI seconds](https://physics.nist.gov/cuu/Units/second.html)
        /// divided by 86400. Unlike `ut`, `tt` increases uniformly without adjustments
        /// for changes in the Earth's rotation.
        ///
        /// The value in `tt` is used for calculations of movements not involving the Earth's rotation,
        /// such as the orbits of planets around the Sun, or the Moon around the Earth.
        ///
        /// Historically, Terrestrial Time has also been known by the term *Ephemeris Time* (ET).
        /// </remarks>
        public readonly double tt;

        internal double psi;    // For internal use only. Used to optimize Earth tilt calculations.
        internal double eps;    // For internal use only. Used to optimize Earth tilt calculations.

        /// <summary>
        /// Creates an `AstroTime` object from a Universal Time day value.
        /// </summary>
        /// <param name="ut">The number of days after the J2000 epoch.</param>
        public AstroTime(double ut)
        {
            this.ut = ut;
            this.tt = Astronomy.TerrestrialTime(ut);
            this.psi = this.eps = double.NaN;
        }

        /// <summary>
        /// Creates an `AstroTime` object from a .NET `DateTime` object.
        /// </summary>
        /// <param name="d">The date and time to be converted to AstroTime format.</param>
        public AstroTime(DateTime d)
            : this((d - Origin).TotalDays)
        {
        }

        /// <summary>
        /// Creates an `AstroTime` object from a UTC year, month, day, hour, minute and second.
        /// </summary>
        /// <param name="year">The UTC year value.</param>
        /// <param name="month">The UTC month value 1..12.</param>
        /// <param name="day">The UTC day of the month 1..31.</param>
        /// <param name="hour">The UTC hour value 0..23.</param>
        /// <param name="minute">The UTC minute value 0..59.</param>
        /// <param name="second">The UTC second value 0..59.</param>
        public AstroTime(int year, int month, int day, int hour, int minute, int second)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc))
        {
        }

        /// <summary>
        /// Converts this object to .NET `DateTime` format.
        /// </summary>
        /// <returns>a UTC `DateTime` object for this `AstroTime` value.</returns>
        public DateTime ToUtcDateTime()
        {
            return Origin.AddDays(ut).ToUniversalTime();
        }

        /// <summary>
        /// Converts this `AstroTime` to ISO 8601 format, expressed in UTC with millisecond resolution.
        /// </summary>
        /// <returns>Example: "2019-08-30T17:45:22.763".</returns>
        public override string ToString()
        {
            return ToUtcDateTime().ToString("yyyy-MM-ddThh:mm:ss.fffZ");
        }

        /// <summary>
        /// Calculates the sum or difference of an #AstroTime with a specified floating point number of days.
        /// </summary>
        /// <remarks>
        /// Sometimes we need to adjust a given #astro_time_t value by a certain amount of time.
        /// This function adds the given real number of days in `days` to the date and time in this object.
        ///
        /// More precisely, the result's Universal Time field `ut` is exactly adjusted by `days` and
        /// the Terrestrial Time field `tt` is adjusted correctly for the resulting UTC date and time,
        /// according to the historical and predictive Delta-T model provided by the
        /// [United States Naval Observatory](http://maia.usno.navy.mil/ser7/).
        /// </remarks>
        /// <param name="days">A floating point number of days by which to adjust `time`. May be negative, 0, or positive.</param>
        /// <returns>A date and time that is conceptually equal to `time + days`.</returns>
        public AstroTime AddDays(double days)
        {
            return new AstroTime(this.ut + days);
        }
    }

    /// <summary>
    /// A 3D Cartesian vector whose components are expressed in Astronomical Units (AU).
    /// </summary>
    public struct AstroVector
    {
        /// <summary>
        /// The Cartesian x-coordinate of the vector in AU.
        /// </summary>
        public readonly double x;

        /// <summary>
        /// The Cartesian y-coordinate of the vector in AU.
        /// </summary>
        public readonly double y;

        /// <summary>
        /// The Cartesian z-coordinate of the vector in AU.
        /// </summary>
        public readonly double z;

        /// <summary>
        /// The date and time at which this vector is valid.
        /// </summary>
        public readonly AstroTime t;

        /// <summary>
        /// Creates an AstroVector.
        /// </summary>
        /// <param name="x">A Cartesian x-coordinate expressed in AU.</param>
        /// <param name="y">A Cartesian y-coordinate expressed in AU.</param>
        /// <param name="z">A Cartesian z-coordinate expressed in AU.</param>
        /// <param name="t">The date and time at which this vector is valid.</param>
        public AstroVector(double x, double y, double z, AstroTime t)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.t = t;
        }

        /// <summary>
        /// Calculates the total distance in AU represented by this vector.
        /// </summary>
        /// <returns>The nonnegative length of the Cartisian vector in AU.</returns>
        public double Length()
        {
            return Math.Sqrt(x*x + y*y + z*z);
        }
    }

    /// <summary>
    /// The location of an observer on (or near) the surface of the Earth.
    /// </summary>
    /// <remarks>
    /// This structure is passed to functions that calculate phenomena as observed
    /// from a particular place on the Earth.
    /// </remarks>
    public class Observer
    {
        /// <summary>
        /// Geographic latitude in degrees north (positive) or south (negative) of the equator.
        /// </summary>
        public readonly double latitude;

        /// <summary>
        /// Geographic longitude in degrees east (positive) or west (negative) of the prime meridian at Greenwich, England.
        /// </summary>
        public readonly double longitude;

        /// <summary>
        /// The height above (positive) or below (negative) sea level, expressed in meters.
        /// </summary>
        public readonly double height;

        /// <summary>
        /// Creates an Observer object.
        /// </summary>
        /// <param name="latitude">Geographic latitude in degrees north (positive) or south (negative) of the equator.</param>
        /// <param name="longitude">Geographic longitude in degrees east (positive) or west (negative) of the prime meridian at Greenwich, England.</param>
        /// <param name="height">The height above (positive) or below (negative) sea level, expressed in meters.</param>
        public Observer(double latitude, double longitude, double height)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.height = height;
        }
    }

    /// <summary>
    /// Selects the date for which the Earth's equator is to be used for representing equatorial coordinates.
    /// </summary>
    /// <remarks>
    /// The Earth's equator is not always in the same plane due to precession and nutation.
    ///
    /// Sometimes it is useful to have a fixed plane of reference for equatorial coordinates
    /// across different calendar dates.  In these cases, a fixed *epoch*, or reference time,
    /// is helpful. Astronomy Engine provides the J2000 epoch for such cases.  This refers
    /// to the plane of the Earth's orbit as it was on noon UTC on 1 January 2000.
    ///
    /// For some other purposes, it is more helpful to represent coordinates using the Earth's
    /// equator exactly as it is on that date. For example, when calculating rise/set times
    /// or horizontal coordinates, it is most accurate to use the orientation of the Earth's
    /// equator at that same date and time. For these uses, Astronomy Engine allows *of-date*
    /// calculations.
    /// </remarks>
    public enum EquatorEpoch
    {
        /// <summary>
        /// Represent equatorial coordinates in the J2000 epoch.
        /// </summary>
        J2000,

        /// <summary>
        /// Represent equatorial coordinates using the Earth's equator at the given date and time.
        /// </summary>
        OfDate,
    }

    /// <summary>
    /// Aberration calculation options.
    /// </summary>
    /// <remarks>
    /// [Aberration](https://en.wikipedia.org/wiki/Aberration_of_light) is an effect
    /// causing the apparent direction of an observed body to be shifted due to transverse
    /// movement of the Earth with respect to the rays of light coming from that body.
    /// This angular correction can be anywhere from 0 to about 20 arcseconds,
    /// depending on the position of the observed body relative to the instantaneous
    /// velocity vector of the Earth.
    ///
    /// Some Astronomy Engine functions allow optional correction for aberration by
    /// passing in a value of this enumerated type.
    ///
    /// Aberration correction is useful to improve accuracy of coordinates of
    /// apparent locations of bodies seen from the Earth.
    /// However, because aberration affects not only the observed body (such as a planet)
    /// but the surrounding stars, aberration may be unhelpful (for example)
    /// for determining exactly when a planet crosses from one constellation to another.
    /// </remarks>
    public enum Aberration
    {
        /// <summary>
        /// Request correction for aberration.
        /// </summary>
        Corrected,

        /// <summary>
        /// Do not correct for aberration.
        /// </summary>
        None,
    }

    /// <summary>
    /// Selects whether to correct for atmospheric refraction, and if so, how.
    /// </summary>
    public enum Refraction
    {
        /// <summary>
        /// No atmospheric refraction correction (airless).
        /// </summary>
        None,

        /// <summary>
        /// Recommended correction for standard atmospheric refraction.
        /// </summary>
        Normal,

        /// <summary>
        /// Used only for compatibility testing with JPL Horizons online tool.
        /// </summary>
        JplHor,
    }

    /// <summary>
    /// Selects whether to search for a rising event or a setting event for a celestial body.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Indicates a rising event: a celestial body is observed to rise above the horizon by an observer on the Earth.
        /// </summary>
        Rise = +1,

        /// <summary>
        /// Indicates a setting event: a celestial body is observed to sink below the horizon by an observer on the Earth.
        /// </summary>
        Set = -1,
    }

    /// <summary>
    /// Indicates whether a body (especially Mercury or Venus) is best seen in the morning or evening.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// The body is best visible in the morning, before sunrise.
        /// </summary>
        Morning,

        /// <summary>
        /// The body is best visible in the evening, after sunset.
        /// </summary>
        Evening,
    }

    /// <summary>
    /// Equatorial angular coordinates.
    /// </summary>
    /// <remarks>
    /// Coordinates of a celestial body as seen from the Earth
    /// (geocentric or topocentric, depending on context),
    /// oriented with respect to the projection of the Earth's equator onto the sky.
    /// </remarks>
    public class Equatorial
    {
        /// <summary>
        /// Right ascension in sidereal hours.
        /// </summary>
        public readonly double ra;

        /// <summary>
        /// Declination in degrees.
        /// </summary>
        public readonly double dec;

        /// <summary>
        /// Distance to the celestial body in AU.
        /// </summary>
        public readonly double dist;

        /// <summary>
        /// Creates an equatorial coordinates object.
        /// </summary>
        /// <param name="ra">Right ascension in sidereal hours.</param>
        /// <param name="dec">Declination in degrees.</param>
        /// <param name="dist">Distance to the celestial body in AU.</param>
        public Equatorial(double ra, double dec, double dist)
        {
            this.ra = ra;
            this.dec = dec;
            this.dist = dist;
        }
    }

    /// <summary>
    /// Ecliptic angular and Cartesian coordinates.
    /// </summary>
    /// <remarks>
    /// Coordinates of a celestial body as seen from the center of the Sun (heliocentric),
    /// oriented with respect to the plane of the Earth's orbit around the Sun (the ecliptic).
    /// </remarks>
    public class Ecliptic
    {
        /// <summary>
        /// Cartesian x-coordinate: in the direction of the equinox along the ecliptic plane.
        /// </summary>
        public readonly double ex;

        /// <summary>
        /// Cartesian y-coordinate: in the ecliptic plane 90 degrees prograde from the equinox.
        /// </summary>
        public readonly double ey;

        /// <summary>
        /// Cartesian z-coordinate: perpendicular to the ecliptic plane. Positive is north.
        /// </summary>
        public readonly double ez;

        /// <summary>
        /// Latitude in degrees north (positive) or south (negative) of the ecliptic plane.
        /// </summary>
        public readonly double elat;

        /// <summary>
        /// Longitude in degrees around the ecliptic plane prograde from the equinox.
        /// </summary>
        public readonly double elon;

        /// <summary>
        /// Creates an object that holds Cartesian and angular ecliptic coordinates.
        /// </summary>
        /// <param name="ex">x-coordinate of the ecliptic position</param>
        /// <param name="ey">y-coordinate of the ecliptic position</param>
        /// <param name="ez">z-coordinate of the ecliptic position</param>
        /// <param name="elat">ecliptic latitude</param>
        /// <param name="elon">ecliptic longitude</param>
        public Ecliptic(double ex, double ey, double ez, double elat, double elon)
        {
            this.ex = ex;
            this.ey = ey;
            this.ez = ez;
            this.elat = elat;
            this.elon = elon;
        }
    }

    /// <summary>
    /// Coordinates of a celestial body as seen by a topocentric observer.
    /// </summary>
    /// <remarks>
    /// Contains horizontal and equatorial coordinates seen by an observer on or near
    /// the surface of the Earth (a topocentric observer).
    /// Optionally corrected for atmospheric refraction.
    /// </remarks>
    public struct Topocentric
    {
        /// <summary>
        /// Compass direction around the horizon in degrees. 0=North, 90=East, 180=South, 270=West.
        /// </summary>
        public readonly double azimuth;

        /// <summary>
        /// Angle in degrees above (positive) or below (negative) the observer's horizon.
        /// </summary>
        public readonly double altitude;

        /// <summary>
        /// Right ascension in sidereal hours.
        /// </summary>
        public readonly double ra;

        /// <summary>
        /// Declination in degrees.
        /// </summary>
        public readonly double dec;

        /// <summary>
        /// Creates a topocentric position object.
        /// </summary>
        /// <param name="azimuth">Compass direction around the horizon in degrees. 0=North, 90=East, 180=South, 270=West.</param>
        /// <param name="altitude">Angle in degrees above (positive) or below (negative) the observer's horizon.</param>
        /// <param name="ra">Right ascension in sidereal hours.</param>
        /// <param name="dec">Declination in degrees.</param>
        public Topocentric(double azimuth, double altitude, double ra, double dec)
        {
            this.azimuth = azimuth;
            this.altitude = altitude;
            this.ra = ra;
            this.dec = dec;
        }
    }

    /// <summary>
    /// The dates and times of changes of season for a given calendar year.
    /// Call #Seasons to calculate this data structure for a given year.
    /// </summary>
    public struct SeasonsInfo
    {
        /// <summary>
        /// The date and time of the March equinox for the specified year.
        /// </summary>
        public readonly AstroTime mar_equinox;

        /// <summary>
        /// The date and time of the June soltice for the specified year.
        /// </summary>
        public readonly AstroTime jun_solstice;

        /// <summary>
        /// The date and time of the September equinox for the specified year.
        /// </summary>
        public readonly AstroTime sep_equinox;

        /// <summary>
        /// The date and time of the December solstice for the specified year.
        /// </summary>
        public readonly AstroTime dec_solstice;

        internal SeasonsInfo(AstroTime mar_equinox, AstroTime jun_solstice, AstroTime sep_equinox, AstroTime dec_solstice)
        {
            this.mar_equinox = mar_equinox;
            this.jun_solstice = jun_solstice;
            this.sep_equinox = sep_equinox;
            this.dec_solstice = dec_solstice;
        }
    }

    /// <summary>
    /// A lunar quarter event (new moon, first quarter, full moon, or third quarter) along with its date and time.
    /// </summary>
    public struct MoonQuarterInfo
    {
        /// <summary>
        /// 0=new moon, 1=first quarter, 2=full moon, 3=third quarter.
        /// </summary>
        public readonly int quarter;

        /// <summary>
        /// The date and time of the lunar quarter.
        /// </summary>
        public readonly AstroTime time;

        internal MoonQuarterInfo(int quarter, AstroTime time)
        {
            this.quarter = quarter;
            this.time = time;
        }
    }

    /// <summary>
    /// Information about a celestial body crossing a specific hour angle.
    /// </summary>
    /// <remarks>
    /// Returned by the function #SearchHourAngle to report information about
    /// a celestial body crossing a certain hour angle as seen by a specified topocentric observer.
    /// </remarks>
    public struct HourAngleInfo
    {
        /// <summary>The date and time when the body crosses the specified hour angle.</summary>
        public readonly AstroTime time;

        /// <summary>Apparent coordinates of the body at the time it crosses the specified hour angle.</summary>
        public readonly Topocentric hor;

        /// <summary>
        /// Creates a struct that represents a celestial body crossing a specific hour angle.
        /// </summary>
        /// <param name="time">The date and time when the body crosses the specified hour angle.</param>
        /// <param name="hor">Apparent coordinates of the body at the time it crosses the specified hour angle.</param>
        public HourAngleInfo(AstroTime time, Topocentric hor)
        {
            this.time = time;
            this.hor = hor;
        }
    }

    /// <summary>
    /// Contains information about the visibility of a celestial body at a given date and time.
    /// See #Elongation for more detailed information about the members of this structure.
    /// See also #SearchMaxElongation for how to search for maximum elongation events.
    /// </summary>
    public struct ElongationInfo
    {
        /// <summary>The date and time of the observation.</summary>
        public readonly AstroTime time;

        /// <summary>Whether the body is best seen in the morning or the evening.</summary>
        public readonly Visibility visibility;

        /// <summary>The angle in degrees between the body and the Sun, as seen from the Earth.</summary>
        public readonly double elongation;

        /// <summary>The difference between the ecliptic longitudes of the body and the Sun, as seen from the Earth.</summary>
        public readonly double ecliptic_separation;

        /// <summary>
        /// Creates a structure that represents an elongation event.
        /// </summary>
        /// <param name="time">The date and time of the observation.</param>
        /// <param name="visibility">Whether the body is best seen in the morning or the evening.</param>
        /// <param name="elongation">The angle in degrees between the body and the Sun, as seen from the Earth.</param>
        /// <param name="ecliptic_separation">The difference between the ecliptic longitudes of the body and the Sun, as seen from the Earth.</param>
        public ElongationInfo(AstroTime time, Visibility visibility, double elongation, double ecliptic_separation)
        {
            this.time = time;
            this.visibility = visibility;
            this.elongation = elongation;
            this.ecliptic_separation = ecliptic_separation;
        }
    }

    /// <summary>
    /// The type of apsis: pericenter (closest approach) or apocenter (farthest distance).
    /// </summary>
    public enum ApsisKind
    {
        /// <summary>The body is at its closest approach to the object it orbits.</summary>
        Pericenter,

        /// <summary>The body is at its farthest distance from the object it orbits.</summary>
        Apocenter,
    }

    /// <summary>
    /// An apsis event: pericenter (closest approach) or apocenter (farthest distance).
    /// </summary>
    /// <remarks>
    /// For the Moon orbiting the Earth, or a planet orbiting the Sun, an *apsis* is an
    /// event where the orbiting body reaches its closest or farthest point from the primary body.
    /// The closest approach is called *pericenter* and the farthest point is *apocenter*.
    ///
    /// More specific terminology is common for particular orbiting bodies.
    /// The Moon's closest approach to the Earth is called *perigee* and its farthest
    /// point is called *apogee*. The closest approach of a planet to the Sun is called
    /// *perihelion* and the furthest point is called *aphelion*.
    ///
    /// This data structure is returned by #Astronomy.SearchLunarApsis and #Astronomy.NextLunarApsis
    /// to iterate through consecutive alternating perigees and apogees.
    /// </remarks>
    public struct ApsisInfo
    {
        /// <summary>The date and time of the apsis.</summary>
        public readonly AstroTime time;

        /// <summary>Whether this is a pericenter or apocenter event.</summary>
        public readonly ApsisKind kind;

        /// <summary>The distance between the centers of the bodies in astronomical units.</summary>
        public readonly double dist_au;

        /// <summary>The distance between the centers of the bodies in kilometers.</summary>
        public readonly double dist_km;

        internal ApsisInfo(AstroTime time, ApsisKind kind, double dist_au)
        {
            this.time = time;
            this.kind = kind;
            this.dist_au = dist_au;
            this.dist_km = dist_au * Astronomy.KM_PER_AU;
        }
    }

    /// <summary>
    /// Information about the brightness and illuminated shape of a celestial body.
    /// </summary>
    /// <remarks>
    /// Returned by the functions #Astronomy.Illumination and #Astronomy.SearchPeakMagnitude
    /// to report the visual magnitude and illuminated fraction of a celestial body at a given date and time.
    /// </remarks>
    public struct IllumInfo
    {
        /// <summary>The date and time of the observation.</summary>
        public readonly AstroTime time;

        /// <summary>The visual magnitude of the body. Smaller values are brighter.</summary>
        public readonly double  mag;

        /// <summary>The angle in degrees between the Sun and the Earth, as seen from the body. Indicates the body's phase as seen from the Earth.</summary>
        public readonly double phase_angle;

        /// <summary>The distance between the Sun and the body at the observation time.</summary>
        public readonly double helio_dist;

        /// <summary>For Saturn, the tilt angle in degrees of its rings as seen from Earth. For all other bodies, 0.</summary>
        public readonly double ring_tilt;

        internal IllumInfo(AstroTime time, double mag, double phase_angle, double helio_dist, double ring_tilt)
        {
            this.time = time;
            this.mag = mag;
            this.phase_angle = phase_angle;
            this.helio_dist = helio_dist;
            this.ring_tilt = ring_tilt;
        }
    }

    /// <summary>
    /// The wrapper class that holds Astronomy Engine functions.
    /// </summary>
    public static class Astronomy
    {
        private const double T0 = 2451545.0;
        private const double MJD_BASIS = 2400000.5;
        private const double Y2000_IN_MJD  =  T0 - MJD_BASIS;
        internal const double DEG2RAD = 0.017453292519943296;
        internal const double RAD2DEG = 57.295779513082321;
        private const double ASEC360 = 1296000.0;
        private const double ASEC2RAD = 4.848136811095359935899141e-6;
        internal const double PI2 = 2.0 * Math.PI;
        internal const double ARC = 3600.0 * 180.0 / Math.PI;     /* arcseconds per radian */
        private const double C_AUDAY = 173.1446326846693;        /* speed of light in AU/day */
        internal const double ERAD = 6378136.6;                   /* mean earth radius in meters */
        internal const double AU = 1.4959787069098932e+11;        /* astronomical unit in meters */
        internal const double KM_PER_AU = 1.4959787069098932e+8;
        private const double ANGVEL = 7.2921150e-5;
        private const double SECONDS_PER_DAY = 24.0 * 3600.0;
        private const double SOLAR_DAYS_PER_SIDEREAL_DAY = 0.9972695717592592;
        private const double MEAN_SYNODIC_MONTH = 29.530588;     /* average number of days for Moon to return to the same phase */
        private const double EARTH_ORBITAL_PERIOD = 365.256;
        internal const double REFRACTION_NEAR_HORIZON = 34.0 / 60.0;   /* degrees of refractive "lift" seen for objects near horizon */
        internal const double SUN_RADIUS_AU  = 4.6505e-3;
        internal const double MOON_RADIUS_AU = 1.15717e-5;
        private const double ASEC180 = 180.0 * 60.0 * 60.0;         /* arcseconds per 180 degrees (or pi radians) */
        private const double AU_PER_PARSEC = (ASEC180 / Math.PI);   /* exact definition of how many AU = one parsec */

        internal static double LongitudeOffset(double diff)
        {
            double offset = diff;

            while (offset <= -180.0)
                offset += 360.0;

            while (offset > 180.0)
                offset -= 360.0;

            return offset;
        }

        internal static double NormalizeLongitude(double lon)
        {
            while (lon < 0.0)
                lon += 360.0;

            while (lon >= 360.0)
                lon -= 360.0;

            return lon;
        }

        private struct deltat_entry_t
        {
            public double mjd;
            public double dt;
        }

        private static readonly deltat_entry_t[] DT = new deltat_entry_t[]
        {
            new deltat_entry_t { mjd=-72638.0, dt=38 },
            new deltat_entry_t { mjd=-65333.0, dt=26 },
            new deltat_entry_t { mjd=-58028.0, dt=21 },
            new deltat_entry_t { mjd=-50724.0, dt=21.1 },
            new deltat_entry_t { mjd=-43419.0, dt=13.5 },
            new deltat_entry_t { mjd=-39766.0, dt=13.7 },
            new deltat_entry_t { mjd=-36114.0, dt=14.8 },
            new deltat_entry_t { mjd=-32461.0, dt=15.7 },
            new deltat_entry_t { mjd=-28809.0, dt=15.6 },
            new deltat_entry_t { mjd=-25156.0, dt=13.3 },
            new deltat_entry_t { mjd=-21504.0, dt=12.6 },
            new deltat_entry_t { mjd=-17852.0, dt=11.2 },
            new deltat_entry_t { mjd=-14200.0, dt=11.13 },
            new deltat_entry_t { mjd=-10547.0, dt=7.95 },
            new deltat_entry_t { mjd=-6895.0, dt=6.22 },
            new deltat_entry_t { mjd=-3242.0, dt=6.55 },
            new deltat_entry_t { mjd=-1416.0, dt=7.26 },
            new deltat_entry_t { mjd=410.0, dt=7.35 },
            new deltat_entry_t { mjd=2237.0, dt=5.92 },
            new deltat_entry_t { mjd=4063.0, dt=1.04 },
            new deltat_entry_t { mjd=5889.0, dt=-3.19 },
            new deltat_entry_t { mjd=7715.0, dt=-5.36 },
            new deltat_entry_t { mjd=9542.0, dt=-5.74 },
            new deltat_entry_t { mjd=11368.0, dt=-5.86 },
            new deltat_entry_t { mjd=13194.0, dt=-6.41 },
            new deltat_entry_t { mjd=15020.0, dt=-2.70 },
            new deltat_entry_t { mjd=16846.0, dt=3.92 },
            new deltat_entry_t { mjd=18672.0, dt=10.38 },
            new deltat_entry_t { mjd=20498.0, dt=17.19 },
            new deltat_entry_t { mjd=22324.0, dt=21.41 },
            new deltat_entry_t { mjd=24151.0, dt=23.63 },
            new deltat_entry_t { mjd=25977.0, dt=24.02 },
            new deltat_entry_t { mjd=27803.0, dt=23.91 },
            new deltat_entry_t { mjd=29629.0, dt=24.35 },
            new deltat_entry_t { mjd=31456.0, dt=26.76 },
            new deltat_entry_t { mjd=33282.0, dt=29.15 },
            new deltat_entry_t { mjd=35108.0, dt=31.07 },
            new deltat_entry_t { mjd=36934.0, dt=33.150 },
            new deltat_entry_t { mjd=38761.0, dt=35.738 },
            new deltat_entry_t { mjd=40587.0, dt=40.182 },
            new deltat_entry_t { mjd=42413.0, dt=45.477 },
            new deltat_entry_t { mjd=44239.0, dt=50.540 },
            new deltat_entry_t { mjd=44605.0, dt=51.3808 },
            new deltat_entry_t { mjd=44970.0, dt=52.1668 },
            new deltat_entry_t { mjd=45335.0, dt=52.9565 },
            new deltat_entry_t { mjd=45700.0, dt=53.7882 },
            new deltat_entry_t { mjd=46066.0, dt=54.3427 },
            new deltat_entry_t { mjd=46431.0, dt=54.8712 },
            new deltat_entry_t { mjd=46796.0, dt=55.3222 },
            new deltat_entry_t { mjd=47161.0, dt=55.8197 },
            new deltat_entry_t { mjd=47527.0, dt=56.3000 },
            new deltat_entry_t { mjd=47892.0, dt=56.8553 },
            new deltat_entry_t { mjd=48257.0, dt=57.5653 },
            new deltat_entry_t { mjd=48622.0, dt=58.3092 },
            new deltat_entry_t { mjd=48988.0, dt=59.1218 },
            new deltat_entry_t { mjd=49353.0, dt=59.9845 },
            new deltat_entry_t { mjd=49718.0, dt=60.7853 },
            new deltat_entry_t { mjd=50083.0, dt=61.6287 },
            new deltat_entry_t { mjd=50449.0, dt=62.2950 },
            new deltat_entry_t { mjd=50814.0, dt=62.9659 },
            new deltat_entry_t { mjd=51179.0, dt=63.4673 },
            new deltat_entry_t { mjd=51544.0, dt=63.8285 },
            new deltat_entry_t { mjd=51910.0, dt=64.0908 },
            new deltat_entry_t { mjd=52275.0, dt=64.2998 },
            new deltat_entry_t { mjd=52640.0, dt=64.4734 },
            new deltat_entry_t { mjd=53005.0, dt=64.5736 },
            new deltat_entry_t { mjd=53371.0, dt=64.6876 },
            new deltat_entry_t { mjd=53736.0, dt=64.8452 },
            new deltat_entry_t { mjd=54101.0, dt=65.1464 },
            new deltat_entry_t { mjd=54466.0, dt=65.4573 },
            new deltat_entry_t { mjd=54832.0, dt=65.7768 },
            new deltat_entry_t { mjd=55197.0, dt=66.0699 },
            new deltat_entry_t { mjd=55562.0, dt=66.3246 },
            new deltat_entry_t { mjd=55927.0, dt=66.6030 },
            new deltat_entry_t { mjd=56293.0, dt=66.9069 },
            new deltat_entry_t { mjd=56658.0, dt=67.2810 },
            new deltat_entry_t { mjd=57023.0, dt=67.6439 },
            new deltat_entry_t { mjd=57388.0, dt=68.1024 },
            new deltat_entry_t { mjd=57754.0, dt=68.5927 },
            new deltat_entry_t { mjd=58119.0, dt=68.9676 },
            new deltat_entry_t { mjd=58484.0, dt=69.2201 },
            new deltat_entry_t { mjd=58849.0, dt=69.87 },
            new deltat_entry_t { mjd=59214.0, dt=70.39 },
            new deltat_entry_t { mjd=59580.0, dt=70.91 },
            new deltat_entry_t { mjd=59945.0, dt=71.40 },
            new deltat_entry_t { mjd=60310.0, dt=71.88 },
            new deltat_entry_t { mjd=60675.0, dt=72.36 },
            new deltat_entry_t { mjd=61041.0, dt=72.83 },
            new deltat_entry_t { mjd=61406.0, dt=73.32 },
            new deltat_entry_t { mjd=61680.0, dt=73.66 }
        };

        private struct vsop_term_t
        {
            public double amplitude;
            public double phase;
            public double frequency;

            public vsop_term_t(double amplitude, double phase, double frequency)
            {
                this.amplitude = amplitude;
                this.phase = phase;
                this.frequency = frequency;
            }
        }

        private struct vsop_series_t
        {
            public vsop_term_t[] term;

            public vsop_series_t(vsop_term_t[] term)
            {
                this.term = term;
            }
        }

        private struct vsop_formula_t
        {
            public vsop_series_t[] series;

            public vsop_formula_t(vsop_series_t[] series)
            {
                this.series = series;
            }
        }

        private struct vsop_model_t
        {
            public vsop_formula_t lat;
            public vsop_formula_t lon;
            public vsop_formula_t rad;

            public vsop_model_t(vsop_series_t[] lat, vsop_series_t[] lon, vsop_series_t[] rad)
            {
                this.lat = new vsop_formula_t(lat);
                this.lon = new vsop_formula_t(lon);
                this.rad = new vsop_formula_t(rad);
            }
        };

        private static readonly vsop_term_t[] vsop_lat_Mercury_0 = new vsop_term_t[]
        {
            new vsop_term_t(4.40250710144, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.40989414977, 1.48302034195, 26087.90314157420),
            new vsop_term_t(0.05046294200, 4.47785489551, 52175.80628314840),
            new vsop_term_t(0.00855346844, 1.16520322459, 78263.70942472259),
            new vsop_term_t(0.00165590362, 4.11969163423, 104351.61256629678),
            new vsop_term_t(0.00034561897, 0.77930768443, 130439.51570787099),
            new vsop_term_t(0.00007583476, 3.71348404924, 156527.41884944518)
        };

        private static readonly vsop_term_t[] vsop_lat_Mercury_1 = new vsop_term_t[]
        {
            new vsop_term_t(26087.90313685529, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01131199811, 6.21874197797, 26087.90314157420),
            new vsop_term_t(0.00292242298, 3.04449355541, 52175.80628314840),
            new vsop_term_t(0.00075775081, 6.08568821653, 78263.70942472259),
            new vsop_term_t(0.00019676525, 2.80965111777, 104351.61256629678)
        };

        private static readonly vsop_series_t[] vsop_lat_Mercury = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Mercury_0),
            new vsop_series_t(vsop_lat_Mercury_1)
        };

        private static readonly vsop_term_t[] vsop_lon_Mercury_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.11737528961, 1.98357498767, 26087.90314157420),
            new vsop_term_t(0.02388076996, 5.03738959686, 52175.80628314840),
            new vsop_term_t(0.01222839532, 3.14159265359, 0.00000000000),
            new vsop_term_t(0.00543251810, 1.79644363964, 78263.70942472259),
            new vsop_term_t(0.00129778770, 4.83232503958, 104351.61256629678),
            new vsop_term_t(0.00031866927, 1.58088495658, 130439.51570787099),
            new vsop_term_t(0.00007963301, 4.60972126127, 156527.41884944518)
        };

        private static readonly vsop_term_t[] vsop_lon_Mercury_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00274646065, 3.95008450011, 26087.90314157420),
            new vsop_term_t(0.00099737713, 3.14159265359, 0.00000000000)
        };

        private static readonly vsop_series_t[] vsop_lon_Mercury = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Mercury_0),
            new vsop_series_t(vsop_lon_Mercury_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Mercury_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.39528271651, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.07834131818, 6.19233722598, 26087.90314157420),
            new vsop_term_t(0.00795525558, 2.95989690104, 52175.80628314840),
            new vsop_term_t(0.00121281764, 6.01064153797, 78263.70942472259),
            new vsop_term_t(0.00021921969, 2.77820093972, 104351.61256629678),
            new vsop_term_t(0.00004354065, 5.82894543774, 130439.51570787099)
        };

        private static readonly vsop_term_t[] vsop_rad_Mercury_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00217347740, 4.65617158665, 26087.90314157420),
            new vsop_term_t(0.00044141826, 1.42385544001, 52175.80628314840)
        };

        private static readonly vsop_series_t[] vsop_rad_Mercury = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Mercury_0),
            new vsop_series_t(vsop_rad_Mercury_1)
        };


        private static readonly vsop_term_t[] vsop_lat_Venus_0 = new vsop_term_t[]
        {
            new vsop_term_t(3.17614666774, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01353968419, 5.59313319619, 10213.28554621100),
            new vsop_term_t(0.00089891645, 5.30650047764, 20426.57109242200),
            new vsop_term_t(0.00005477194, 4.41630661466, 7860.41939243920),
            new vsop_term_t(0.00003455741, 2.69964447820, 11790.62908865880),
            new vsop_term_t(0.00002372061, 2.99377542079, 3930.20969621960),
            new vsop_term_t(0.00001317168, 5.18668228402, 26.29831979980),
            new vsop_term_t(0.00001664146, 4.25018630147, 1577.34354244780),
            new vsop_term_t(0.00001438387, 4.15745084182, 9683.59458111640),
            new vsop_term_t(0.00001200521, 6.15357116043, 30639.85663863300)
        };

        private static readonly vsop_term_t[] vsop_lat_Venus_1 = new vsop_term_t[]
        {
            new vsop_term_t(10213.28554621638, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00095617813, 2.46406511110, 10213.28554621100),
            new vsop_term_t(0.00007787201, 0.62478482220, 20426.57109242200)
        };

        private static readonly vsop_series_t[] vsop_lat_Venus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Venus_0),
            new vsop_series_t(vsop_lat_Venus_1)
        };

        private static readonly vsop_term_t[] vsop_lon_Venus_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.05923638472, 0.26702775812, 10213.28554621100),
            new vsop_term_t(0.00040107978, 1.14737178112, 20426.57109242200),
            new vsop_term_t(0.00032814918, 3.14159265359, 0.00000000000)
        };

        private static readonly vsop_term_t[] vsop_lon_Venus_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00287821243, 1.88964962838, 10213.28554621100)
        };

        private static readonly vsop_series_t[] vsop_lon_Venus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Venus_0),
            new vsop_series_t(vsop_lon_Venus_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Venus_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.72334820891, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00489824182, 4.02151831717, 10213.28554621100),
            new vsop_term_t(0.00001658058, 4.90206728031, 20426.57109242200)
        };

        private static readonly vsop_term_t[] vsop_rad_Venus_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00034551041, 0.89198706276, 10213.28554621100)
        };

        private static readonly vsop_series_t[] vsop_rad_Venus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Venus_0),
            new vsop_series_t(vsop_rad_Venus_1)
        };


        private static readonly vsop_term_t[] vsop_lat_Earth_0 = new vsop_term_t[]
        {
            new vsop_term_t(1.75347045673, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.03341656453, 4.66925680415, 6283.07584999140),
            new vsop_term_t(0.00034894275, 4.62610242189, 12566.15169998280),
            new vsop_term_t(0.00003417572, 2.82886579754, 3.52311834900),
            new vsop_term_t(0.00003497056, 2.74411783405, 5753.38488489680),
            new vsop_term_t(0.00003135899, 3.62767041756, 77713.77146812050),
            new vsop_term_t(0.00002676218, 4.41808345438, 7860.41939243920),
            new vsop_term_t(0.00002342691, 6.13516214446, 3930.20969621960),
            new vsop_term_t(0.00001273165, 2.03709657878, 529.69096509460),
            new vsop_term_t(0.00001324294, 0.74246341673, 11506.76976979360),
            new vsop_term_t(0.00000901854, 2.04505446477, 26.29831979980),
            new vsop_term_t(0.00001199167, 1.10962946234, 1577.34354244780),
            new vsop_term_t(0.00000857223, 3.50849152283, 398.14900340820),
            new vsop_term_t(0.00000779786, 1.17882681962, 5223.69391980220),
            new vsop_term_t(0.00000990250, 5.23268072088, 5884.92684658320),
            new vsop_term_t(0.00000753141, 2.53339052847, 5507.55323866740),
            new vsop_term_t(0.00000505267, 4.58292599973, 18849.22754997420),
            new vsop_term_t(0.00000492392, 4.20505711826, 775.52261132400),
            new vsop_term_t(0.00000356672, 2.91954114478, 0.06731030280),
            new vsop_term_t(0.00000284125, 1.89869240932, 796.29800681640),
            new vsop_term_t(0.00000242879, 0.34481445893, 5486.77784317500),
            new vsop_term_t(0.00000317087, 5.84901948512, 11790.62908865880),
            new vsop_term_t(0.00000271112, 0.31486255375, 10977.07880469900),
            new vsop_term_t(0.00000206217, 4.80646631478, 2544.31441988340),
            new vsop_term_t(0.00000205478, 1.86953770281, 5573.14280143310),
            new vsop_term_t(0.00000202318, 2.45767790232, 6069.77675455340),
            new vsop_term_t(0.00000126225, 1.08295459501, 20.77539549240),
            new vsop_term_t(0.00000155516, 0.83306084617, 213.29909543800)
        };

        private static readonly vsop_term_t[] vsop_lat_Earth_1 = new vsop_term_t[]
        {
            new vsop_term_t(6283.07584999140, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00206058863, 2.67823455808, 6283.07584999140),
            new vsop_term_t(0.00004303419, 2.63512233481, 12566.15169998280)
        };

        private static readonly vsop_term_t[] vsop_lat_Earth_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00008721859, 1.07253635559, 6283.07584999140)
        };

        private static readonly vsop_series_t[] vsop_lat_Earth = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Earth_0),
            new vsop_series_t(vsop_lat_Earth_1),
            new vsop_series_t(vsop_lat_Earth_2)
        };

        private static readonly vsop_term_t[] vsop_lon_Earth_0 = new vsop_term_t[]
        {
        };

        private static readonly vsop_term_t[] vsop_lon_Earth_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00227777722, 3.41376620530, 6283.07584999140),
            new vsop_term_t(0.00003805678, 3.37063423795, 12566.15169998280)
        };

        private static readonly vsop_series_t[] vsop_lon_Earth = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Earth_0),
            new vsop_series_t(vsop_lon_Earth_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Earth_0 = new vsop_term_t[]
        {
            new vsop_term_t(1.00013988784, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01670699632, 3.09846350258, 6283.07584999140),
            new vsop_term_t(0.00013956024, 3.05524609456, 12566.15169998280),
            new vsop_term_t(0.00003083720, 5.19846674381, 77713.77146812050),
            new vsop_term_t(0.00001628463, 1.17387558054, 5753.38488489680),
            new vsop_term_t(0.00001575572, 2.84685214877, 7860.41939243920),
            new vsop_term_t(0.00000924799, 5.45292236722, 11506.76976979360),
            new vsop_term_t(0.00000542439, 4.56409151453, 3930.20969621960),
            new vsop_term_t(0.00000472110, 3.66100022149, 5884.92684658320)
        };

        private static readonly vsop_term_t[] vsop_rad_Earth_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00103018607, 1.10748968172, 6283.07584999140),
            new vsop_term_t(0.00001721238, 1.06442300386, 12566.15169998280)
        };

        private static readonly vsop_term_t[] vsop_rad_Earth_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00004359385, 5.78455133808, 6283.07584999140)
        };

        private static readonly vsop_series_t[] vsop_rad_Earth = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Earth_0),
            new vsop_series_t(vsop_rad_Earth_1),
            new vsop_series_t(vsop_rad_Earth_2)
        };


        private static readonly vsop_term_t[] vsop_lat_Mars_0 = new vsop_term_t[]
        {
            new vsop_term_t(6.20347711581, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.18656368093, 5.05037100270, 3340.61242669980),
            new vsop_term_t(0.01108216816, 5.40099836344, 6681.22485339960),
            new vsop_term_t(0.00091798406, 5.75478744667, 10021.83728009940),
            new vsop_term_t(0.00027744987, 5.97049513147, 3.52311834900),
            new vsop_term_t(0.00010610235, 2.93958560338, 2281.23049651060),
            new vsop_term_t(0.00012315897, 0.84956094002, 2810.92146160520),
            new vsop_term_t(0.00008926784, 4.15697846427, 0.01725365220),
            new vsop_term_t(0.00008715691, 6.11005153139, 13362.44970679920),
            new vsop_term_t(0.00006797556, 0.36462229657, 398.14900340820),
            new vsop_term_t(0.00007774872, 3.33968761376, 5621.84292321040),
            new vsop_term_t(0.00003575078, 1.66186505710, 2544.31441988340),
            new vsop_term_t(0.00004161108, 0.22814971327, 2942.46342329160),
            new vsop_term_t(0.00003075252, 0.85696614132, 191.44826611160),
            new vsop_term_t(0.00002628117, 0.64806124465, 3337.08930835080),
            new vsop_term_t(0.00002937546, 6.07893711402, 0.06731030280),
            new vsop_term_t(0.00002389414, 5.03896442664, 796.29800681640),
            new vsop_term_t(0.00002579844, 0.02996736156, 3344.13554504880),
            new vsop_term_t(0.00001528141, 1.14979301996, 6151.53388830500),
            new vsop_term_t(0.00001798806, 0.65634057445, 529.69096509460),
            new vsop_term_t(0.00001264357, 3.62275122593, 5092.15195811580),
            new vsop_term_t(0.00001286228, 3.06796065034, 2146.16541647520),
            new vsop_term_t(0.00001546404, 2.91579701718, 1751.53953141600),
            new vsop_term_t(0.00001024902, 3.69334099279, 8962.45534991020),
            new vsop_term_t(0.00000891566, 0.18293837498, 16703.06213349900),
            new vsop_term_t(0.00000858759, 2.40093811940, 2914.01423582380),
            new vsop_term_t(0.00000832715, 2.46418619474, 3340.59517304760),
            new vsop_term_t(0.00000832720, 4.49495782139, 3340.62968035200),
            new vsop_term_t(0.00000712902, 3.66335473479, 1059.38193018920),
            new vsop_term_t(0.00000748723, 3.82248614017, 155.42039943420),
            new vsop_term_t(0.00000723861, 0.67497311481, 3738.76143010800),
            new vsop_term_t(0.00000635548, 2.92182225127, 8432.76438481560),
            new vsop_term_t(0.00000655162, 0.48864064125, 3127.31333126180),
            new vsop_term_t(0.00000550474, 3.81001042328, 0.98032106820),
            new vsop_term_t(0.00000552750, 4.47479317037, 1748.01641306700),
            new vsop_term_t(0.00000425966, 0.55364317304, 6283.07584999140),
            new vsop_term_t(0.00000415131, 0.49662285038, 213.29909543800),
            new vsop_term_t(0.00000472167, 3.62547124025, 1194.44701022460),
            new vsop_term_t(0.00000306551, 0.38052848348, 6684.74797174860),
            new vsop_term_t(0.00000312141, 0.99853944405, 6677.70173505060),
            new vsop_term_t(0.00000293198, 4.22131299634, 20.77539549240),
            new vsop_term_t(0.00000302375, 4.48618007156, 3532.06069281140),
            new vsop_term_t(0.00000274027, 0.54222167059, 3340.54511639700),
            new vsop_term_t(0.00000281079, 5.88163521788, 1349.86740965880),
            new vsop_term_t(0.00000231183, 1.28242156993, 3870.30339179440),
            new vsop_term_t(0.00000283602, 5.76885434940, 3149.16416058820),
            new vsop_term_t(0.00000236117, 5.75503217933, 3333.49887969900),
            new vsop_term_t(0.00000274033, 0.13372524985, 3340.67973700260),
            new vsop_term_t(0.00000299395, 2.78323740866, 6254.62666252360)
        };

        private static readonly vsop_term_t[] vsop_lat_Mars_1 = new vsop_term_t[]
        {
            new vsop_term_t(3340.61242700512, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01457554523, 3.60433733236, 3340.61242669980),
            new vsop_term_t(0.00168414711, 3.92318567804, 6681.22485339960),
            new vsop_term_t(0.00020622975, 4.26108844583, 10021.83728009940),
            new vsop_term_t(0.00003452392, 4.73210393190, 3.52311834900),
            new vsop_term_t(0.00002586332, 4.60670058555, 13362.44970679920),
            new vsop_term_t(0.00000841535, 4.45864030426, 2281.23049651060)
        };

        private static readonly vsop_term_t[] vsop_lat_Mars_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00058152577, 2.04961712429, 3340.61242669980),
            new vsop_term_t(0.00013459579, 2.45738706163, 6681.22485339960)
        };

        private static readonly vsop_series_t[] vsop_lat_Mars = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Mars_0),
            new vsop_series_t(vsop_lat_Mars_1),
            new vsop_series_t(vsop_lat_Mars_2)
        };

        private static readonly vsop_term_t[] vsop_lon_Mars_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.03197134986, 3.76832042431, 3340.61242669980),
            new vsop_term_t(0.00298033234, 4.10616996305, 6681.22485339960),
            new vsop_term_t(0.00289104742, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00031365539, 4.44651053090, 10021.83728009940),
            new vsop_term_t(0.00003484100, 4.78812549260, 13362.44970679920)
        };

        private static readonly vsop_term_t[] vsop_lon_Mars_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00217310991, 6.04472194776, 3340.61242669980),
            new vsop_term_t(0.00020976948, 3.14159265359, 0.00000000000),
            new vsop_term_t(0.00012834709, 1.60810667915, 6681.22485339960)
        };

        private static readonly vsop_series_t[] vsop_lon_Mars = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Mars_0),
            new vsop_series_t(vsop_lon_Mars_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Mars_0 = new vsop_term_t[]
        {
            new vsop_term_t(1.53033488271, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.14184953160, 3.47971283528, 3340.61242669980),
            new vsop_term_t(0.00660776362, 3.81783443019, 6681.22485339960),
            new vsop_term_t(0.00046179117, 4.15595316782, 10021.83728009940),
            new vsop_term_t(0.00008109733, 5.55958416318, 2810.92146160520),
            new vsop_term_t(0.00007485318, 1.77239078402, 5621.84292321040),
            new vsop_term_t(0.00005523191, 1.36436303770, 2281.23049651060),
            new vsop_term_t(0.00003825160, 4.49407183687, 13362.44970679920),
            new vsop_term_t(0.00002306537, 0.09081579001, 2544.31441988340),
            new vsop_term_t(0.00001999396, 5.36059617709, 3337.08930835080),
            new vsop_term_t(0.00002484394, 4.92545639920, 2942.46342329160),
            new vsop_term_t(0.00001960195, 4.74249437639, 3344.13554504880),
            new vsop_term_t(0.00001167119, 2.11260868341, 5092.15195811580),
            new vsop_term_t(0.00001102816, 5.00908403998, 398.14900340820),
            new vsop_term_t(0.00000899066, 4.40791133207, 529.69096509460),
            new vsop_term_t(0.00000992252, 5.83861961952, 6151.53388830500),
            new vsop_term_t(0.00000807354, 2.10217065501, 1059.38193018920),
            new vsop_term_t(0.00000797915, 3.44839203899, 796.29800681640),
            new vsop_term_t(0.00000740975, 1.49906336885, 2146.16541647520)
        };

        private static readonly vsop_term_t[] vsop_rad_Mars_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.01107433345, 2.03250524857, 3340.61242669980),
            new vsop_term_t(0.00103175887, 2.37071847807, 6681.22485339960),
            new vsop_term_t(0.00012877200, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00010815880, 2.70888095665, 10021.83728009940)
        };

        private static readonly vsop_term_t[] vsop_rad_Mars_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00044242249, 0.47930604954, 3340.61242669980),
            new vsop_term_t(0.00008138042, 0.86998389204, 6681.22485339960)
        };

        private static readonly vsop_series_t[] vsop_rad_Mars = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Mars_0),
            new vsop_series_t(vsop_rad_Mars_1),
            new vsop_series_t(vsop_rad_Mars_2)
        };


        private static readonly vsop_term_t[] vsop_lat_Jupiter_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.59954691494, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.09695898719, 5.06191793158, 529.69096509460),
            new vsop_term_t(0.00573610142, 1.44406205629, 7.11354700080),
            new vsop_term_t(0.00306389205, 5.41734730184, 1059.38193018920),
            new vsop_term_t(0.00097178296, 4.14264726552, 632.78373931320),
            new vsop_term_t(0.00072903078, 3.64042916389, 522.57741809380),
            new vsop_term_t(0.00064263975, 3.41145165351, 103.09277421860),
            new vsop_term_t(0.00039806064, 2.29376740788, 419.48464387520),
            new vsop_term_t(0.00038857767, 1.27231755835, 316.39186965660),
            new vsop_term_t(0.00027964629, 1.78454591820, 536.80451209540),
            new vsop_term_t(0.00013589730, 5.77481040790, 1589.07289528380),
            new vsop_term_t(0.00008246349, 3.58227925840, 206.18554843720),
            new vsop_term_t(0.00008768704, 3.63000308199, 949.17560896980),
            new vsop_term_t(0.00007368042, 5.08101194270, 735.87651353180),
            new vsop_term_t(0.00006263150, 0.02497628807, 213.29909543800),
            new vsop_term_t(0.00006114062, 4.51319998626, 1162.47470440780),
            new vsop_term_t(0.00004905396, 1.32084470588, 110.20632121940),
            new vsop_term_t(0.00005305285, 1.30671216791, 14.22709400160),
            new vsop_term_t(0.00005305441, 4.18625634012, 1052.26838318840),
            new vsop_term_t(0.00004647248, 4.69958103684, 3.93215326310),
            new vsop_term_t(0.00003045023, 4.31676431084, 426.59819087600),
            new vsop_term_t(0.00002609999, 1.56667394063, 846.08283475120),
            new vsop_term_t(0.00002028191, 1.06376530715, 3.18139373770),
            new vsop_term_t(0.00001764763, 2.14148655117, 1066.49547719000),
            new vsop_term_t(0.00001722972, 3.88036268267, 1265.56747862640),
            new vsop_term_t(0.00001920945, 0.97168196472, 639.89728631400),
            new vsop_term_t(0.00001633223, 3.58201833555, 515.46387109300),
            new vsop_term_t(0.00001431999, 4.29685556046, 625.67019231240),
            new vsop_term_t(0.00000973272, 4.09764549134, 95.97922721780)
        };

        private static readonly vsop_term_t[] vsop_lat_Jupiter_1 = new vsop_term_t[]
        {
            new vsop_term_t(529.69096508814, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00489503243, 4.22082939470, 529.69096509460),
            new vsop_term_t(0.00228917222, 6.02646855621, 7.11354700080),
            new vsop_term_t(0.00030099479, 4.54540782858, 1059.38193018920),
            new vsop_term_t(0.00020720920, 5.45943156902, 522.57741809380),
            new vsop_term_t(0.00012103653, 0.16994816098, 536.80451209540),
            new vsop_term_t(0.00006067987, 4.42422292017, 103.09277421860),
            new vsop_term_t(0.00005433968, 3.98480737746, 419.48464387520),
            new vsop_term_t(0.00004237744, 5.89008707199, 14.22709400160)
        };

        private static readonly vsop_term_t[] vsop_lat_Jupiter_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00047233601, 4.32148536482, 7.11354700080),
            new vsop_term_t(0.00030649436, 2.92977788700, 529.69096509460),
            new vsop_term_t(0.00014837605, 3.14159265359, 0.00000000000)
        };

        private static readonly vsop_series_t[] vsop_lat_Jupiter = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Jupiter_0),
            new vsop_series_t(vsop_lat_Jupiter_1),
            new vsop_series_t(vsop_lat_Jupiter_2)
        };

        private static readonly vsop_term_t[] vsop_lon_Jupiter_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.02268615702, 3.55852606721, 529.69096509460),
            new vsop_term_t(0.00109971634, 3.90809347197, 1059.38193018920),
            new vsop_term_t(0.00110090358, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00008101428, 3.60509572885, 522.57741809380),
            new vsop_term_t(0.00006043996, 4.25883108339, 1589.07289528380),
            new vsop_term_t(0.00006437782, 0.30627119215, 536.80451209540)
        };

        private static readonly vsop_term_t[] vsop_lon_Jupiter_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00078203446, 1.52377859742, 529.69096509460)
        };

        private static readonly vsop_series_t[] vsop_lon_Jupiter = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Jupiter_0),
            new vsop_series_t(vsop_lon_Jupiter_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Jupiter_0 = new vsop_term_t[]
        {
            new vsop_term_t(5.20887429326, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.25209327119, 3.49108639871, 529.69096509460),
            new vsop_term_t(0.00610599976, 3.84115365948, 1059.38193018920),
            new vsop_term_t(0.00282029458, 2.57419881293, 632.78373931320),
            new vsop_term_t(0.00187647346, 2.07590383214, 522.57741809380),
            new vsop_term_t(0.00086792905, 0.71001145545, 419.48464387520),
            new vsop_term_t(0.00072062974, 0.21465724607, 536.80451209540),
            new vsop_term_t(0.00065517248, 5.97995884790, 316.39186965660),
            new vsop_term_t(0.00029134542, 1.67759379655, 103.09277421860),
            new vsop_term_t(0.00030135335, 2.16132003734, 949.17560896980),
            new vsop_term_t(0.00023453271, 3.54023522184, 735.87651353180),
            new vsop_term_t(0.00022283743, 4.19362594399, 1589.07289528380),
            new vsop_term_t(0.00023947298, 0.27458037480, 7.11354700080),
            new vsop_term_t(0.00013032614, 2.96042965363, 1162.47470440780),
            new vsop_term_t(0.00009703360, 1.90669633585, 206.18554843720),
            new vsop_term_t(0.00012749023, 2.71550286592, 1052.26838318840)
        };

        private static readonly vsop_term_t[] vsop_rad_Jupiter_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.01271801520, 2.64937512894, 529.69096509460),
            new vsop_term_t(0.00061661816, 3.00076460387, 1059.38193018920),
            new vsop_term_t(0.00053443713, 3.89717383175, 522.57741809380),
            new vsop_term_t(0.00031185171, 4.88276958012, 536.80451209540),
            new vsop_term_t(0.00041390269, 0.00000000000, 0.00000000000)
        };

        private static readonly vsop_series_t[] vsop_rad_Jupiter = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Jupiter_0),
            new vsop_series_t(vsop_rad_Jupiter_1)
        };


        private static readonly vsop_term_t[] vsop_lat_Saturn_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.87401354025, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.11107659762, 3.96205090159, 213.29909543800),
            new vsop_term_t(0.01414150957, 4.58581516874, 7.11354700080),
            new vsop_term_t(0.00398379389, 0.52112032699, 206.18554843720),
            new vsop_term_t(0.00350769243, 3.30329907896, 426.59819087600),
            new vsop_term_t(0.00206816305, 0.24658372002, 103.09277421860),
            new vsop_term_t(0.00079271300, 3.84007056878, 220.41264243880),
            new vsop_term_t(0.00023990355, 4.66976924553, 110.20632121940),
            new vsop_term_t(0.00016573588, 0.43719228296, 419.48464387520),
            new vsop_term_t(0.00014906995, 5.76903183869, 316.39186965660),
            new vsop_term_t(0.00015820290, 0.93809155235, 632.78373931320),
            new vsop_term_t(0.00014609559, 1.56518472000, 3.93215326310),
            new vsop_term_t(0.00013160301, 4.44891291899, 14.22709400160),
            new vsop_term_t(0.00015053543, 2.71669915667, 639.89728631400),
            new vsop_term_t(0.00013005299, 5.98119023644, 11.04570026390),
            new vsop_term_t(0.00010725067, 3.12939523827, 202.25339517410),
            new vsop_term_t(0.00005863206, 0.23656938524, 529.69096509460),
            new vsop_term_t(0.00005227757, 4.20783365759, 3.18139373770),
            new vsop_term_t(0.00006126317, 1.76328667907, 277.03499374140),
            new vsop_term_t(0.00005019687, 3.17787728405, 433.71173787680),
            new vsop_term_t(0.00004592550, 0.61977744975, 199.07200143640),
            new vsop_term_t(0.00004005867, 2.24479718502, 63.73589830340),
            new vsop_term_t(0.00002953796, 0.98280366998, 95.97922721780),
            new vsop_term_t(0.00003873670, 3.22283226966, 138.51749687070),
            new vsop_term_t(0.00002461186, 2.03163875071, 735.87651353180),
            new vsop_term_t(0.00003269484, 0.77492638211, 949.17560896980),
            new vsop_term_t(0.00001758145, 3.26580109940, 522.57741809380),
            new vsop_term_t(0.00001640172, 5.50504453050, 846.08283475120),
            new vsop_term_t(0.00001391327, 4.02333150505, 323.50541665740),
            new vsop_term_t(0.00001580648, 4.37265307169, 309.27832265580),
            new vsop_term_t(0.00001123498, 2.83726798446, 415.55249061210),
            new vsop_term_t(0.00001017275, 3.71700135395, 227.52618943960),
            new vsop_term_t(0.00000848642, 3.19150170830, 209.36694217490)
        };

        private static readonly vsop_term_t[] vsop_lat_Saturn_1 = new vsop_term_t[]
        {
            new vsop_term_t(213.29909521690, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01297370862, 1.82834923978, 213.29909543800),
            new vsop_term_t(0.00564345393, 2.88499717272, 7.11354700080),
            new vsop_term_t(0.00093734369, 1.06311793502, 426.59819087600),
            new vsop_term_t(0.00107674962, 2.27769131009, 206.18554843720),
            new vsop_term_t(0.00040244455, 2.04108104671, 220.41264243880),
            new vsop_term_t(0.00019941774, 1.27954390470, 103.09277421860),
            new vsop_term_t(0.00010511678, 2.74880342130, 14.22709400160),
            new vsop_term_t(0.00006416106, 0.38238295041, 639.89728631400),
            new vsop_term_t(0.00004848994, 2.43037610229, 419.48464387520),
            new vsop_term_t(0.00004056892, 2.92133209468, 110.20632121940),
            new vsop_term_t(0.00003768635, 3.64965330780, 3.93215326310)
        };

        private static readonly vsop_term_t[] vsop_lat_Saturn_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00116441330, 1.17988132879, 7.11354700080),
            new vsop_term_t(0.00091841837, 0.07325195840, 213.29909543800),
            new vsop_term_t(0.00036661728, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00015274496, 4.06493179167, 206.18554843720)
        };

        private static readonly vsop_series_t[] vsop_lat_Saturn = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Saturn_0),
            new vsop_series_t(vsop_lat_Saturn_1),
            new vsop_series_t(vsop_lat_Saturn_2)
        };

        private static readonly vsop_term_t[] vsop_lon_Saturn_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.04330678039, 3.60284428399, 213.29909543800),
            new vsop_term_t(0.00240348302, 2.85238489373, 426.59819087600),
            new vsop_term_t(0.00084745939, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00030863357, 3.48441504555, 220.41264243880),
            new vsop_term_t(0.00034116062, 0.57297307557, 206.18554843720),
            new vsop_term_t(0.00014734070, 2.11846596715, 639.89728631400),
            new vsop_term_t(0.00009916667, 5.79003188904, 419.48464387520),
            new vsop_term_t(0.00006993564, 4.73604689720, 7.11354700080),
            new vsop_term_t(0.00004807588, 5.43305312061, 316.39186965660)
        };

        private static readonly vsop_term_t[] vsop_lon_Saturn_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00198927992, 4.93901017903, 213.29909543800),
            new vsop_term_t(0.00036947916, 3.14159265359, 0.00000000000),
            new vsop_term_t(0.00017966989, 0.51979431110, 426.59819087600)
        };

        private static readonly vsop_series_t[] vsop_lon_Saturn = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Saturn_0),
            new vsop_series_t(vsop_lon_Saturn_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Saturn_0 = new vsop_term_t[]
        {
            new vsop_term_t(9.55758135486, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.52921382865, 2.39226219573, 213.29909543800),
            new vsop_term_t(0.01873679867, 5.23549604660, 206.18554843720),
            new vsop_term_t(0.01464663929, 1.64763042902, 426.59819087600),
            new vsop_term_t(0.00821891141, 5.93520042303, 316.39186965660),
            new vsop_term_t(0.00547506923, 5.01532618980, 103.09277421860),
            new vsop_term_t(0.00371684650, 2.27114821115, 220.41264243880),
            new vsop_term_t(0.00361778765, 3.13904301847, 7.11354700080),
            new vsop_term_t(0.00140617506, 5.70406606781, 632.78373931320),
            new vsop_term_t(0.00108974848, 3.29313390175, 110.20632121940),
            new vsop_term_t(0.00069006962, 5.94099540992, 419.48464387520),
            new vsop_term_t(0.00061053367, 0.94037691801, 639.89728631400),
            new vsop_term_t(0.00048913294, 1.55733638681, 202.25339517410),
            new vsop_term_t(0.00034143772, 0.19519102597, 277.03499374140),
            new vsop_term_t(0.00032401773, 5.47084567016, 949.17560896980),
            new vsop_term_t(0.00020936596, 0.46349251129, 735.87651353180)
        };

        private static readonly vsop_term_t[] vsop_rad_Saturn_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.06182981340, 0.25843511480, 213.29909543800),
            new vsop_term_t(0.00506577242, 0.71114625261, 206.18554843720),
            new vsop_term_t(0.00341394029, 5.79635741658, 426.59819087600),
            new vsop_term_t(0.00188491195, 0.47215589652, 220.41264243880),
            new vsop_term_t(0.00186261486, 3.14159265359, 0.00000000000),
            new vsop_term_t(0.00143891146, 1.40744822888, 7.11354700080)
        };

        private static readonly vsop_term_t[] vsop_rad_Saturn_2 = new vsop_term_t[]
        {
            new vsop_term_t(0.00436902572, 4.78671677509, 213.29909543800)
        };

        private static readonly vsop_series_t[] vsop_rad_Saturn = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Saturn_0),
            new vsop_series_t(vsop_rad_Saturn_1),
            new vsop_series_t(vsop_rad_Saturn_2)
        };


        private static readonly vsop_term_t[] vsop_lat_Uranus_0 = new vsop_term_t[]
        {
            new vsop_term_t(5.48129294297, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.09260408234, 0.89106421507, 74.78159856730),
            new vsop_term_t(0.01504247898, 3.62719260920, 1.48447270830),
            new vsop_term_t(0.00365981674, 1.89962179044, 73.29712585900),
            new vsop_term_t(0.00272328168, 3.35823706307, 149.56319713460),
            new vsop_term_t(0.00070328461, 5.39254450063, 63.73589830340),
            new vsop_term_t(0.00068892678, 6.09292483287, 76.26607127560),
            new vsop_term_t(0.00061998615, 2.26952066061, 2.96894541660),
            new vsop_term_t(0.00061950719, 2.85098872691, 11.04570026390),
            new vsop_term_t(0.00026468770, 3.14152083966, 71.81265315070),
            new vsop_term_t(0.00025710476, 6.11379840493, 454.90936652730),
            new vsop_term_t(0.00021078850, 4.36059339067, 148.07872442630),
            new vsop_term_t(0.00017818647, 1.74436930289, 36.64856292950),
            new vsop_term_t(0.00014613507, 4.73732166022, 3.93215326310),
            new vsop_term_t(0.00011162509, 5.82681796350, 224.34479570190),
            new vsop_term_t(0.00010997910, 0.48865004018, 138.51749687070),
            new vsop_term_t(0.00009527478, 2.95516862826, 35.16409022120),
            new vsop_term_t(0.00007545601, 5.23626582400, 109.94568878850),
            new vsop_term_t(0.00004220241, 3.23328220918, 70.84944530420),
            new vsop_term_t(0.00004051900, 2.27755017300, 151.04766984290),
            new vsop_term_t(0.00003354596, 1.06549007380, 4.45341812490),
            new vsop_term_t(0.00002926718, 4.62903718891, 9.56122755560),
            new vsop_term_t(0.00003490340, 5.48306144511, 146.59425171800),
            new vsop_term_t(0.00003144069, 4.75199570434, 77.75054398390),
            new vsop_term_t(0.00002922333, 5.35235361027, 85.82729883120),
            new vsop_term_t(0.00002272788, 4.36600400036, 70.32818044240),
            new vsop_term_t(0.00002051219, 1.51773566586, 0.11187458460),
            new vsop_term_t(0.00002148602, 0.60745949945, 38.13303563780),
            new vsop_term_t(0.00001991643, 4.92437588682, 277.03499374140),
            new vsop_term_t(0.00001376226, 2.04283539351, 65.22037101170),
            new vsop_term_t(0.00001666902, 3.62744066769, 380.12776796000),
            new vsop_term_t(0.00001284107, 3.11347961505, 202.25339517410),
            new vsop_term_t(0.00001150429, 0.93343589092, 3.18139373770),
            new vsop_term_t(0.00001533221, 2.58594681212, 52.69019803950),
            new vsop_term_t(0.00001281604, 0.54271272721, 222.86032299360),
            new vsop_term_t(0.00001372139, 4.19641530878, 111.43016149680),
            new vsop_term_t(0.00001221029, 0.19900650030, 108.46121608020),
            new vsop_term_t(0.00000946181, 1.19253165736, 127.47179660680),
            new vsop_term_t(0.00001150989, 4.17898916639, 33.67961751290)
        };

        private static readonly vsop_term_t[] vsop_lat_Uranus_1 = new vsop_term_t[]
        {
            new vsop_term_t(74.78159860910, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00154332863, 5.24158770553, 74.78159856730),
            new vsop_term_t(0.00024456474, 1.71260334156, 1.48447270830),
            new vsop_term_t(0.00009258442, 0.42829732350, 11.04570026390),
            new vsop_term_t(0.00008265977, 1.50218091379, 63.73589830340),
            new vsop_term_t(0.00009150160, 1.41213765216, 149.56319713460)
        };

        private static readonly vsop_series_t[] vsop_lat_Uranus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Uranus_0),
            new vsop_series_t(vsop_lat_Uranus_1)
        };

        private static readonly vsop_term_t[] vsop_lon_Uranus_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.01346277648, 2.61877810547, 74.78159856730),
            new vsop_term_t(0.00062341400, 5.08111189648, 149.56319713460),
            new vsop_term_t(0.00061601196, 3.14159265359, 0.00000000000),
            new vsop_term_t(0.00009963722, 1.61603805646, 76.26607127560),
            new vsop_term_t(0.00009926160, 0.57630380333, 73.29712585900)
        };

        private static readonly vsop_term_t[] vsop_lon_Uranus_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.00034101978, 0.01321929936, 74.78159856730)
        };

        private static readonly vsop_series_t[] vsop_lon_Uranus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Uranus_0),
            new vsop_series_t(vsop_lon_Uranus_1)
        };

        private static readonly vsop_term_t[] vsop_rad_Uranus_0 = new vsop_term_t[]
        {
            new vsop_term_t(19.21264847206, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.88784984413, 5.60377527014, 74.78159856730),
            new vsop_term_t(0.03440836062, 0.32836099706, 73.29712585900),
            new vsop_term_t(0.02055653860, 1.78295159330, 149.56319713460),
            new vsop_term_t(0.00649322410, 4.52247285911, 76.26607127560),
            new vsop_term_t(0.00602247865, 3.86003823674, 63.73589830340),
            new vsop_term_t(0.00496404167, 1.40139935333, 454.90936652730),
            new vsop_term_t(0.00338525369, 1.58002770318, 138.51749687070),
            new vsop_term_t(0.00243509114, 1.57086606044, 71.81265315070),
            new vsop_term_t(0.00190522303, 1.99809394714, 1.48447270830),
            new vsop_term_t(0.00161858838, 2.79137786799, 148.07872442630),
            new vsop_term_t(0.00143706183, 1.38368544947, 11.04570026390),
            new vsop_term_t(0.00093192405, 0.17437220467, 36.64856292950),
            new vsop_term_t(0.00071424548, 4.24509236074, 224.34479570190),
            new vsop_term_t(0.00089806014, 3.66105364565, 109.94568878850),
            new vsop_term_t(0.00039009723, 1.66971401684, 70.84944530420),
            new vsop_term_t(0.00046677296, 1.39976401694, 35.16409022120),
            new vsop_term_t(0.00039025624, 3.36234773834, 277.03499374140),
            new vsop_term_t(0.00036755274, 3.88649278513, 146.59425171800),
            new vsop_term_t(0.00030348723, 0.70100838798, 151.04766984290),
            new vsop_term_t(0.00029156413, 3.18056336700, 77.75054398390)
        };

        private static readonly vsop_term_t[] vsop_rad_Uranus_1 = new vsop_term_t[]
        {
            new vsop_term_t(0.01479896629, 3.67205697578, 74.78159856730)
        };

        private static readonly vsop_series_t[] vsop_rad_Uranus = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Uranus_0),
            new vsop_series_t(vsop_rad_Uranus_1)
        };


        private static readonly vsop_term_t[] vsop_lat_Neptune_0 = new vsop_term_t[]
        {
            new vsop_term_t(5.31188633046, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.01798475530, 2.90101273890, 38.13303563780),
            new vsop_term_t(0.01019727652, 0.48580922867, 1.48447270830),
            new vsop_term_t(0.00124531845, 4.83008090676, 36.64856292950),
            new vsop_term_t(0.00042064466, 5.41054993053, 2.96894541660),
            new vsop_term_t(0.00037714584, 6.09221808686, 35.16409022120),
            new vsop_term_t(0.00033784738, 1.24488874087, 76.26607127560),
            new vsop_term_t(0.00016482741, 0.00007727998, 491.55792945680),
            new vsop_term_t(0.00009198584, 4.93747051954, 39.61750834610),
            new vsop_term_t(0.00008994250, 0.27462171806, 175.16605980020)
        };

        private static readonly vsop_term_t[] vsop_lat_Neptune_1 = new vsop_term_t[]
        {
            new vsop_term_t(38.13303563957, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00016604172, 4.86323329249, 1.48447270830),
            new vsop_term_t(0.00015744045, 2.27887427527, 38.13303563780)
        };

        private static readonly vsop_series_t[] vsop_lat_Neptune = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lat_Neptune_0),
            new vsop_series_t(vsop_lat_Neptune_1)
        };

        private static readonly vsop_term_t[] vsop_lon_Neptune_0 = new vsop_term_t[]
        {
            new vsop_term_t(0.03088622933, 1.44104372644, 38.13303563780),
            new vsop_term_t(0.00027780087, 5.91271884599, 76.26607127560),
            new vsop_term_t(0.00027623609, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.00015355489, 2.52123799551, 36.64856292950),
            new vsop_term_t(0.00015448133, 3.50877079215, 39.61750834610)
        };

        private static readonly vsop_series_t[] vsop_lon_Neptune = new vsop_series_t[]
        {
            new vsop_series_t(vsop_lon_Neptune_0)
        };

        private static readonly vsop_term_t[] vsop_rad_Neptune_0 = new vsop_term_t[]
        {
            new vsop_term_t(30.07013205828, 0.00000000000, 0.00000000000),
            new vsop_term_t(0.27062259632, 1.32999459377, 38.13303563780),
            new vsop_term_t(0.01691764014, 3.25186135653, 36.64856292950),
            new vsop_term_t(0.00807830553, 5.18592878704, 1.48447270830),
            new vsop_term_t(0.00537760510, 4.52113935896, 35.16409022120),
            new vsop_term_t(0.00495725141, 1.57105641650, 491.55792945680),
            new vsop_term_t(0.00274571975, 1.84552258866, 175.16605980020)
        };

        private static readonly vsop_series_t[] vsop_rad_Neptune = new vsop_series_t[]
        {
            new vsop_series_t(vsop_rad_Neptune_0)
        };



        private static readonly vsop_model_t[] vsop = new vsop_model_t[]
        {
            new vsop_model_t(vsop_lat_Mercury,  vsop_lon_Mercury,   vsop_rad_Mercury),
            new vsop_model_t(vsop_lat_Venus,    vsop_lon_Venus,     vsop_rad_Venus  ),
            new vsop_model_t(vsop_lat_Earth,    vsop_lon_Earth,     vsop_rad_Earth  ),
            new vsop_model_t(vsop_lat_Mars,     vsop_lon_Mars,      vsop_rad_Mars   ),
            new vsop_model_t(vsop_lat_Jupiter,  vsop_lon_Jupiter,   vsop_rad_Jupiter),
            new vsop_model_t(vsop_lat_Saturn,   vsop_lon_Saturn,    vsop_rad_Saturn ),
            new vsop_model_t(vsop_lat_Uranus,   vsop_lon_Uranus,    vsop_rad_Uranus ),
            new vsop_model_t(vsop_lat_Neptune,  vsop_lon_Neptune,   vsop_rad_Neptune)
        };

        /// <summary>
        /// The minimum year value supported by Astronomy Engine.
        /// </summary>
        public const int MinYear = 1700;

        /// <summary>
        /// The maximum year value supported by Astronomy Engine.
        /// </summary>
        public const int MaxYear = 2200;

        private static double DeltaT(double mjd)
        {
            int lo, hi, c;
            double frac;

            if (mjd <= DT[0].mjd)
                return DT[0].dt;

            if (mjd >= DT[DT.Length-1].mjd)
                return DT[DT.Length-1].dt;

            // Do a binary search to find the pair of indexes this mjd lies between.

            lo = 0;
            hi = DT.Length-2;   // make sure there is always an array element after the one we are looking at.
            for(;;)
            {
                if (lo > hi)
                {
                    // This should never happen unless there is a bug in the binary search.
                    throw new Exception("Could not find delta-t value");
                }

                c = (lo + hi) / 2;
                if (mjd < DT[c].mjd)
                    hi = c-1;
                else if (mjd > DT[c+1].mjd)
                    lo = c+1;
                else
                {
                    frac = (mjd - DT[c].mjd) / (DT[c+1].mjd - DT[c].mjd);
                    return DT[c].dt + frac*(DT[c+1].dt - DT[c].dt);
                }
            }
        }

        internal static double TerrestrialTime(double ut)
        {
            return ut + DeltaT(ut + Y2000_IN_MJD)/86400.0;
        }

        private static double VsopFormulaCalc(vsop_formula_t formula, double t)
        {
            double coord = 0.0;
            double tpower = 1.0;
            for (int s=0; s < formula.series.Length; ++s)
            {
                double sum = 0.0;
                vsop_series_t series = formula.series[s];
                for (int i=0; i < series.term.Length; ++i)
                {
                    vsop_term_t term = series.term[i];
                    sum += term.amplitude * Math.Cos(term.phase + (t * term.frequency));
                }
                coord += tpower * sum;
                tpower *= t;
            }
            return coord;
        }

        private static AstroVector CalcVsop(vsop_model_t model, AstroTime time)
        {
            double t = time.tt / 365250;    /* millennia since 2000 */

            /* Calculate the VSOP "B" trigonometric series to obtain ecliptic spherical coordinates. */
            double sphere0 = VsopFormulaCalc(model.lat, t);
            double sphere1 = VsopFormulaCalc(model.lon, t);
            double sphere2 = VsopFormulaCalc(model.rad, t);

            /* Convert ecliptic spherical coordinates to ecliptic Cartesian coordinates. */
            double r_coslat = sphere2 * Math.Cos(sphere1);
            double eclip0 = r_coslat * Math.Cos(sphere0);
            double eclip1 = r_coslat * Math.Sin(sphere0);
            double eclip2 = sphere2 * Math.Sin(sphere1);

            /* Convert ecliptic Cartesian coordinates to equatorial Cartesian coordinates. */
            double x = eclip0 + 0.000000440360*eclip1 - 0.000000190919*eclip2;
            double y = -0.000000479966*eclip0 + 0.917482137087*eclip1 - 0.397776982902*eclip2;
            double z = 0.397776982902*eclip1 + 0.917482137087*eclip2;

            return new AstroVector(x, y, z, time);
        }

        private struct astro_cheb_coeff_t
        {
            public double[] data;

            public astro_cheb_coeff_t(double x, double y, double z)
            {
                this.data = new double[] { x, y, z };
            }
        }

        private struct astro_cheb_record_t
        {
            public double tt;
            public double ndays;
            public astro_cheb_coeff_t[] coeff;

            public astro_cheb_record_t(double tt, double ndays, astro_cheb_coeff_t[] coeff)
            {
                this.tt = tt;
                this.ndays = ndays;
                this.coeff = coeff;
            }
        }

        private static readonly astro_cheb_coeff_t[] cheb_8_0 =
        {
            new astro_cheb_coeff_t(-30.303124711144, -18.980368465705,   3.206649343866),
            new astro_cheb_coeff_t( 20.092745278347, -27.533908687219, -14.641121965990),
            new astro_cheb_coeff_t(  9.137264744925,   6.513103657467,  -0.720732357468),
            new astro_cheb_coeff_t( -1.201554708717,   2.149917852301,   1.032022293526),
            new astro_cheb_coeff_t( -0.566068170022,  -0.285737361191,   0.081379987808),
            new astro_cheb_coeff_t(  0.041678527795,  -0.143363105040,  -0.057534475984),
            new astro_cheb_coeff_t(  0.041087908142,   0.007911321580,  -0.010270655537),
            new astro_cheb_coeff_t(  0.001611769878,   0.011409821837,   0.003679980733),
            new astro_cheb_coeff_t( -0.002536458296,  -0.000145632543,   0.000949924030),
            new astro_cheb_coeff_t(  0.001167651969,  -0.000049912680,   0.000115867710),
            new astro_cheb_coeff_t( -0.000196953286,   0.000420406270,   0.000110147171),
            new astro_cheb_coeff_t(  0.001073825784,   0.000442658285,   0.000146985332),
            new astro_cheb_coeff_t( -0.000906160087,   0.001702360394,   0.000758987924),
            new astro_cheb_coeff_t( -0.001467464335,  -0.000622191266,  -0.000231866243),
            new astro_cheb_coeff_t( -0.000008986691,   0.000004086384,   0.000001442956),
            new astro_cheb_coeff_t( -0.001099078039,  -0.000544633529,  -0.000205534708),
            new astro_cheb_coeff_t(  0.001259974751,  -0.002178533187,  -0.000965315934),
            new astro_cheb_coeff_t(  0.001695288316,   0.000768480768,   0.000287916141),
            new astro_cheb_coeff_t( -0.001428026702,   0.002707551594,   0.001195955756)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_1 =
        {
            new astro_cheb_coeff_t( 67.049456204563,  -9.279626603192, -23.091941092128),
            new astro_cheb_coeff_t( 14.860676672314,  26.594121136143,   3.819668867047),
            new astro_cheb_coeff_t( -6.254409044120,   1.408757903538,   2.323726101433),
            new astro_cheb_coeff_t(  0.114416381092,  -0.942273228585,  -0.328566335886),
            new astro_cheb_coeff_t(  0.074973631246,   0.106749156044,   0.010806547171),
            new astro_cheb_coeff_t( -0.018627741964,  -0.009983491157,   0.002589955906),
            new astro_cheb_coeff_t(  0.006167206174,  -0.001042430439,  -0.001521881831),
            new astro_cheb_coeff_t( -0.000471293617,   0.002337935239,   0.001060879763),
            new astro_cheb_coeff_t( -0.000240627462,  -0.001380351742,  -0.000546042590),
            new astro_cheb_coeff_t(  0.001872140444,   0.000679876620,   0.000240384842),
            new astro_cheb_coeff_t( -0.000334705177,   0.000693528330,   0.000301138309),
            new astro_cheb_coeff_t(  0.000796124758,   0.000653183163,   0.000259527079),
            new astro_cheb_coeff_t( -0.001276116664,   0.001393959948,   0.000629574865),
            new astro_cheb_coeff_t( -0.001235158458,  -0.000889985319,  -0.000351392687),
            new astro_cheb_coeff_t( -0.000019881944,   0.000048339979,   0.000021342186),
            new astro_cheb_coeff_t( -0.000987113745,  -0.000748420747,  -0.000296503569),
            new astro_cheb_coeff_t(  0.001721891782,  -0.001893675502,  -0.000854270937),
            new astro_cheb_coeff_t(  0.001505145187,   0.001081653337,   0.000426723640),
            new astro_cheb_coeff_t( -0.002019479384,   0.002375617497,   0.001068258925)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_2 =
        {
            new astro_cheb_coeff_t( 46.038290912405,  73.773759757856,   9.148670950706),
            new astro_cheb_coeff_t(-22.354364534703,  10.217143138926,   9.921247676076),
            new astro_cheb_coeff_t( -2.696282001399,  -4.440843715929,  -0.572373037840),
            new astro_cheb_coeff_t(  0.385475818800,  -0.287872688575,  -0.205914693555),
            new astro_cheb_coeff_t(  0.020994433095,   0.004256602589,  -0.004817361041),
            new astro_cheb_coeff_t(  0.003212255378,   0.000574875698,  -0.000764464370),
            new astro_cheb_coeff_t( -0.000158619286,  -0.001035559544,  -0.000535612316),
            new astro_cheb_coeff_t(  0.000967952107,  -0.000653111849,  -0.000292019750),
            new astro_cheb_coeff_t(  0.001763494906,  -0.000370815938,  -0.000224698363),
            new astro_cheb_coeff_t(  0.001157990330,   0.001849810828,   0.000759641577),
            new astro_cheb_coeff_t( -0.000883535516,   0.000384038162,   0.000191242192),
            new astro_cheb_coeff_t(  0.000709486562,   0.000655810827,   0.000265431131),
            new astro_cheb_coeff_t( -0.001525810419,   0.001126870468,   0.000520202001),
            new astro_cheb_coeff_t( -0.000983210860,  -0.001116073455,  -0.000456026382),
            new astro_cheb_coeff_t( -0.000015655450,   0.000069184008,   0.000029796623),
            new astro_cheb_coeff_t( -0.000815102021,  -0.000900597010,  -0.000365274209),
            new astro_cheb_coeff_t(  0.002090300438,  -0.001536778673,  -0.000709827438),
            new astro_cheb_coeff_t(  0.001234661297,   0.001342978436,   0.000545313112),
            new astro_cheb_coeff_t( -0.002517963678,   0.001941826791,   0.000893859860)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_3 =
        {
            new astro_cheb_coeff_t(-39.074661990988,  30.963513412373,  21.431709298065),
            new astro_cheb_coeff_t(-12.033639281924, -31.693679132310,  -6.263961539568),
            new astro_cheb_coeff_t(  7.233936758611,  -3.979157072767,  -3.421027935569),
            new astro_cheb_coeff_t(  1.383182539917,   1.090729793400,  -0.076771771448),
            new astro_cheb_coeff_t( -0.009894394996,   0.313614402007,   0.101180677344),
            new astro_cheb_coeff_t( -0.055459383449,   0.031782406403,   0.026374448864),
            new astro_cheb_coeff_t( -0.011074105991,  -0.007176759494,   0.001896208351),
            new astro_cheb_coeff_t( -0.000263363398,  -0.001145329444,   0.000215471838),
            new astro_cheb_coeff_t(  0.000405700185,  -0.000839229891,  -0.000418571366),
            new astro_cheb_coeff_t(  0.001004921401,   0.001135118493,   0.000406734549),
            new astro_cheb_coeff_t( -0.000473938695,   0.000282751002,   0.000114911593),
            new astro_cheb_coeff_t(  0.000528685886,   0.000966635293,   0.000401955197),
            new astro_cheb_coeff_t( -0.001838869845,   0.000806432189,   0.000394594478),
            new astro_cheb_coeff_t( -0.000713122169,  -0.001334810971,  -0.000554511235),
            new astro_cheb_coeff_t(  0.000006449359,   0.000060730000,   0.000024513230),
            new astro_cheb_coeff_t( -0.000596025142,  -0.000999492770,  -0.000413930406),
            new astro_cheb_coeff_t(  0.002364904429,  -0.001099236865,  -0.000528480902),
            new astro_cheb_coeff_t(  0.000907458104,   0.001537243912,   0.000637001965),
            new astro_cheb_coeff_t( -0.002909908764,   0.001413648354,   0.000677030924)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_4 =
        {
            new astro_cheb_coeff_t( 23.380075041204, -38.969338804442, -19.204762094135),
            new astro_cheb_coeff_t( 33.437140696536,   8.735194448531,  -7.348352917314),
            new astro_cheb_coeff_t( -3.127251304544,   8.324311848708,   3.540122328502),
            new astro_cheb_coeff_t( -1.491354030154,  -1.350371407475,   0.028214278544),
            new astro_cheb_coeff_t(  0.361398480996,  -0.118420687058,  -0.145375605480),
            new astro_cheb_coeff_t( -0.011771350229,   0.085880588309,   0.030665997197),
            new astro_cheb_coeff_t( -0.015839541688,  -0.014165128211,   0.000523465951),
            new astro_cheb_coeff_t(  0.004213218926,  -0.001426373728,  -0.001906412496),
            new astro_cheb_coeff_t(  0.001465150002,   0.000451513538,   0.000081936194),
            new astro_cheb_coeff_t(  0.000640069511,   0.001886692235,   0.000884675556),
            new astro_cheb_coeff_t( -0.000883554940,   0.000301907356,   0.000127310183),
            new astro_cheb_coeff_t(  0.000245524038,   0.000910362686,   0.000385555148),
            new astro_cheb_coeff_t( -0.001942010476,   0.000438682280,   0.000237124027),
            new astro_cheb_coeff_t( -0.000425455660,  -0.001442138768,  -0.000607751390),
            new astro_cheb_coeff_t(  0.000004168433,   0.000033856562,   0.000013881811),
            new astro_cheb_coeff_t( -0.000337920193,  -0.001074290356,  -0.000452503056),
            new astro_cheb_coeff_t(  0.002544755354,  -0.000620356219,  -0.000327246228),
            new astro_cheb_coeff_t(  0.000534534110,   0.001670320887,   0.000702775941),
            new astro_cheb_coeff_t( -0.003169380270,   0.000816186705,   0.000427213817)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_5 =
        {
            new astro_cheb_coeff_t( 74.130449310804,  43.372111541004,  -8.799489207171),
            new astro_cheb_coeff_t( -8.705941488523,  23.344631690845,   9.908006472122),
            new astro_cheb_coeff_t( -4.614752911564,  -2.587334376729,   0.583321715294),
            new astro_cheb_coeff_t(  0.316219286624,  -0.395448970181,  -0.219217574801),
            new astro_cheb_coeff_t(  0.004593734664,   0.027528474371,   0.007736197280),
            new astro_cheb_coeff_t( -0.001192268851,  -0.004987723997,  -0.001599399192),
            new astro_cheb_coeff_t(  0.003051998429,  -0.001287028653,  -0.000780744058),
            new astro_cheb_coeff_t(  0.001482572043,   0.001613554244,   0.000635747068),
            new astro_cheb_coeff_t(  0.000581965277,   0.000788286674,   0.000315285159),
            new astro_cheb_coeff_t( -0.000311830730,   0.001622369930,   0.000714817617),
            new astro_cheb_coeff_t( -0.000711275723,  -0.000160014561,  -0.000050445901),
            new astro_cheb_coeff_t(  0.000177159088,   0.001032713853,   0.000435835541),
            new astro_cheb_coeff_t( -0.002032280820,   0.000144281331,   0.000111910344),
            new astro_cheb_coeff_t( -0.000148463759,  -0.001495212309,  -0.000635892081),
            new astro_cheb_coeff_t( -0.000009629403,  -0.000013678407,  -0.000006187457),
            new astro_cheb_coeff_t( -0.000061196084,  -0.001119783520,  -0.000479221572),
            new astro_cheb_coeff_t(  0.002630993795,  -0.000113042927,  -0.000112115452),
            new astro_cheb_coeff_t(  0.000132867113,   0.001741417484,   0.000743224630),
            new astro_cheb_coeff_t( -0.003293498893,   0.000182437998,   0.000158073228)
        };

        private static readonly astro_cheb_coeff_t[] cheb_8_6 =
        {
            new astro_cheb_coeff_t( -5.727994625506,  71.194823351703,  23.946198176031),
            new astro_cheb_coeff_t(-26.767323214686, -12.264949302780,   4.238297122007),
            new astro_cheb_coeff_t(  0.890596204250,  -5.970227904551,  -2.131444078785),
            new astro_cheb_coeff_t(  0.808383708156,  -0.143104108476,  -0.288102517987),
            new astro_cheb_coeff_t(  0.089303327519,   0.049290470655,  -0.010970501667),
            new astro_cheb_coeff_t(  0.010197195705,   0.012879721400,   0.001317586740),
            new astro_cheb_coeff_t(  0.001795282629,   0.004482403780,   0.001563326157),
            new astro_cheb_coeff_t( -0.001974716105,   0.001278073933,   0.000652735133),
            new astro_cheb_coeff_t(  0.000906544715,  -0.000805502229,  -0.000336200833),
            new astro_cheb_coeff_t(  0.000283816745,   0.001799099064,   0.000756827653),
            new astro_cheb_coeff_t( -0.000784971304,   0.000123081220,   0.000068812133),
            new astro_cheb_coeff_t( -0.000237033406,   0.000980100466,   0.000427758498),
            new astro_cheb_coeff_t( -0.001976846386,  -0.000280421081,  -0.000072417045),
            new astro_cheb_coeff_t(  0.000195628511,  -0.001446079585,  -0.000624011074),
            new astro_cheb_coeff_t( -0.000044622337,  -0.000035865046,  -0.000013581236),
            new astro_cheb_coeff_t(  0.000204397832,  -0.001127474894,  -0.000488668673),
            new astro_cheb_coeff_t(  0.002625373003,   0.000389300123,   0.000102756139),
            new astro_cheb_coeff_t( -0.000277321614,   0.001732818354,   0.000749576471),
            new astro_cheb_coeff_t( -0.003280537764,  -0.000457571669,  -0.000116383655)
        };

        private static readonly astro_cheb_record_t[] cheb_8 =
        {
            new astro_cheb_record_t( -109573.5, 26141.0, cheb_8_0),
            new astro_cheb_record_t(  -83432.5, 26141.0, cheb_8_1),
            new astro_cheb_record_t(  -57291.5, 26141.0, cheb_8_2),
            new astro_cheb_record_t(  -31150.5, 26141.0, cheb_8_3),
            new astro_cheb_record_t(   -5009.5, 26141.0, cheb_8_4),
            new astro_cheb_record_t(   21131.5, 26141.0, cheb_8_5),
            new astro_cheb_record_t(   47272.5, 26141.0, cheb_8_6)
        };

        private static double ChebScale(double t_min, double t_max, double t)
        {
            return (2*t - (t_max + t_min)) / (t_max - t_min);
        }

        private static AstroVector CalcChebyshev(astro_cheb_record_t[] model, AstroTime time)
        {
            var pos = new double[3];
            double p0, p1, p2, sum;

            /* Search for a record that overlaps the given time value. */
            for (int i=0; i < model.Length; ++i)
            {
                double x = ChebScale(model[i].tt, model[i].tt + model[i].ndays, time.tt);
                if (-1.0 <= x && x <= +1.0)
                {
                    for (int d=0; d < 3; ++d)
                    {
                        p0 = 1.0;
                        sum = model[i].coeff[0].data[d];
                        p1 = x;
                        sum += model[i].coeff[1].data[d] * p1;
                        for (int k=2; k < model[i].coeff.Length; ++k)
                        {
                            p2 = (2.0 * x * p1) - p0;
                            sum += model[i].coeff[k].data[d] * p2;
                            p0 = p1;
                            p1 = p2;
                        }
                        pos[d] = sum - model[i].coeff[0].data[d] / 2.0;
                    }

                    /* We found the position of the body. */
                    return new AstroVector(pos[0], pos[1], pos[2], time);
                }
            }

            /* The Chebyshev model does not cover this time value. */
            throw new ArgumentException(string.Format("Time argument is out of bounds: {0}", time));
        }

        private static AstroVector precession(double tt1, AstroVector pos1, double tt2)
        {
            double xx, yx, zx, xy, yy, zy, xz, yz, zz;
            double t, psia, omegaa, chia, sa, ca, sb, cb, sc, cc, sd, cd;
            double eps0 = 84381.406;

            if ((tt1 != 0.0) && (tt2 != 0.0))
                throw new ArgumentException("precession: one of (tt1, tt2) must be zero.");

            t = (tt2 - tt1) / 36525;
            if (tt2 == 0)
                t = -t;

            psia   = (((((-    0.0000000951  * t
                        +    0.000132851 ) * t
                        -    0.00114045  ) * t
                        -    1.0790069   ) * t
                        + 5038.481507    ) * t);

            omegaa = (((((+    0.0000003337  * t
                        -    0.000000467 ) * t
                        -    0.00772503  ) * t
                        +    0.0512623   ) * t
                        -    0.025754    ) * t + eps0);

            chia   = (((((-    0.0000000560  * t
                        +    0.000170663 ) * t
                        -    0.00121197  ) * t
                        -    2.3814292   ) * t
                        +   10.556403    ) * t);

            eps0 = eps0 * ASEC2RAD;
            psia = psia * ASEC2RAD;
            omegaa = omegaa * ASEC2RAD;
            chia = chia * ASEC2RAD;

            sa = Math.Sin(eps0);
            ca = Math.Cos(eps0);
            sb = Math.Sin(-psia);
            cb = Math.Cos(-psia);
            sc = Math.Sin(-omegaa);
            cc = Math.Cos(-omegaa);
            sd = Math.Sin(chia);
            cd = Math.Cos(chia);

            xx =  cd * cb - sb * sd * cc;
            yx =  cd * sb * ca + sd * cc * cb * ca - sa * sd * sc;
            zx =  cd * sb * sa + sd * cc * cb * sa + ca * sd * sc;
            xy = -sd * cb - sb * cd * cc;
            yy = -sd * sb * ca + cd * cc * cb * ca - sa * cd * sc;
            zy = -sd * sb * sa + cd * cc * cb * sa + ca * cd * sc;
            xz =  sb * sc;
            yz = -sc * cb * ca - sa * cc;
            zz = -sc * cb * sa + cc * ca;

            double x, y, z;

            if (tt2 == 0.0)
            {
                /* Perform rotation from other epoch to J2000.0. */
                x = xx * pos1.x + xy * pos1.y + xz * pos1.z;
                y = yx * pos1.x + yy * pos1.y + yz * pos1.z;
                z = zx * pos1.x + zy * pos1.y + zz * pos1.z;
            }
            else
            {
                /* Perform rotation from J2000.0 to other epoch. */
                x = xx * pos1.x + yx * pos1.y + zx * pos1.z;
                y = xy * pos1.x + yy * pos1.y + zy * pos1.z;
                z = xz * pos1.x + yz * pos1.y + zz * pos1.z;
            }

            return new AstroVector(x, y, z, null);
        }

        private struct earth_tilt_t
        {
            public double tt;
            public double dpsi;
            public double deps;
            public double ee;
            public double mobl;
            public double tobl;

            public earth_tilt_t(double tt, double dpsi, double deps, double ee, double mobl, double tobl)
            {
                this.tt = tt;
                this.dpsi = dpsi;
                this.deps = deps;
                this.ee = ee;
                this.mobl = mobl;
                this.tobl = tobl;
            }
        }

        private struct iau_row_t
        {
            public int nals0;
            public int nals1;
            public int nals2;
            public int nals3;
            public int nals4;

            public double cls0;
            public double cls1;
            public double cls2;
            public double cls3;
            public double cls4;
            public double cls5;
        }

        private static readonly iau_row_t[] iau_row = new iau_row_t[]
        {

        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =   -172064161, cls1 =      -174666, cls2 =        33386, cls3 =     92052331, cls4 =         9086, cls5 =        15377 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =    -13170906, cls1 =        -1675, cls2 =       -13696, cls3 =      5730336, cls4 =        -3015, cls5 =        -4587 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =     -2276413, cls1 =         -234, cls2 =         2796, cls3 =       978459, cls4 =         -485, cls5 =         1374 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  2 , cls0 =      2074554, cls1 =          207, cls2 =         -698, cls3 =      -897492, cls4 =          470, cls5 =         -291 },
        new iau_row_t { nals0 =  0, nals1 =  1, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =      1475877, cls1 =        -3633, cls2 =        11817, cls3 =        73871, cls4 =         -184, cls5 =        -1924 },
        new iau_row_t { nals0 =  0, nals1 =  1, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =      -516821, cls1 =         1226, cls2 =         -524, cls3 =       224386, cls4 =         -677, cls5 =         -174 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =       711159, cls1 =           73, cls2 =         -872, cls3 =        -6750, cls4 =            0, cls5 =          358 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  1 , cls0 =      -387298, cls1 =         -367, cls2 =          380, cls3 =       200728, cls4 =           18, cls5 =          318 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =      -301461, cls1 =          -36, cls2 =          816, cls3 =       129025, cls4 =          -63, cls5 =          367 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =       215829, cls1 =         -494, cls2 =          111, cls3 =       -95929, cls4 =          299, cls5 =          132 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =       128227, cls1 =          137, cls2 =          181, cls3 =       -68982, cls4 =           -9, cls5 =           39 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =       123457, cls1 =           11, cls2 =           19, cls3 =       -53311, cls4 =           32, cls5 =           -4 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =       156994, cls1 =           10, cls2 =         -168, cls3 =        -1235, cls4 =            0, cls5 =           82 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =        63110, cls1 =           63, cls2 =           27, cls3 =       -33228, cls4 =            0, cls5 =           -9 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =       -57976, cls1 =          -63, cls2 =         -189, cls3 =        31429, cls4 =            0, cls5 =          -75 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =       -59641, cls1 =          -11, cls2 =          149, cls3 =        25543, cls4 =          -11, cls5 =           66 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  1 , cls0 =       -51613, cls1 =          -42, cls2 =          129, cls3 =        26366, cls4 =            0, cls5 =           78 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  1 , cls0 =        45893, cls1 =           50, cls2 =           31, cls3 =       -24236, cls4 =          -10, cls5 =           20 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =        63384, cls1 =           11, cls2 =         -150, cls3 =        -1220, cls4 =            0, cls5 =           29 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =       -38571, cls1 =           -1, cls2 =          158, cls3 =        16452, cls4 =          -11, cls5 =           68 },
        new iau_row_t { nals0 =  0, nals1 = -2, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =        32481, cls1 =            0, cls2 =            0, cls3 =       -13870, cls4 =            0, cls5 =            0 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =       -47722, cls1 =            0, cls2 =          -18, cls3 =          477, cls4 =            0, cls5 =          -25 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =       -31046, cls1 =           -1, cls2 =          131, cls3 =        13238, cls4 =          -11, cls5 =           59 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =        28593, cls1 =            0, cls2 =           -1, cls3 =       -12338, cls4 =           10, cls5 =           -3 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  1 , cls0 =        20441, cls1 =           21, cls2 =           10, cls3 =       -10758, cls4 =            0, cls5 =           -3 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =        29243, cls1 =            0, cls2 =          -74, cls3 =         -609, cls4 =            0, cls5 =           13 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  0 , cls0 =        25887, cls1 =            0, cls2 =          -66, cls3 =         -550, cls4 =            0, cls5 =           11 },
        new iau_row_t { nals0 =  0, nals1 =  1, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =       -14053, cls1 =          -25, cls2 =           79, cls3 =         8551, cls4 =           -2, cls5 =          -45 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  1 , cls0 =        15164, cls1 =           10, cls2 =           11, cls3 =        -8001, cls4 =            0, cls5 =           -1 },
        new iau_row_t { nals0 =  0, nals1 =  2, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =       -15794, cls1 =           72, cls2 =          -16, cls3 =         6850, cls4 =          -42, cls5 =           -5 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 = -2, nals3 =  2, nals4 =  0 , cls0 =        21783, cls1 =            0, cls2 =           13, cls3 =         -167, cls4 =            0, cls5 =           13 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  0, nals3 = -2, nals4 =  1 , cls0 =       -12873, cls1 =          -10, cls2 =          -37, cls3 =         6953, cls4 =            0, cls5 =          -14 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =       -12654, cls1 =           11, cls2 =           63, cls3 =         6415, cls4 =            0, cls5 =           26 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  1 , cls0 =       -10204, cls1 =            0, cls2 =           25, cls3 =         5222, cls4 =            0, cls5 =           15 },
        new iau_row_t { nals0 =  0, nals1 =  2, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =        16707, cls1 =          -85, cls2 =          -10, cls3 =          168, cls4 =           -1, cls5 =           10 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =        -7691, cls1 =            0, cls2 =           44, cls3 =         3268, cls4 =            0, cls5 =           19 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  0 , cls0 =       -11024, cls1 =            0, cls2 =          -14, cls3 =          104, cls4 =            0, cls5 =            2 },
        new iau_row_t { nals0 =  0, nals1 =  1, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =         7566, cls1 =          -21, cls2 =          -11, cls3 =        -3250, cls4 =            0, cls5 =           -5 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  1 , cls0 =        -6637, cls1 =          -11, cls2 =           25, cls3 =         3353, cls4 =            0, cls5 =           14 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =        -7141, cls1 =           21, cls2 =            8, cls3 =         3070, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  1 , cls0 =        -6302, cls1 =          -11, cls2 =            2, cls3 =         3272, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =         5800, cls1 =           10, cls2 =            2, cls3 =        -3045, cls4 =            0, cls5 =           -1 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =         6443, cls1 =            0, cls2 =           -7, cls3 =        -2768, cls4 =            0, cls5 =           -4 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  1 , cls0 =        -5774, cls1 =          -11, cls2 =          -15, cls3 =         3041, cls4 =            0, cls5 =           -5 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  1 , cls0 =        -5350, cls1 =            0, cls2 =           21, cls3 =         2695, cls4 =            0, cls5 =           12 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =        -4752, cls1 =          -11, cls2 =           -3, cls3 =         2719, cls4 =            0, cls5 =           -3 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 = -2, nals4 =  1 , cls0 =        -4940, cls1 =          -11, cls2 =          -21, cls3 =         2720, cls4 =            0, cls5 =           -9 },
        new iau_row_t { nals0 = -1, nals1 = -1, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =         7350, cls1 =            0, cls2 =           -8, cls3 =          -51, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  0, nals3 = -2, nals4 =  1 , cls0 =         4065, cls1 =            0, cls2 =            6, cls3 =        -2206, cls4 =            0, cls5 =            1 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =         6579, cls1 =            0, cls2 =          -24, cls3 =         -199, cls4 =            0, cls5 =            2 },
        new iau_row_t { nals0 =  0, nals1 =  1, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =         3579, cls1 =            0, cls2 =            5, cls3 =        -1900, cls4 =            0, cls5 =            1 },
        new iau_row_t { nals0 =  1, nals1 = -1, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =         4725, cls1 =            0, cls2 =           -6, cls3 =          -41, cls4 =            0, cls5 =            3 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =        -3075, cls1 =            0, cls2 =           -2, cls3 =         1313, cls4 =            0, cls5 =           -1 },
        new iau_row_t { nals0 =  3, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =        -2904, cls1 =            0, cls2 =           15, cls3 =         1233, cls4 =            0, cls5 =            7 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  0, nals3 =  2, nals4 =  0 , cls0 =         4348, cls1 =            0, cls2 =          -10, cls3 =          -81, cls4 =            0, cls5 =            2 },
        new iau_row_t { nals0 =  1, nals1 = -1, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =        -2878, cls1 =            0, cls2 =            8, cls3 =         1232, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  0, nals3 =  1, nals4 =  0 , cls0 =        -4230, cls1 =            0, cls2 =            5, cls3 =          -20, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 = -1, nals1 = -1, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =        -2819, cls1 =            0, cls2 =            7, cls3 =         1207, cls4 =            0, cls5 =            3 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  0 , cls0 =        -4056, cls1 =            0, cls2 =            5, cls3 =           40, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 =  0, nals1 = -1, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =        -2647, cls1 =            0, cls2 =           11, cls3 =         1129, cls4 =            0, cls5 =            5 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =        -2294, cls1 =            0, cls2 =          -10, cls3 =         1266, cls4 =            0, cls5 =           -4 },
        new iau_row_t { nals0 =  1, nals1 =  1, nals2 =  2, nals3 =  0, nals4 =  2 , cls0 =         2481, cls1 =            0, cls2 =           -7, cls3 =        -1062, cls4 =            0, cls5 =           -3 },
        new iau_row_t { nals0 =  2, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  1 , cls0 =         2179, cls1 =            0, cls2 =           -2, cls3 =        -1129, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 = -1, nals1 =  1, nals2 =  0, nals3 =  1, nals4 =  0 , cls0 =         3276, cls1 =            0, cls2 =            1, cls3 =           -9, cls4 =            0, cls5 =            0 },
        new iau_row_t { nals0 =  1, nals1 =  1, nals2 =  0, nals3 =  0, nals4 =  0 , cls0 =        -3389, cls1 =            0, cls2 =            5, cls3 =           35, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 =  0, nals4 =  0 , cls0 =         3339, cls1 =            0, cls2 =          -13, cls3 =         -107, cls4 =            0, cls5 =            1 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =        -1987, cls1 =            0, cls2 =           -6, cls3 =         1073, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  2 , cls0 =        -1981, cls1 =            0, cls2 =            0, cls3 =          854, cls4 =            0, cls5 =            0 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  0, nals3 =  1, nals4 =  0 , cls0 =         4026, cls1 =            0, cls2 =         -353, cls3 =         -553, cls4 =            0, cls5 =         -139 },
        new iau_row_t { nals0 =  0, nals1 =  0, nals2 =  2, nals3 =  1, nals4 =  2 , cls0 =         1660, cls1 =            0, cls2 =           -5, cls3 =         -710, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  2, nals3 =  4, nals4 =  2 , cls0 =        -1521, cls1 =            0, cls2 =            9, cls3 =          647, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 = -1, nals1 =  1, nals2 =  0, nals3 =  1, nals4 =  1 , cls0 =         1314, cls1 =            0, cls2 =            0, cls3 =         -700, cls4 =            0, cls5 =            0 },
        new iau_row_t { nals0 =  0, nals1 = -2, nals2 =  2, nals3 = -2, nals4 =  1 , cls0 =        -1283, cls1 =            0, cls2 =            0, cls3 =          672, cls4 =            0, cls5 =            0 },
        new iau_row_t { nals0 =  1, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  1 , cls0 =        -1331, cls1 =            0, cls2 =            8, cls3 =          663, cls4 =            0, cls5 =            4 },
        new iau_row_t { nals0 = -2, nals1 =  0, nals2 =  2, nals3 =  2, nals4 =  2 , cls0 =         1383, cls1 =            0, cls2 =           -2, cls3 =         -594, cls4 =            0, cls5 =           -2 },
        new iau_row_t { nals0 = -1, nals1 =  0, nals2 =  0, nals3 =  0, nals4 =  2 , cls0 =         1405, cls1 =            0, cls2 =            4, cls3 =         -610, cls4 =            0, cls5 =            2 },
        new iau_row_t { nals0 =  1, nals1 =  1, nals2 =  2, nals3 = -2, nals4 =  2 , cls0 =         1290, cls1 =            0, cls2 =            0, cls3 =         -556, cls4 =            0, cls5 =            0 }

        };

        private static void iau2000b(AstroTime time)
        {
            /* Adapted from the NOVAS C 3.1 function of the same name. */

            double t, el, elp, f, d, om, arg, dp, de, sarg, carg;
            int i;

            if (double.IsNaN(time.psi))
            {
                t = time.tt / 36525.0;
                el  = ((485868.249036 + t * 1717915923.2178) % ASEC360) * ASEC2RAD;
                elp = ((1287104.79305 + t * 129596581.0481)  % ASEC360) * ASEC2RAD;
                f   = ((335779.526232 + t * 1739527262.8478) % ASEC360) * ASEC2RAD;
                d   = ((1072260.70369 + t * 1602961601.2090) % ASEC360) * ASEC2RAD;
                om  = ((450160.398036 - t * 6962890.5431)    % ASEC360) * ASEC2RAD;
                dp = 0;
                de = 0;
                for (i=76; i >= 0; --i)
                {
                    arg = (iau_row[i].nals0*el + iau_row[i].nals1*elp + iau_row[i].nals2*f + iau_row[i].nals3*d + iau_row[i].nals4*om) % PI2;
                    sarg = Math.Sin(arg);
                    carg = Math.Cos(arg);
                    dp += (iau_row[i].cls0 + iau_row[i].cls1*t) * sarg + iau_row[i].cls2*carg;
                    de += (iau_row[i].cls3 + iau_row[i].cls4*t) * carg + iau_row[i].cls5*sarg;
                }

                time.psi = -0.000135 + (dp * 1.0e-7);
                time.eps = +0.000388 + (de * 1.0e-7);
            }
        }

        private static double mean_obliq(double tt)
        {
            double t = tt / 36525.0;
            double asec =
                (((( -  0.0000000434   * t
                    -  0.000000576  ) * t
                    +  0.00200340   ) * t
                    -  0.0001831    ) * t
                    - 46.836769     ) * t + 84381.406;

            return asec / 3600.0;
        }

        private static earth_tilt_t e_tilt(AstroTime time)
        {
            iau2000b(time);

            double mobl = mean_obliq(time.tt);
            double tobl = mobl + (time.eps / 3600.0);
            double ee = time.psi * Math.Cos(mobl * DEG2RAD) / 15.0;
            return new earth_tilt_t(time.tt, time.psi, time.eps, ee, mobl, tobl);
        }

        private static double era(double ut)        /* Earth Rotation Angle */
        {
            double thet1 = 0.7790572732640 + 0.00273781191135448 * ut;
            double thet3 = ut % 1.0;
            double theta = 360.0 *((thet1 + thet3) % 1.0);
            if (theta < 0.0)
                theta += 360.0;

            return theta;
        }

        private static double sidereal_time(AstroTime time)
        {
            double t = time.tt / 36525.0;
            double eqeq = 15.0 * e_tilt(time).ee;    /* Replace with eqeq=0 to get GMST instead of GAST (if we ever need it) */
            double theta = era(time.ut);
            double st = (eqeq + 0.014506 +
                (((( -    0.0000000368   * t
                    -    0.000029956  ) * t
                    -    0.00000044   ) * t
                    +    1.3915817    ) * t
                    + 4612.156534     ) * t);

            double gst = ((st/3600.0 + theta) % 360.0) / 15.0;
            if (gst < 0.0)
                gst += 24.0;

            return gst;
        }

        private static AstroVector terra(Observer observer, double st)
        {
            double erad_km = ERAD / 1000.0;
            double df = 1.0 - 0.003352819697896;    /* flattening of the Earth */
            double df2 = df * df;
            double phi = observer.latitude * DEG2RAD;
            double sinphi = Math.Sin(phi);
            double cosphi = Math.Cos(phi);
            double c = 1.0 / Math.Sqrt(cosphi*cosphi + df2*sinphi*sinphi);
            double s = df2 * c;
            double ht_km = observer.height / 1000.0;
            double ach = erad_km*c + ht_km;
            double ash = erad_km*s + ht_km;
            double stlocl = (15.0*st + observer.longitude) * DEG2RAD;
            double sinst = Math.Sin(stlocl);
            double cosst = Math.Cos(stlocl);

            return new AstroVector(
                ach * cosphi * cosst / KM_PER_AU,
                ach * cosphi * sinst / KM_PER_AU,
                ash * sinphi / KM_PER_AU,
                null
            );
        }

        private static AstroVector nutation(AstroTime time, int direction, AstroVector inpos)
        {
            earth_tilt_t tilt = e_tilt(time);
            double oblm = tilt.mobl * DEG2RAD;
            double oblt = tilt.tobl * DEG2RAD;
            double psi = tilt.dpsi * ASEC2RAD;
            double cobm = Math.Cos(oblm);
            double sobm = Math.Sin(oblm);
            double cobt = Math.Cos(oblt);
            double sobt = Math.Sin(oblt);
            double cpsi = Math.Cos(psi);
            double spsi = Math.Sin(psi);

            double xx = cpsi;
            double yx = -spsi * cobm;
            double zx = -spsi * sobm;
            double xy = spsi * cobt;
            double yy = cpsi * cobm * cobt + sobm * sobt;
            double zy = cpsi * sobm * cobt - cobm * sobt;
            double xz = spsi * sobt;
            double yz = cpsi * cobm * sobt - sobm * cobt;
            double zz = cpsi * sobm * sobt + cobm * cobt;

            double x, y, z;

            if (direction == 0)
            {
                /* forward rotation */
                x = xx * inpos.x + yx * inpos.y + zx * inpos.z;
                y = xy * inpos.x + yy * inpos.y + zy * inpos.z;
                z = xz * inpos.x + yz * inpos.y + zz * inpos.z;
            }
            else
            {
                /* inverse rotation */
                x = xx * inpos.x + xy * inpos.y + xz * inpos.z;
                y = yx * inpos.x + yy * inpos.y + yz * inpos.z;
                z = zx * inpos.x + zy * inpos.y + zz * inpos.z;
            }

            return new AstroVector(x, y, z, time);
        }

        private static Equatorial vector2radec(AstroVector pos)
        {
            double ra, dec, dist;
            double xyproj;

            xyproj = pos.x*pos.x + pos.y*pos.y;
            dist = Math.Sqrt(xyproj + pos.z*pos.z);
            if (xyproj == 0.0)
            {
                if (pos.z == 0.0)
                {
                    /* Indeterminate coordinates; pos vector has zero length. */
                    throw new ArgumentException("Bad vector");
                }

                if (pos.z < 0)
                {
                    ra = 0.0;
                    dec = -90.0;
                }
                else
                {
                    ra = 0.0;
                    dec = +90.0;
                }
            }
            else
            {
                ra = Math.Atan2(pos.y, pos.x) / (DEG2RAD * 15.0);
                if (ra < 0)
                    ra += 24.0;

                dec = RAD2DEG * Math.Atan2(pos.z, Math.Sqrt(xyproj));
            }

            return new Equatorial(ra, dec, dist);
        }

        private static AstroVector geo_pos(AstroTime time, Observer observer)
        {
            double gast = sidereal_time(time);
            AstroVector pos1 = terra(observer, gast);
            AstroVector pos2 = nutation(time, -1, pos1);
            return precession(time.tt, pos2, 0.0);
        }

        private static AstroVector spin(double angle, AstroVector pos)
        {
            double angr = angle * DEG2RAD;
            double cosang = Math.Cos(angr);
            double sinang = Math.Sin(angr);
            return new AstroVector(
                +cosang*pos.x + sinang*pos.y,
                -sinang*pos.x + cosang*pos.y,
                pos.z,
                null
            );
        }

        private static AstroVector ecl2equ_vec(AstroTime time, AstroVector ecl)
        {
            double obl = mean_obliq(time.tt) * DEG2RAD;
            double cos_obl = Math.Cos(obl);
            double sin_obl = Math.Sin(obl);

            return new AstroVector(
                ecl.x,
                ecl.y*cos_obl - ecl.z*sin_obl,
                ecl.y*sin_obl + ecl.z*cos_obl,
                time
            );
        }

        private static AstroVector GeoMoon(AstroTime time)
        {
            var context = new MoonContext(time.tt / 36525.0);
            MoonResult moon = context.CalcMoon();

            /* Convert geocentric ecliptic spherical coordinates to Cartesian coordinates. */
            double dist_cos_lat = moon.distance_au * Math.Cos(moon.geo_eclip_lat);

            var gepos = new AstroVector(
                dist_cos_lat * Math.Cos(moon.geo_eclip_lon),
                dist_cos_lat * Math.Sin(moon.geo_eclip_lon),
                moon.distance_au * Math.Sin(moon.geo_eclip_lat),
                null
            );

            /* Convert ecliptic coordinates to equatorial coordinates, both in mean equinox of date. */
            AstroVector mpos1 = ecl2equ_vec(time, gepos);

            /* Convert from mean equinox of date to J2000. */
            AstroVector mpos2 = precession(time.tt, mpos1, 0);

            /* Patch in the correct time value into the returned vector. */
            return new AstroVector(mpos2.x, mpos2.y, mpos2.z, time);
        }

        /// <summary>
        /// Calculates heliocentric Cartesian coordinates of a body in the J2000 equatorial system.
        /// </summary>
        /// <remarks>
        /// This function calculates the position of the given celestial body as a vector,
        /// using the center of the Sun as the origin.  The result is expressed as a Cartesian
        /// vector in the J2000 equatorial system: the coordinates are based on the mean equator
        /// of the Earth at noon UTC on 1 January 2000.
        ///
        /// The position is not corrected for light travel time or aberration.
        /// This is different from the behavior of #GeoVector.
        ///
        /// If given an invalid value for `body`, or the body is `Body.Pluto` and the `time` is outside
        /// the year range 1700..2200, this function will throw an `ArgumentException`.
        /// </remarks>
        /// <param name="body">A body for which to calculate a heliocentric position: the Sun, Moon, or any of the planets.</param>
        /// <param name="time">The date and time for which to calculate the position.</param>
        /// <returns>A heliocentric position vector of the center of the given body.</returns>
        public static AstroVector HelioVector(Body body, AstroTime time)
        {
            switch (body)
            {
                case Body.Sun:
                    return new AstroVector(0.0, 0.0, 0.0, time);

                case Body.Mercury:
                case Body.Venus:
                case Body.Earth:
                case Body.Mars:
                case Body.Jupiter:
                case Body.Saturn:
                case Body.Uranus:
                case Body.Neptune:
                    return CalcVsop(vsop[(int)body], time);

                case Body.Pluto:
                    return CalcChebyshev(cheb_8, time);

                default:
                    throw new ArgumentException(string.Format("Invalid body: {0}", body));
            }
        }

        private static AstroVector CalcEarth(AstroTime time)
        {
            return CalcVsop(vsop[(int)Body.Earth], time);
        }

        ///
        /// <summary>
        /// Calculates geocentric Cartesian coordinates of a body in the J2000 equatorial system.
        /// </summary>
        /// <remarks>
        /// This function calculates the position of the given celestial body as a vector,
        /// using the center of the Earth as the origin.  The result is expressed as a Cartesian
        /// vector in the J2000 equatorial system: the coordinates are based on the mean equator
        /// of the Earth at noon UTC on 1 January 2000.
        ///
        /// If given an invalid value for `body`, or the body is `Body.Pluto` and the `time` is outside
        /// the year range 1700..2200, this function will throw an exception.
        ///
        /// Unlike #HelioVector, this function always corrects for light travel time.
        /// This means the position of the body is "back-dated" by the amount of time it takes
        /// light to travel from that body to an observer on the Earth.
        ///
        /// Also, the position can optionally be corrected for
        /// [aberration](https://en.wikipedia.org/wiki/Aberration_of_light), an effect
        /// causing the apparent direction of the body to be shifted due to transverse
        /// movement of the Earth with respect to the rays of light coming from that body.
        /// </remarks>
        /// <param name="body">A body for which to calculate a heliocentric position: the Sun, Moon, or any of the planets.</param>
        /// <param name="time">The date and time for which to calculate the position.</param>
        /// <param name="aberration">`Aberration.Corrected` to correct for aberration, or `Aberration.None` to leave uncorrected.</param>
        /// <returns>A geocentric position vector of the center of the given body.</returns>
        public static AstroVector GeoVector(
            Body body,
            AstroTime time,
            Aberration aberration)
        {
            AstroVector vector;
            AstroVector earth = new AstroVector(0.0, 0.0, 0.0, null);
            AstroTime ltime;
            AstroTime ltime2;
            double dt;
            int iter;

            if (aberration != Aberration.Corrected && aberration != Aberration.None)
                throw new ArgumentException(string.Format("Unsupported aberration option {0}", aberration));

            switch (body)
            {
            case Body.Earth:
                /* The Earth's geocentric coordinates are always (0,0,0). */
                return new AstroVector(0.0, 0.0, 0.0, time);

            case Body.Sun:
                /* The Sun's heliocentric coordinates are always (0,0,0). No need for light travel correction. */
                vector = CalcEarth(time);
                return new AstroVector(-vector.x, -vector.y, -vector.z, time);

            case Body.Moon:
                return GeoMoon(time);

            default:
                /* For all other bodies, apply light travel time correction. */

                if (aberration == Aberration.None)
                {
                    /* No aberration, so calculate Earth's position once, at the time of observation. */
                    earth = CalcEarth(time);
                }

                ltime = time;
                for (iter=0; iter < 10; ++iter)
                {
                    vector = HelioVector(body, ltime);
                    if (aberration == Aberration.Corrected)
                    {
                        /*
                            Include aberration, so make a good first-order approximation
                            by backdating the Earth's position also.
                            This is confusing, but it works for objects within the Solar System
                            because the distance the Earth moves in that small amount of light
                            travel time (a few minutes to a few hours) is well approximated
                            by a line segment that substends the angle seen from the remote
                            body viewing Earth. That angle is pretty close to the aberration
                            angle of the moving Earth viewing the remote body.
                            In other words, both of the following approximate the aberration angle:
                                (transverse distance Earth moves) / (distance to body)
                                (transverse speed of Earth) / (speed of light).
                        */
                        earth = CalcEarth(ltime);
                    }

                    /* Convert heliocentric vector to geocentric vector. */
                    vector = new AstroVector(vector.x - earth.x, vector.y - earth.y, vector.z - earth.z, time);
                    ltime2 = time.AddDays(-vector.Length() / C_AUDAY);
                    dt = Math.Abs(ltime2.tt - ltime.tt);
                    if (dt < 1.0e-9)
                        return vector;

                    ltime = ltime2;
                }
                throw new Exception("Light travel time correction did not converge");
            }
        }

        /// <summary>
        /// Calculates equatorial coordinates of a celestial body as seen by an observer on the Earth's surface.
        /// </summary>
        /// <remarks>
        /// Calculates topocentric equatorial coordinates in one of two different systems:
        /// J2000 or true-equator-of-date, depending on the value of the `equdate` parameter.
        /// Equatorial coordinates include right ascension, declination, and distance in astronomical units.
        ///
        /// This function corrects for light travel time: it adjusts the apparent location
        /// of the observed body based on how long it takes for light to travel from the body to the Earth.
        ///
        /// This function corrects for *topocentric parallax*, meaning that it adjusts for the
        /// angular shift depending on where the observer is located on the Earth. This is most
        /// significant for the Moon, because it is so close to the Earth. However, parallax corection
        /// has a small effect on the apparent positions of other bodies.
        ///
        /// Correction for aberration is optional, using the `aberration` parameter.
        /// </remarks>
        /// <param name="body">The celestial body to be observed. Not allowed to be `Body.Earth`.</param>
        /// <param name="time">The date and time at which the observation takes place.</param>
        /// <param name="observer">A location on or near the surface of the Earth.</param>
        /// <param name="equdate">Selects the date of the Earth's equator in which to express the equatorial coordinates.</param>
        /// <param name="aberration">Selects whether or not to correct for aberration.</param>
        public static Equatorial Equator(
            Body body,
            AstroTime time,
            Observer observer,
            EquatorEpoch equdate,
            Aberration aberration)
        {
            AstroVector gc_observer = geo_pos(time, observer);
            AstroVector gc = GeoVector(body, time, aberration);
            AstroVector j2000 = new AstroVector(gc.x - gc_observer.x, gc.y - gc_observer.y, gc.z - gc_observer.z, time);

            switch (equdate)
            {
                case EquatorEpoch.OfDate:
                    AstroVector temp = precession(0.0, j2000, time.tt);
                    AstroVector datevect = nutation(time, 0, temp);
                    return vector2radec(datevect);

                case EquatorEpoch.J2000:
                    return vector2radec(j2000);

                default:
                    throw new ArgumentException(string.Format("Unsupported equator epoch {0}", equdate));
            }
        }

        /// <summary>
        /// Calculates the apparent location of a body relative to the local horizon of an observer on the Earth.
        /// </summary>
        /// <remarks>
        /// Given a date and time, the geographic location of an observer on the Earth, and
        /// equatorial coordinates (right ascension and declination) of a celestial body,
        /// this function returns horizontal coordinates (azimuth and altitude angles) for the body
        /// relative to the horizon at the geographic location.
        ///
        /// The right ascension `ra` and declination `dec` passed in must be *equator of date*
        /// coordinates, based on the Earth's true equator at the date and time of the observation.
        /// Otherwise the resulting horizontal coordinates will be inaccurate.
        /// Equator of date coordinates can be obtained by calling #Equator, passing in
        /// `EquatorEpoch.OfDate` as its `equdate` parameter. It is also recommended to enable
        /// aberration correction by passing in `Aberration.Corrected` as the `aberration` parameter.
        ///
        /// This function optionally corrects for atmospheric refraction.
        /// For most uses, it is recommended to pass `Refraction.Normal` in the `refraction` parameter to
        /// correct for optical lensing of the Earth's atmosphere that causes objects
        /// to appear somewhat higher above the horizon than they actually are.
        /// However, callers may choose to avoid this correction by passing in `Refraction.None`.
        /// If refraction correction is enabled, the azimuth, altitude, right ascension, and declination
        /// in the #Topocentric structure returned by this function will all be corrected for refraction.
        /// If refraction is disabled, none of these four coordinates will be corrected; in that case,
        /// the right ascension and declination in the returned structure will be numerically identical
        /// to the respective `ra` and `dec` values passed in.
        /// </remarks>
        /// <param name="time">The date and time of the observation.</param>
        /// <param name="observer">The geographic location of the observer.</param>
        /// <param name="ra">The right ascension of the body in sidereal hours. See remarks above for more details.</param>
        /// <param name="dec">The declination of the body in degrees. See remarks above for more details.</param>
        /// <param name="refraction">
        /// Selects whether to correct for atmospheric refraction, and if so, which model to use.
        /// The recommended value for most uses is `Refraction.Normal`.
        /// See remarks above for more details.
        /// </param>
        /// <returns>
        /// The body's apparent horizontal coordinates and equatorial coordinates, both optionally corrected for refraction.
        /// </returns>
        public static Topocentric Horizon(
            AstroTime time,
            Observer observer,
            double ra,
            double dec,
            Refraction refraction)
        {
            double sinlat = Math.Sin(observer.latitude * DEG2RAD);
            double coslat = Math.Cos(observer.latitude * DEG2RAD);
            double sinlon = Math.Sin(observer.longitude * DEG2RAD);
            double coslon = Math.Cos(observer.longitude * DEG2RAD);
            double sindc = Math.Sin(dec * DEG2RAD);
            double cosdc = Math.Cos(dec * DEG2RAD);
            double sinra = Math.Sin(ra * 15 * DEG2RAD);
            double cosra = Math.Cos(ra * 15 * DEG2RAD);

            var uze = new AstroVector(coslat * coslon, coslat * sinlon, sinlat, null);
            var une = new AstroVector(-sinlat * coslon, -sinlat * sinlon, coslat, null);
            var uwe = new AstroVector(sinlon, -coslon, 0.0, null);

            double spin_angle = -15.0 * sidereal_time(time);
            AstroVector uz = spin(spin_angle, uze);
            AstroVector un = spin(spin_angle, une);
            AstroVector uw = spin(spin_angle, uwe);

            var p = new AstroVector(cosdc * cosra, cosdc * sinra, sindc, null);
            double pz = p.x*uz.x + p.y*uz.y + p.z*uz.z;
            double pn = p.x*un.x + p.y*un.y + p.z*un.z;
            double pw = p.x*uw.x + p.y*uw.y + p.z*uw.z;

            double proj = Math.Sqrt(pn*pn + pw*pw);
            double az = 0.0;
            if (proj > 0.0)
            {
                az = -Math.Atan2(pw, pn) * RAD2DEG;
                if (az < 0.0)
                    az += 360.0;
                else if (az >= 360.0)
                    az -= 360.0;
            }
            double zd = Math.Atan2(proj, pz) * RAD2DEG;
            double hor_ra = ra;
            double hor_dec = dec;

            if (refraction == Refraction.Normal || refraction == Refraction.JplHor)
            {
                double zd0 = zd;
                // http://extras.springer.com/1999/978-1-4471-0555-8/chap4/horizons/horizons.pdf
                // JPL Horizons says it uses refraction algorithm from
                // Meeus "Astronomical Algorithms", 1991, p. 101-102.
                // I found the following Go implementation:
                // https://github.com/soniakeys/meeus/blob/master/v3/refraction/refract.go
                // This is a translation from the function "Saemundsson" there.
                // I found experimentally that JPL Horizons clamps the angle to 1 degree below the horizon.
                // This is important because the 'refr' formula below goes crazy near hd = -5.11.
                double hd = 90.0 - zd;
                if (hd < -1.0)
                    hd = -1.0;

                double refr = (1.02 / Math.Tan((hd+10.3/(hd+5.11))*DEG2RAD)) / 60.0;

                if (refraction == Refraction.Normal && zd > 91.0)
                {
                    // In "normal" mode we gradually reduce refraction toward the nadir
                    // so that we never get an altitude angle less than -90 degrees.
                    // When horizon angle is -1 degrees, zd = 91, and the factor is exactly 1.
                    // As zd approaches 180 (the nadir), the fraction approaches 0 linearly.
                    refr *= (180.0 - zd) / 89.0;
                }

                zd -= refr;

                if (refr > 0.0 && zd > 3.0e-4)
                {
                    double sinzd = Math.Sin(zd * DEG2RAD);
                    double coszd = Math.Cos(zd * DEG2RAD);
                    double sinzd0 = Math.Sin(zd0 * DEG2RAD);
                    double coszd0 = Math.Cos(zd0 * DEG2RAD);

                    double prx = ((p.x - coszd0 * uz.x) / sinzd0)*sinzd + uz.x*coszd;
                    double pry = ((p.y - coszd0 * uz.y) / sinzd0)*sinzd + uz.y*coszd;
                    double prz = ((p.z - coszd0 * uz.z) / sinzd0)*sinzd + uz.z*coszd;

                    proj = Math.Sqrt(prx*prx + pry*pry);
                    if (proj > 0.0)
                    {
                        hor_ra = Math.Atan2(pry, prx) * (RAD2DEG / 15.0);
                        if (hor_ra < 0.0)
                            hor_ra += 24.0;
                        else if (hor_ra >= 24.0)
                            hor_ra -= 24.0;
                    }
                    else
                    {
                        hor_ra = 0.0;
                    }
                    hor_dec = Math.Atan2(prz, proj) * RAD2DEG;
                }
            }
            else if (refraction != Refraction.None)
                throw new ArgumentException(string.Format("Unsupported refraction option {0}", refraction));

            return new Topocentric(az, 90.0 - zd, hor_ra, hor_dec);
        }

        /// <summary>
        /// Calculates geocentric ecliptic coordinates for the Sun.
        /// </summary>
        /// <remarks>
        /// This function calculates the position of the Sun as seen from the Earth.
        /// The returned value includes both Cartesian and spherical coordinates.
        /// The x-coordinate and longitude values in the returned structure are based
        /// on the *true equinox of date*: one of two points in the sky where the instantaneous
        /// plane of the Earth's equator at the given date and time (the *equatorial plane*)
        /// intersects with the plane of the Earth's orbit around the Sun (the *ecliptic plane*).
        /// By convention, the apparent location of the Sun at the March equinox is chosen
        /// as the longitude origin and x-axis direction, instead of the one for September.
        ///
        /// `SunPosition` corrects for precession and nutation of the Earth's axis
        /// in order to obtain the exact equatorial plane at the given time.
        ///
        /// This function can be used for calculating changes of seasons: equinoxes and solstices.
        /// In fact, the function #Seasons does use this function for that purpose.
        /// </remarks>
        /// <param name="time">
        /// The date and time for which to calculate the Sun's position.
        /// </param>
        /// <returns>
        /// The ecliptic coordinates of the Sun using the Earth's true equator of date.
        /// </returns>
        public static Ecliptic SunPosition(AstroTime time)
        {
            /* Correct for light travel time from the Sun. */
            /* Otherwise season calculations (equinox, solstice) will all be early by about 8 minutes! */
            AstroTime adjusted_time = time.AddDays(-1.0 / C_AUDAY);

            AstroVector earth2000 = CalcEarth(adjusted_time);

            /* Convert heliocentric location of Earth to geocentric location of Sun. */
            AstroVector sun2000 = new AstroVector(-earth2000.x, -earth2000.y, -earth2000.z, adjusted_time);

            /* Convert to equatorial Cartesian coordinates of date. */
            AstroVector stemp = precession(0.0, sun2000, adjusted_time.tt);
            AstroVector sun_ofdate = nutation(adjusted_time, 0, stemp);

            /* Convert equatorial coordinates to ecliptic coordinates. */
            double true_obliq = DEG2RAD * e_tilt(adjusted_time).tobl;
            return RotateEquatorialToEcliptic(sun_ofdate, true_obliq);
        }

        private static Ecliptic RotateEquatorialToEcliptic(AstroVector pos, double obliq_radians)
        {
            double cos_ob = Math.Cos(obliq_radians);
            double sin_ob = Math.Sin(obliq_radians);

            double ex = +pos.x;
            double ey = +pos.y*cos_ob + pos.z*sin_ob;
            double ez = -pos.y*sin_ob + pos.z*cos_ob;

            double xyproj = Math.Sqrt(ex*ex + ey*ey);
            double elon = 0.0;
            if (xyproj > 0.0)
            {
                elon = RAD2DEG * Math.Atan2(ey, ex);
                if (elon < 0.0)
                    elon += 360.0;
            }

            double elat = RAD2DEG * Math.Atan2(ez, xyproj);

            return new Ecliptic(ex, ey, ez, elat, elon);
        }

        /// <summary>
        /// Converts J2000 equatorial Cartesian coordinates to J2000 ecliptic coordinates.
        /// </summary>
        /// <remarks>
        /// Given coordinates relative to the Earth's equator at J2000 (the instant of noon UTC
        /// on 1 January 2000), this function converts those coordinates to J2000 ecliptic coordinates,
        /// which are relative to the plane of the Earth's orbit around the Sun.
        /// </remarks>
        /// <param name="equ">
        /// Equatorial coordinates in the J2000 frame of reference.
        /// You can call #GeoVector to obtain suitable equatorial coordinates.
        /// </param>
        /// <returns>Ecliptic coordinates in the J2000 frame of reference.</returns>
        public static Ecliptic EquatorialToEcliptic(AstroVector equ)
        {
            /* Based on NOVAS functions equ2ecl() and equ2ecl_vec(). */
            const double ob2000 = 0.40909260059599012;   /* mean obliquity of the J2000 ecliptic in radians */
            return RotateEquatorialToEcliptic(equ, ob2000);
        }

        /// <summary>
        /// Finds both equinoxes and both solstices for a given calendar year.
        /// </summary>
        /// <remarks>
        /// The changes of seasons are defined by solstices and equinoxes.
        /// Given a calendar year number, this function calculates the
        /// March and September equinoxes and the June and December solstices.
        ///
        /// The equinoxes are the moments twice each year when the plane of the
        /// Earth's equator passes through the center of the Sun. In other words,
        /// the Sun's declination is zero at both equinoxes.
        /// The March equinox defines the beginning of spring in the northern hemisphere
        /// and the beginning of autumn in the southern hemisphere.
        /// The September equinox defines the beginning of autumn in the northern hemisphere
        /// and the beginning of spring in the southern hemisphere.
        ///
        /// The solstices are the moments twice each year when one of the Earth's poles
        /// is most tilted toward the Sun. More precisely, the Sun's declination reaches
        /// its minimum value at the December solstice, which defines the beginning of
        /// winter in the northern hemisphere and the beginning of summer in the southern
        /// hemisphere. The Sun's declination reaches its maximum value at the June solstice,
        /// which defines the beginning of summer in the northern hemisphere and the beginning
        /// of winter in the southern hemisphere.
        /// </remarks>
        /// <param name="year">
        /// The calendar year number for which to calculate equinoxes and solstices.
        /// The value may be any integer, but only the years 1800 through 2100 have been
        /// validated for accuracy: unit testing against data from the
        /// United States Naval Observatory confirms that all equinoxes and solstices
        /// for that range of years are within 2 minutes of the correct time.
        /// </param>
        /// <returns>
        /// A #SeasonsInfo structure that contains four #AstroTime values:
        /// the March and September equinoxes and the June and December solstices.
        /// </returns>
        public static SeasonsInfo Seasons(int year)
        {
            return new SeasonsInfo(
                FindSeasonChange(  0, year,  3, 19),
                FindSeasonChange( 90, year,  6, 19),
                FindSeasonChange(180, year,  9, 21),
                FindSeasonChange(270, year, 12, 20)
            );
        }

        private static AstroTime FindSeasonChange(double targetLon, int year, int month, int day)
        {
            var startTime = new AstroTime(year, month, day, 0, 0, 0);
            return SearchSunLongitude(targetLon, startTime, 4.0);
        }

        /// <summary>
        /// Searches for the time when the Sun reaches an apparent ecliptic longitude as seen from the Earth.
        /// </summary>
        /// <remarks>
        /// This function finds the moment in time, if any exists in the given time window,
        /// that the center of the Sun reaches a specific ecliptic longitude as seen from the center of the Earth.
        ///
        /// This function can be used to determine equinoxes and solstices.
        /// However, it is usually more convenient and efficient to call #Seasons
        /// to calculate all equinoxes and solstices for a given calendar year.
        ///
        /// The function searches the window of time specified by `startTime` and `startTime+limitDays`.
        /// The search will return an error if the Sun never reaches the longitude `targetLon` or
        /// if the window is so large that the longitude ranges more than 180 degrees within it.
        /// It is recommended to keep the window smaller than 10 days when possible.
        /// </remarks>
        /// <param name="targetLon">
        /// The desired ecliptic longitude in degrees, relative to the true equinox of date.
        /// This may be any value in the range [0, 360), although certain values have
        /// conventional meanings:
        /// 0 = March equinox, 90 = June solstice, 180 = September equinox, 270 = December solstice.
        /// </param>
        /// <param name="startTime">
        /// The date and time for starting the search for the desired longitude event.
        /// </param>
        /// <param name="limitDays">
        /// The real-valued number of days, which when added to `startTime`, limits the
        /// range of time over which the search looks.
        /// It is recommended to keep this value between 1 and 10 days.
        /// See remarks above for more details.
        /// </param>
        /// <returns>
        /// The date and time when the Sun reaches the specified apparent ecliptic longitude.
        /// </returns>
        public static AstroTime SearchSunLongitude(double targetLon, AstroTime startTime, double limitDays)
        {
            var sun_offset = new SearchContext_SunOffset(targetLon);
            AstroTime t2 = startTime.AddDays(limitDays);
            return Search(sun_offset, startTime, t2, 1.0);
        }

        /// <summary>
        /// Searches for a time at which a function's value increases through zero.
        /// </summary>
        /// <remarks>
        /// Certain astronomy calculations involve finding a time when an event occurs.
        /// Often such events can be defined as the root of a function:
        /// the time at which the function's value becomes zero.
        ///
        /// `Search` finds the *ascending root* of a function: the time at which
        /// the function's value becomes zero while having a positive slope. That is, as time increases,
        /// the function transitions from a negative value, through zero at a specific moment,
        /// to a positive value later. The goal of the search is to find that specific moment.
        ///
        /// The `func` parameter is an instance of the abstract class #SearchContext.
        /// As an example, a caller may wish to find the moment a celestial body reaches a certain
        /// ecliptic longitude. In that case, the caller might derive a class that contains
        /// a #Body member to specify the body and a `double` to hold the target longitude.
        /// It could subtract the target longitude from the actual longitude at a given time;
        /// thus the difference would equal zero at the moment in time the planet reaches the
        /// desired longitude.
        ///
        /// Every call to `func.Eval` must either return a valid #AstroTime or throw an exception.
        ///
        /// The search calls `func.Eval` repeatedly to rapidly narrow in on any ascending
        /// root within the time window specified by `t1` and `t2`. The search never
        /// reports a solution outside this time window.
        ///
        /// `Search` uses a combination of bisection and quadratic interpolation
        /// to minimize the number of function calls. However, it is critical that the
        /// supplied time window be small enough that there cannot be more than one root
        /// (ascedning or descending) within it; otherwise the search can fail.
        /// Beyond that, it helps to make the time window as small as possible, ideally
        /// such that the function itself resembles a smooth parabolic curve within that window.
        ///
        /// If an ascending root is not found, or more than one root
        /// (ascending and/or descending) exists within the window `t1`..`t2`,
        /// the search will return `null`.
        ///
        /// If the search does not converge within 20 iterations, it will throw an exception.
        /// </remarks>
        /// <param name="func">
        /// The function for which to find the time of an ascending root.
        /// See remarks above for more details.
        /// </param>
        /// <param name="t1">
        /// The lower time bound of the search window.
        /// See remarks above for more details.
        /// </param>
        /// <param name="t2">
        /// The upper time bound of the search window.
        /// See remarks above for more details.
        /// </param>
        /// <param name="dt_tolerance_seconds">
        /// Specifies an amount of time in seconds within which a bounded ascending root
        /// is considered accurate enough to stop. A typical value is 1 second.
        /// </param>
        /// <returns>
        /// If successful, returns an #AstroTime value indicating a date and time
        /// that is within `dt_tolerance_seconds` of an ascending root.
        /// If no ascending root is found, or more than one root exists in the time
        /// window `t1`..`t2`, the function returns `null`.
        /// If the search does not converge within 20 iterations, an exception is thrown.
        /// </returns>
        public static AstroTime Search(
            SearchContext func,
            AstroTime t1,
            AstroTime t2,
            double dt_tolerance_seconds)
        {
            const int iter_limit = 20;
            double dt_days = Math.Abs(dt_tolerance_seconds / SECONDS_PER_DAY);
            double f1 = func.Eval(t1);
            double f2 = func.Eval(t2);
            int iter = 0;
            bool calc_fmid = true;
            double fmid = 0.0;
            for(;;)
            {
                if (++iter > iter_limit)
                    throw new Exception(string.Format("Search did not converge within {0} iterations.", iter_limit));

                double dt = (t2.tt - t1.tt) / 2.0;
                AstroTime tmid = t1.AddDays(dt);
                if (Math.Abs(dt) < dt_days)
                {
                    /* We are close enough to the event to stop the search. */
                    return tmid;
                }

                if (calc_fmid)
                    fmid = func.Eval(tmid);
                else
                    calc_fmid = true;   /* we already have the correct value of fmid from the previous loop */

                /* Quadratic interpolation: */
                /* Try to find a parabola that passes through the 3 points we have sampled: */
                /* (t1,f1), (tmid,fmid), (t2,f2) */

                double q_x, q_ut, q_df_dt;
                if (QuadInterp(tmid.ut, t2.ut - tmid.ut, f1, fmid, f2, out q_x, out q_ut, out q_df_dt))
                {
                    var tq = new AstroTime(q_ut);
                    double fq = func.Eval(tq);
                    if (q_df_dt != 0.0)
                    {
                        double dt_guess = Math.Abs(fq / q_df_dt);
                        if (dt_guess < dt_days)
                        {
                            /* The estimated time error is small enough that we can quit now. */
                            return tq;
                        }

                        /* Try guessing a tighter boundary with the interpolated root at the center. */
                        dt_guess *= 1.2;
                        if (dt_guess < dt/10.0)
                        {
                            AstroTime tleft = tq.AddDays(-dt_guess);
                            AstroTime tright = tq.AddDays(+dt_guess);
                            if ((tleft.ut - t1.ut)*(tleft.ut - t2.ut) < 0.0)
                            {
                                if ((tright.ut - t1.ut)*(tright.ut - t2.ut) < 0.0)
                                {
                                    double fleft, fright;
                                    fleft = func.Eval(tleft);
                                    fright = func.Eval(tright);
                                    if (fleft<0.0 && fright>=0.0)
                                    {
                                        f1 = fleft;
                                        f2 = fright;
                                        t1 = tleft;
                                        t2 = tright;
                                        fmid = fq;
                                        calc_fmid = false;  /* save a little work -- no need to re-calculate fmid next time around the loop */
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }

                /* After quadratic interpolation attempt. */
                /* Now just divide the region in two parts and pick whichever one appears to contain a root. */
                if (f1 < 0.0 && fmid >= 0.0)
                {
                    t2 = tmid;
                    f2 = fmid;
                    continue;
                }

                if (fmid < 0.0 && f2 >= 0.0)
                {
                    t1 = tmid;
                    f1 = fmid;
                    continue;
                }

                /* Either there is no ascending zero-crossing in this range */
                /* or the search window is too wide (more than one zero-crossing). */
                return null;
            }
        }

        private static bool QuadInterp(
            double tm, double dt, double fa, double fm, double fb,
            out double out_x, out double out_t, out double out_df_dt)
        {
            double Q, R, S;
            double u, ru, x1, x2;

            out_x = out_t = out_df_dt = 0.0;

            Q = (fb + fa)/2.0 - fm;
            R = (fb - fa)/2.0;
            S = fm;

            if (Q == 0.0)
            {
                /* This is a line, not a parabola. */
                if (R == 0.0)
                    return false;       /* This is a HORIZONTAL line... can't make progress! */
                out_x = -S / R;
                if (out_x < -1.0 || out_x > +1.0)
                    return false;   /* out of bounds */
            }
            else
            {
                /* This really is a parabola. Find roots x1, x2. */
                u = R*R - 4*Q*S;
                if (u <= 0.0)
                    return false;   /* can't solve if imaginary, or if vertex of parabola is tangent. */

                ru = Math.Sqrt(u);
                x1 = (-R + ru) / (2.0 * Q);
                x2 = (-R - ru) / (2.0 * Q);
                if (-1.0 <= x1 && x1 <= +1.0)
                {
                    if (-1.0 <= x2 && x2 <= +1.0)
                        return false;   /* two roots are within bounds; we require a unique zero-crossing. */
                    out_x = x1;
                }
                else if (-1.0 <= x2 && x2 <= +1.0)
                    out_x = x2;
                else
                    return false;   /* neither root is within bounds */
            }

            out_t = tm + out_x*dt;
            out_df_dt = (2*Q*out_x + R) / dt;
            return true;   /* success */
        }

        ///
        /// <summary>
        /// Returns a body's ecliptic longitude with respect to the Sun, as seen from the Earth.
        /// </summary>
        /// <remarks>
        /// This function can be used to determine where a planet appears around the ecliptic plane
        /// (the plane of the Earth's orbit around the Sun) as seen from the Earth,
        /// relative to the Sun's apparent position.
        ///
        /// The angle starts at 0 when the body and the Sun are at the same ecliptic longitude
        /// as seen from the Earth. The angle increases in the prograde direction
        /// (the direction that the planets orbit the Sun and the Moon orbits the Earth).
        ///
        /// When the angle is 180 degrees, it means the Sun and the body appear on opposite sides
        /// of the sky for an Earthly observer. When `body` is a planet whose orbit around the
        /// Sun is farther than the Earth's, 180 degrees indicates opposition. For the Moon,
        /// it indicates a full moon.
        ///
        /// The angle keeps increasing up to 360 degrees as the body's apparent prograde
        /// motion continues relative to the Sun. When the angle reaches 360 degrees, it starts
        /// over at 0 degrees.
        ///
        /// Values between 0 and 180 degrees indicate that the body is visible in the evening sky
        /// after sunset.  Values between 180 degrees and 360 degrees indicate that the body
        /// is visible in the morning sky before sunrise.
        /// </remarks>
        /// <param name="body">The celestial body for which to find longitude from the Sun.</param>
        /// <param name="time">The date and time of the observation.</param>
        /// <returns>
        /// A value in the range [0, 360), expressed in degrees.
        /// </returns>
        public static double LongitudeFromSun(Body body, AstroTime time)
        {
            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            AstroVector sv = GeoVector(Body.Sun, time, Aberration.Corrected);
            Ecliptic se = EquatorialToEcliptic(sv);

            AstroVector bv = GeoVector(body, time, Aberration.Corrected);
            Ecliptic be = EquatorialToEcliptic(bv);

            return NormalizeLongitude(be.elon - se.elon);
        }

        /// <summary>
        /// Returns the Moon's phase as an angle from 0 to 360 degrees.
        /// </summary>
        /// <remarks>
        /// This function determines the phase of the Moon using its apparent
        /// ecliptic longitude relative to the Sun, as seen from the center of the Earth.
        /// Certain values of the angle have conventional definitions:
        ///
        /// - 0 = new moon
        /// - 90 = first quarter
        /// - 180 = full moon
        /// - 270 = third quarter
        /// </remarks>
        /// <param name="time">The date and time of the observation.</param>
        /// <returns>The angle as described above, a value in the range 0..360 degrees.</returns>
        public static double MoonPhase(AstroTime time)
        {
            return LongitudeFromSun(Body.Moon, time);
        }

        /// <summary>
        /// Finds the first lunar quarter after the specified date and time.
        /// </summary>
        /// <remarks>
        /// A lunar quarter is one of the following four lunar phase events:
        /// new moon, first quarter, full moon, third quarter.
        /// This function finds the lunar quarter that happens soonest
        /// after the specified date and time.
        ///
        /// To continue iterating through consecutive lunar quarters, call this function once,
        /// followed by calls to #NextMoonQuarter as many times as desired.
        /// </remarks>
        /// <param name="startTime">The date and time at which to start the search.</param>
        /// <returns>
        /// A #MoonQuarterInfo structure reporting the next quarter phase and the time it will occur.
        /// </returns>
        public static MoonQuarterInfo SearchMoonQuarter(AstroTime startTime)
        {
            double angres = MoonPhase(startTime);
            int quarter = (1 + (int)Math.Floor(angres / 90.0)) % 4;
            AstroTime qtime = SearchMoonPhase(90.0 * quarter, startTime, 10.0);
            if (qtime == null)
                throw new Exception(string.Format("Internal error: could not find moon quarter {0} after {1}", quarter, startTime));
            return new MoonQuarterInfo(quarter, qtime);
        }

        /// <summary>
        /// Continues searching for lunar quarters from a previous search.
        /// </summary>
        /// <remarks>
        /// After calling #SearchMoonQuarter, this function can be called
        /// one or more times to continue finding consecutive lunar quarters.
        /// This function finds the next consecutive moon quarter event after
        /// the one passed in as the parameter `mq`.
        /// </remarks>
        /// <param name="mq">The previous moon quarter found by a call to #SearchMoonQuarter or NextMoonQuarter.</param>
        /// <returns>The moon quarter that occurs next in time after the one passed in `mq`.</returns>
        public static MoonQuarterInfo NextMoonQuarter(MoonQuarterInfo mq)
        {
            /* Skip 6 days past the previous found moon quarter to find the next one. */
            /* This is less than the minimum possible increment. */
            /* So far I have seen the interval well contained by the range (6.5, 8.3) days. */

            AstroTime time = mq.time.AddDays(6.0);
            MoonQuarterInfo next_mq = SearchMoonQuarter(time);
            /* Verify that we found the expected moon quarter. */
            if (next_mq.quarter != (1 + mq.quarter) % 4)
                throw new Exception("Internal error: found the wrong moon quarter.");
            return next_mq;
        }

        ///
        /// <summary>Searches for the time that the Moon reaches a specified phase.</summary>
        /// <remarks>
        /// Lunar phases are conventionally defined in terms of the Moon's geocentric ecliptic
        /// longitude with respect to the Sun's geocentric ecliptic longitude.
        /// When the Moon and the Sun have the same longitude, that is defined as a new moon.
        /// When their longitudes are 180 degrees apart, that is defined as a full moon.
        ///
        /// This function searches for any value of the lunar phase expressed as an
        /// angle in degrees in the range [0, 360).
        ///
        /// If you want to iterate through lunar quarters (new moon, first quarter, full moon, third quarter)
        /// it is much easier to call the functions #SearchMoonQuarter and #NextMoonQuarter.
        /// This function is useful for finding general phase angles outside those four quarters.
        /// </remarks>
        /// <param name="targetLon">
        /// The difference in geocentric longitude between the Sun and Moon
        /// that specifies the lunar phase being sought. This can be any value
        /// in the range [0, 360).  Certain values have conventional names:
        /// 0 = new moon, 90 = first quarter, 180 = full moon, 270 = third quarter.
        /// </param>
        /// <param name="startTime">
        /// The beginning of the time window in which to search for the Moon reaching the specified phase.
        /// </param>
        /// <param name="limitDays">
        /// The number of days after `startTime` that limits the time window for the search.
        /// </param>
        /// <returns>
        /// If successful, returns the date and time the moon reaches the phase specified by
        /// `targetlon`. This function will return null if the phase does not occur within
        /// `limitDays` of `startTime`; that is, if the search window is too small.
        /// </returns>
        public static AstroTime SearchMoonPhase(double targetLon, AstroTime startTime, double limitDays)
        {
            /*
                To avoid discontinuities in the moon_offset function causing problems,
                we need to approximate when that function will next return 0.
                We probe it with the start time and take advantage of the fact
                that every lunar phase repeats roughly every 29.5 days.
                There is a surprising uncertainty in the quarter timing,
                due to the eccentricity of the moon's orbit.
                I have seen up to 0.826 days away from the simple prediction.
                To be safe, we take the predicted time of the event and search
                +/-0.9 days around it (a 1.8-day wide window).
                Return null if the final result goes beyond limitDays after startTime.
            */

            const double uncertainty = 0.9;
            var moon_offset = new SearchContext_MoonOffset(targetLon);

            double ya = moon_offset.Eval(startTime);
            if (ya > 0.0) ya -= 360.0;  /* force searching forward in time, not backward */
            double est_dt = -(MEAN_SYNODIC_MONTH * ya) / 360.0;
            double dt1 = est_dt - uncertainty;
            if (dt1 > limitDays)
                return null;    /* not possible for moon phase to occur within specified window (too short) */
            double dt2 = est_dt + uncertainty;
            if (limitDays < dt2)
                dt2 = limitDays;
            AstroTime t1 = startTime.AddDays(dt1);
            AstroTime t2 = startTime.AddDays(dt2);
            return Search(moon_offset, t1, t2, 1.0);
        }

        /// <summary>
        /// Searches for the next time a celestial body rises or sets as seen by an observer on the Earth.
        /// </summary>
        /// <remarks>
        /// This function finds the next rise or set time of the Sun, Moon, or planet other than the Earth.
        /// Rise time is when the body first starts to be visible above the horizon.
        /// For example, sunrise is the moment that the top of the Sun first appears to peek above the horizon.
        /// Set time is the moment when the body appears to vanish below the horizon.
        ///
        /// This function corrects for typical atmospheric refraction, which causes celestial
        /// bodies to appear higher above the horizon than they would if the Earth had no atmosphere.
        /// It also adjusts for the apparent angular radius of the observed body (significant only for the Sun and Moon).
        ///
        /// Note that rise or set may not occur in every 24 hour period.
        /// For example, near the Earth's poles, there are long periods of time where
        /// the Sun stays below the horizon, never rising.
        /// Also, it is possible for the Moon to rise just before midnight but not set during the subsequent 24-hour day.
        /// This is because the Moon sets nearly an hour later each day due to orbiting the Earth a
        /// significant amount during each rotation of the Earth.
        /// Therefore callers must not assume that the function will always succeed.
        /// </remarks>
        ///
        /// <param name="body">The Sun, Moon, or any planet other than the Earth.</param>
        ///
        /// <param name="observer">The location where observation takes place.</param>
        ///
        /// <param name="direction">
        ///      Either `Direction.Rise` to find a rise time or `Direction.Set` to find a set time.
        /// </param>
        ///
        /// <param name="startTime">The date and time at which to start the search.</param>
        ///
        /// <param name="limitDays">
        /// Limits how many days to search for a rise or set time.
        /// To limit a rise or set time to the same day, you can use a value of 1 day.
        /// In cases where you want to find the next rise or set time no matter how far
        /// in the future (for example, for an observer near the south pole), you can
        /// pass in a larger value like 365.
        /// </param>
        ///
        /// <returns>
        /// On success, returns the date and time of the rise or set time as requested.
        /// If the function returns `null`, it means the rise or set event does not occur
        /// within `limitDays` days of `startTime`. This is a normal condition,
        /// not an error.
        /// </returns>
        public static AstroTime SearchRiseSet(
            Body body,
            Observer observer,
            Direction direction,
            AstroTime startTime,
            double limitDays)
        {
            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            double ha_before, ha_after;
            switch (direction)
            {
                case Direction.Rise:
                    ha_before = 12.0;   /* minimum altitude (bottom) happens BEFORE the body rises. */
                    ha_after = 0.0;     /* maximum altitude (culmination) happens AFTER the body rises. */
                    break;

                case Direction.Set:
                    ha_before = 0.0;    /* culmination happens BEFORE the body sets. */
                    ha_after = 12.0;    /* bottom happens AFTER the body sets. */
                    break;

                default:
                    throw new ArgumentException(string.Format("Unsupported direction value {0}", direction));
            }

            var peak_altitude = new SearchContext_PeakAltitude(body, direction, observer);
            /*
                See if the body is currently above/below the horizon.
                If we are looking for next rise time and the body is below the horizon,
                we use the current time as the lower time bound and the next culmination
                as the upper bound.
                If the body is above the horizon, we search for the next bottom and use it
                as the lower bound and the next culmination after that bottom as the upper bound.
                The same logic applies for finding set times, only we swap the hour angles.
            */

            HourAngleInfo evt_before, evt_after;
            AstroTime time_start = startTime;
            double alt_before = peak_altitude.Eval(time_start);
            AstroTime time_before;
            if (alt_before > 0.0)
            {
                /* We are past the sought event, so we have to wait for the next "before" event (culm/bottom). */
                evt_before = SearchHourAngle(body, observer, ha_before, time_start);
                time_before = evt_before.time;
                alt_before = peak_altitude.Eval(time_before);
            }
            else
            {
                /* We are before or at the sought event, so we find the next "after" event (bottom/culm), */
                /* and use the current time as the "before" event. */
                time_before = time_start;
            }

            evt_after = SearchHourAngle(body, observer, ha_after, time_before);
            double alt_after = peak_altitude.Eval(evt_after.time);

            for(;;)
            {
                if (alt_before <= 0.0 && alt_after > 0.0)
                {
                    /* Search between evt_before and evt_after for the desired event. */
                    AstroTime result = Search(peak_altitude, time_before, evt_after.time, 1.0);
                    if (result != null)
                        return result;
                }

                /* If we didn't find the desired event, use evt_after.time to find the next before-event. */
                evt_before = SearchHourAngle(body, observer, ha_before, evt_after.time);
                evt_after = SearchHourAngle(body, observer, ha_after, evt_before.time);

                if (evt_before.time.ut >= time_start.ut + limitDays)
                    return null;

                time_before = evt_before.time;

                alt_before = peak_altitude.Eval(evt_before.time);
                alt_after = peak_altitude.Eval(evt_after.time);
            }
        }

        /// <summary>
        /// Searches for the time when a celestial body reaches a specified hour angle as seen by an observer on the Earth.
        /// </summary>
        ///
        /// <remarks>
        /// The *hour angle* of a celestial body indicates its position in the sky with respect
        /// to the Earth's rotation. The hour angle depends on the location of the observer on the Earth.
        /// The hour angle is 0 when the body reaches its highest angle above the horizon in a given day.
        /// The hour angle increases by 1 unit for every sidereal hour that passes after that point, up
        /// to 24 sidereal hours when it reaches the highest point again. So the hour angle indicates
        /// the number of hours that have passed since the most recent time that the body has culminated,
        /// or reached its highest point.
        ///
        /// This function searches for the next time a celestial body reaches the given hour angle
        /// after the date and time specified by `startTime`.
        /// To find when a body culminates, pass 0 for `hourAngle`.
        /// To find when a body reaches its lowest point in the sky, pass 12 for `hourAngle`.
        ///
        /// Note that, especially close to the Earth's poles, a body as seen on a given day
        /// may always be above the horizon or always below the horizon, so the caller cannot
        /// assume that a culminating object is visible nor that an object is below the horizon
        /// at its minimum altitude.
        ///
        /// On success, the function reports the date and time, along with the horizontal coordinates
        /// of the body at that time, as seen by the given observer.
        /// </remarks>
        ///
        /// <param name="body">
        /// The celestial body, which can the Sun, the Moon, or any planet other than the Earth.
        /// </param>
        ///
        /// <param name="observer">
        /// Indicates a location on or near the surface of the Earth where the observer is located.
        /// </param>
        ///
        /// <param name="hourAngle">
        /// An hour angle value in the range [0, 24) indicating the number of sidereal hours after the
        /// body's most recent culmination.
        /// </param>
        ///
        /// <param name="startTime">
        /// The date and time at which to start the search.
        /// </param>
        ///
        /// <returns>
        /// This function returns a valid #HourAngleInfo object on success.
        /// If any error occurs, it throws an exception.
        /// It never returns a null value.
        /// </returns>
        public static HourAngleInfo SearchHourAngle(
            Body body,
            Observer observer,
            double hourAngle,
            AstroTime startTime)
        {
            int iter = 0;

            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            if (hourAngle < 0.0 || hourAngle >= 24.0)
                throw new ArgumentException("hourAngle is out of the allowed range [0, 24).");

            AstroTime time = startTime;
            for(;;)
            {
                ++iter;

                /* Calculate Greenwich Apparent Sidereal Time (GAST) at the given time. */
                double gast = sidereal_time(time);

                /* Obtain equatorial coordinates of date for the body. */
                Equatorial ofdate = Equator(body, time, observer, EquatorEpoch.OfDate, Aberration.Corrected);

                /* Calculate the adjustment needed in sidereal time */
                /* to bring the hour angle to the desired value. */

                double delta_sidereal_hours = ((hourAngle + ofdate.ra - observer.longitude/15.0) - gast) % 24.0;
                if (iter == 1)
                {
                    /* On the first iteration, always search forward in time. */
                    if (delta_sidereal_hours < 0.0)
                        delta_sidereal_hours += 24.0;
                }
                else
                {
                    /* On subsequent iterations, we make the smallest possible adjustment, */
                    /* either forward or backward in time. */
                    if (delta_sidereal_hours < -12.0)
                        delta_sidereal_hours += 24.0;
                    else if (delta_sidereal_hours > +12.0)
                        delta_sidereal_hours -= 24.0;
                }

                /* If the error is tolerable (less than 0.1 seconds), the search has succeeded. */
                if (Math.Abs(delta_sidereal_hours) * 3600.0 < 0.1)
                {
                    Topocentric hor = Horizon(time, observer, ofdate.ra, ofdate.dec, Refraction.Normal);
                    return new HourAngleInfo(time, hor);
                }

                /* We need to loop another time to get more accuracy. */
                /* Update the terrestrial time (in solar days) adjusting by sidereal time (sidereal hours). */
                time = time.AddDays((delta_sidereal_hours / 24.0) * SOLAR_DAYS_PER_SIDEREAL_DAY);
            }
        }

        /// <summary>
        ///      Searches for the time when the Earth and another planet are separated by a specified angle
        ///      in ecliptic longitude, as seen from the Sun.
        /// </summary>
        ///
        /// <remarks>
        /// A relative longitude is the angle between two bodies measured in the plane of the Earth's orbit
        /// (the ecliptic plane). The distance of the bodies above or below the ecliptic plane is ignored.
        /// If you imagine the shadow of the body cast onto the ecliptic plane, and the angle measured around
        /// that plane from one body to the other in the direction the planets orbit the Sun, you will get an
        /// angle somewhere between 0 and 360 degrees. This is the relative longitude.
        ///
        /// Given a planet other than the Earth in `body` and a time to start the search in `startTime`,
        /// this function searches for the next time that the relative longitude measured from the planet
        /// to the Earth is `targetRelLon`.
        ///
        /// Certain astronomical events are defined in terms of relative longitude between the Earth and another planet:
        ///
        /// - When the relative longitude is 0 degrees, it means both planets are in the same direction from the Sun.
        ///   For planets that orbit closer to the Sun (Mercury and Venus), this is known as *inferior conjunction*,
        ///   a time when the other planet becomes very difficult to see because of being lost in the Sun's glare.
        ///   (The only exception is in the rare event of a transit, when we see the silhouette of the planet passing
        ///   between the Earth and the Sun.)
        ///
        /// - When the relative longitude is 0 degrees and the other planet orbits farther from the Sun,
        ///   this is known as *opposition*.  Opposition is when the planet is closest to the Earth, and
        ///   also when it is visible for most of the night, so it is considered the best time to observe the planet.
        ///
        /// - When the relative longitude is 180 degrees, it means the other planet is on the opposite side of the Sun
        ///   from the Earth. This is called *superior conjunction*. Like inferior conjunction, the planet is
        ///   very difficult to see from the Earth. Superior conjunction is possible for any planet other than the Earth.
        /// </remarks>
        ///
        /// <param name="body">
        ///      A planet other than the Earth.
        ///      If `body` is `Body.Earth`, `Body.Sun`, or `Body.Moon`, this function throws an exception.
        /// </param>
        ///
        /// <param name="targetRelLon">
        ///      The desired relative longitude, expressed in degrees. Must be in the range [0, 360).
        /// </param>
        ///
        /// <param name="startTime">
        ///      The date and time at which to begin the search.
        /// </param>
        ///
        /// <returns>
        ///      If successful, returns the date and time of the relative longitude event.
        ///      Otherwise this function returns null.
        /// </returns>
        public static AstroTime SearchRelativeLongitude(Body body, double targetRelLon, AstroTime startTime)
        {
            if (body == Body.Earth || body == Body.Sun || body == Body.Moon)
                throw new ArgumentException(string.Format("{0} is not a valid body. Must be a planet other than the Earth.", body));

            double syn = SynodicPeriod(body);
            int direction = IsSuperiorPlanet(body) ? +1 : -1;

            /* Iterate until we converge on the desired event. */
            /* Calculate the error angle, which will be a negative number of degrees, */
            /* meaning we are "behind" the target relative longitude. */

            double error_angle = rlon_offset(body, startTime, direction, targetRelLon);
            if (error_angle > 0.0)
                error_angle -= 360.0;    /* force searching forward in time */

            AstroTime time = startTime;
            for (int iter = 0; iter < 100; ++iter)
            {
                /* Estimate how many days in the future (positive) or past (negative) */
                /* we have to go to get closer to the target relative longitude. */
                double day_adjust = (-error_angle/360.0) * syn;
                time = time.AddDays(day_adjust);
                if (Math.Abs(day_adjust) * SECONDS_PER_DAY < 1.0)
                    return time;

                double prev_angle = error_angle;
                error_angle = rlon_offset(body, time, direction, targetRelLon);
                if (Math.Abs(prev_angle) < 30.0 && (prev_angle != error_angle))
                {
                    /* Improve convergence for Mercury/Mars (eccentric orbits) */
                    /* by adjusting the synodic period to more closely match the */
                    /* variable speed of both planets in this part of their respective orbits. */
                    double ratio = prev_angle / (prev_angle - error_angle);
                    if (ratio > 0.5 && ratio < 2.0)
                        syn *= ratio;
                }
            }

            return null;    /* failed to converge */
        }

        private static double rlon_offset(Body body, AstroTime time, int direction, double targetRelLon)
        {
            double plon = EclipticLongitude(body, time);
            double elon = EclipticLongitude(Body.Earth, time);
            double diff = direction * (elon - plon);
            return LongitudeOffset(diff - targetRelLon);
        }

        private static double SynodicPeriod(Body body)
        {
            /* The Earth does not have a synodic period as seen from itself. */
            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            if (body == Body.Moon)
                return MEAN_SYNODIC_MONTH;

            double Tp = PlanetOrbitalPeriod(body);
            return Math.Abs(EARTH_ORBITAL_PERIOD / (EARTH_ORBITAL_PERIOD/Tp - 1.0));
        }

        /// <summary>Calculates heliocentric ecliptic longitude of a body based on the J2000 equinox.</summary>
        /// <remarks>
        /// This function calculates the angle around the plane of the Earth's orbit
        /// of a celestial body, as seen from the center of the Sun.
        /// The angle is measured prograde (in the direction of the Earth's orbit around the Sun)
        /// in degrees from the J2000 equinox. The ecliptic longitude is always in the range [0, 360).
        /// </remarks>
        ///
        /// <param name="body">A body other than the Sun.</param>
        ///
        /// <param name="time">The date and time at which the body's ecliptic longitude is to be calculated.</param>
        ///
        /// <returns>
        ///      Returns the ecliptic longitude in degrees of the given body at the given time.
        /// </returns>
        public static double EclipticLongitude(Body body, AstroTime time)
        {
            if (body == Body.Sun)
                throw new ArgumentException("Cannot calculate heliocentric longitude of the Sun.");

            AstroVector hv = HelioVector(body, time);
            Ecliptic eclip = EquatorialToEcliptic(hv);
            return eclip.elon;
        }

        private static double PlanetOrbitalPeriod(Body body)
        {
            /* Returns the number of days it takes for a planet to orbit the Sun. */
            switch (body)
            {
                case Body.Mercury:  return     87.969;
                case Body.Venus:    return    224.701;
                case Body.Earth:    return    EARTH_ORBITAL_PERIOD;
                case Body.Mars:     return    686.980;
                case Body.Jupiter:  return   4332.589;
                case Body.Saturn:   return  10759.22;
                case Body.Uranus:   return  30685.4;
                case Body.Neptune:  return  60189.0;
                case Body.Pluto:    return  90560.0;
                default:
                    throw new ArgumentException(string.Format("Invalid body {0}. Must be a planet.", body));
            }
        }

        private static bool IsSuperiorPlanet(Body body)
        {
            switch (body)
            {
                case Body.Mars:
                case Body.Jupiter:
                case Body.Saturn:
                case Body.Uranus:
                case Body.Neptune:
                case Body.Pluto:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines visibility of a celestial body relative to the Sun, as seen from the Earth.
        /// </summary>
        ///
        /// <remarks>
        /// This function returns an #ElongationInfo structure, which provides the following
        /// information about the given celestial body at the given time:
        ///
        /// - `visibility` is an enumerated type that specifies whether the body is more easily seen
        ///    in the morning before sunrise, or in the evening after sunset.
        ///
        /// - `elongation` is the angle in degrees between two vectors: one from the center of the Earth to the
        ///    center of the Sun, the other from the center of the Earth to the center of the specified body.
        ///    This angle indicates how far away the body is from the glare of the Sun.
        ///    The elongation angle is always in the range [0, 180].
        ///
        /// - `ecliptic_separation` is the absolute value of the difference between the body's ecliptic longitude
        ///   and the Sun's ecliptic longitude, both as seen from the center of the Earth. This angle measures
        ///   around the plane of the Earth's orbit, and ignores how far above or below that plane the body is.
        ///   The ecliptic separation is measured in degrees and is always in the range [0, 180].
        /// </remarks>
        ///
        /// <param name="body">
        ///      The celestial body whose visibility is to be calculated.
        /// </param>
        ///
        /// <param name="time">
        ///      The date and time of the observation.
        /// </param>
        ///
        /// <returns>
        /// Returns a valid #ElongationInfo structure, or throws an exception if there is an error.
        /// </returns>

        public static ElongationInfo Elongation(Body body, AstroTime time)
        {
            Visibility visibility;
            double ecliptic_separation = LongitudeFromSun(body, time);
            if (ecliptic_separation > 180.0)
            {
                visibility = Visibility.Morning;
                ecliptic_separation = 360.0 - ecliptic_separation;
            }
            else
            {
                visibility = Visibility.Evening;
            }

            double elongation = AngleFromSun(body, time);
            return new ElongationInfo(time, visibility, elongation, ecliptic_separation);
        }

        /// <summary>
        /// Finds a date and time when Mercury or Venus reaches its maximum angle from the Sun as seen from the Earth.
        /// </summary>
        ///
        /// <remarks>
        /// Mercury and Venus are are often difficult to observe because they are closer to the Sun than the Earth is.
        /// Mercury especially is almost always impossible to see because it gets lost in the Sun's glare.
        /// The best opportunities for spotting Mercury, and the best opportunities for viewing Venus through
        /// a telescope without atmospheric interference, are when these planets reach maximum elongation.
        /// These are events where the planets reach the maximum angle from the Sun as seen from the Earth.
        ///
        /// This function solves for those times, reporting the next maximum elongation event's date and time,
        /// the elongation value itself, the relative longitude with the Sun, and whether the planet is best
        /// observed in the morning or evening. See #Astronomy_Elongation for more details about the returned structure.
        /// </remarks>
        ///
        /// <param name="body">
        /// Either `Body.Mercury` or `Body.Venus`. Any other value will result in an exception.
        /// To find the best viewing opportunites for planets farther from the Sun than the Earth is (Mars through Pluto)
        /// use #SearchRelativeLongitude to find the next opposition event.
        /// </param>
        ///
        /// <param name="startTime">
        /// The date and time at which to begin the search. The maximum elongation event found will always
        /// be the first one that occurs after this date and time.
        /// </param>
        ///
        /// <returns>
        /// Either an exception will be thrown, or the function will return a valid value.
        /// </returns>
        public static ElongationInfo SearchMaxElongation(Body body, AstroTime startTime)
        {
            double s1, s2;
            switch (body)
            {
                case Body.Mercury:
                    s1 = 50.0;
                    s2 = 85.0;
                    break;

                case Body.Venus:
                    s1 = 40.0;
                    s2 = 50.0;
                    break;

                default:
                    throw new ArgumentException(string.Format("Invalid body {0}. Must be either Mercury or Venus.", body));
            }

            double syn = SynodicPeriod(body);
            var neg_elong_slope = new SearchContext_NegElongSlope(body);

            for (int iter=0; ++iter <= 2;)
            {
                double plon = EclipticLongitude(body, startTime);
                double elon = EclipticLongitude(Body.Earth, startTime);
                double rlon = LongitudeOffset(plon - elon);     /* clamp to (-180, +180] */

                /* The slope function is not well-behaved when rlon is near 0 degrees or 180 degrees */
                /* because there is a cusp there that causes a discontinuity in the derivative. */
                /* So we need to guard against searching near such times. */
                double adjust_days, rlon_lo, rlon_hi;
                if (rlon >= -s1 && rlon < +s1)
                {
                    /* Seek to the window [+s1, +s2]. */
                    adjust_days = 0.0;
                    /* Search forward for the time t1 when rel lon = +s1. */
                    rlon_lo = +s1;
                    /* Search forward for the time t2 when rel lon = +s2. */
                    rlon_hi = +s2;
                }
                else if (rlon > +s2 || rlon < -s2)
                {
                    /* Seek to the next search window at [-s2, -s1]. */
                    adjust_days = 0.0;
                    /* Search forward for the time t1 when rel lon = -s2. */
                    rlon_lo = -s2;
                    /* Search forward for the time t2 when rel lon = -s1. */
                    rlon_hi = -s1;
                }
                else if (rlon >= 0.0)
                {
                    /* rlon must be in the middle of the window [+s1, +s2]. */
                    /* Search BACKWARD for the time t1 when rel lon = +s1. */
                    adjust_days = -syn / 4.0;
                    rlon_lo = +s1;
                    rlon_hi = +s2;
                    /* Search forward from t1 to find t2 such that rel lon = +s2. */
                }
                else
                {
                    /* rlon must be in the middle of the window [-s2, -s1]. */
                    /* Search BACKWARD for the time t1 when rel lon = -s2. */
                    adjust_days = -syn / 4.0;
                    rlon_lo = -s2;
                    /* Search forward from t1 to find t2 such that rel lon = -s1. */
                    rlon_hi = -s1;
                }

                AstroTime t_start = startTime.AddDays(adjust_days);

                AstroTime t1 = SearchRelativeLongitude(body, rlon_lo, t_start);
                AstroTime t2 = SearchRelativeLongitude(body, rlon_hi, t1);

                /* Now we have a time range [t1,t2] that brackets a maximum elongation event. */
                /* Confirm the bracketing. */
                double m1 = neg_elong_slope.Eval(t1);
                if (m1 >= 0.0)
                    throw new Exception("There is a bug in the bracketing algorithm! m1 = " + m1);

                double m2 = neg_elong_slope.Eval(t2);
                if (m2 <= 0.0)
                    throw new Exception("There is a bug in the bracketing algorithm! m2 = " + m2);

                /* Use the generic search algorithm to home in on where the slope crosses from negative to positive. */
                AstroTime searchx = Search(neg_elong_slope, t1, t2, 10.0);
                if (searchx == null)
                    throw new Exception("Maximum elongation search failed.");

                if (searchx.tt >= startTime.tt)
                    return Elongation(body, searchx);

                /* This event is in the past (earlier than startTime). */
                /* We need to search forward from t2 to find the next possible window. */
                /* We never need to search more than twice. */
                startTime = t2.AddDays(1.0);
            }

            throw new Exception("Maximum elongation search iterated too many times.");
        }

        ///
        /// <summary>Returns the angle between the given body and the Sun, as seen from the Earth.</summary>
        ///
        /// <remarks>
        /// This function calculates the angular separation between the given body and the Sun,
        /// as seen from the center of the Earth. This angle is helpful for determining how
        /// easy it is to see the body away from the glare of the Sun.
        /// </remarks>
        ///
        /// <param name="body">
        /// The celestial body whose angle from the Sun is to be measured.
        /// Not allowed to be `Body.Earth`.
        /// </param>
        ///
        /// <param name="time">
        /// The time at which the observation is made.
        /// </param>
        ///
        /// <returns>
        /// Returns the angle in degrees between the Sun and the specified body as
        /// seen from the center of the Earth.
        /// </returns>
        public static double AngleFromSun(Body body, AstroTime time)
        {
            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            AstroVector sv = GeoVector(Body.Sun, time, Aberration.Corrected);
            AstroVector bv = GeoVector(body, time, Aberration.Corrected);
            return AngleBetween(sv, bv);
        }

        private static double AngleBetween(AstroVector a, AstroVector b)
        {
            double r = a.Length() * b.Length();
            if (r < 1.0e-8)
                throw new Exception("Cannot find angle between vectors because they are too short.");

            double dot = (a.x*b.x + a.y*b.y + a.z*b.z) / r;

            if (dot <= -1.0)
                return 180.0;

            if (dot >= +1.0)
                return 0.0;

            return RAD2DEG * Math.Acos(dot);
        }

        /// <summary>
        ///      Finds the date and time of the Moon's closest distance (perigee)
        ///      or farthest distance (apogee) with respect to the Earth.
        /// </summary>
        /// <remarks>
        /// Given a date and time to start the search in `startTime`, this function finds the
        /// next date and time that the center of the Moon reaches the closest or farthest point
        /// in its orbit with respect to the center of the Earth, whichever comes first
        /// after `startTime`.
        ///
        /// The closest point is called *perigee* and the farthest point is called *apogee*.
        /// The word *apsis* refers to either event.
        ///
        /// To iterate through consecutive alternating perigee and apogee events, call `Astronomy.SearchLunarApsis`
        /// once, then use the return value to call #Astronomy.NextLunarApsis. After that,
        /// keep feeding the previous return value from `Astronomy_NextLunarApsis` into another
        /// call of `Astronomy_NextLunarApsis` as many times as desired.
        /// </remarks>
        /// <param name="startTime">
        ///      The date and time at which to start searching for the next perigee or apogee.
        /// </param>
        /// <returns>
        /// Returns an #ApsisInfo structure containing information about the next lunar apsis.
        /// </returns>
        public static ApsisInfo SearchLunarApsis(AstroTime startTime)
        {
            const double increment = 5.0;   /* number of days to skip in each iteration */
            var positive_slope = new SearchContext_MoonDistanceSlope(+1);
            var negative_slope = new SearchContext_MoonDistanceSlope(-1);

            /*
                Check the rate of change of the distance dr/dt at the start time.
                If it is positive, the Moon is currently getting farther away,
                so start looking for apogee.
                Conversely, if dr/dt < 0, start looking for perigee.
                Either way, the polarity of the slope will change, so the product will be negative.
                Handle the crazy corner case of exactly touching zero by checking for m1*m2 <= 0.
            */
            AstroTime t1 = startTime;
            double m1 = positive_slope.Eval(t1);
            for (int iter=0; iter * increment < 2.0 * Astronomy.MEAN_SYNODIC_MONTH; ++iter)
            {
                AstroTime t2 = t1.AddDays(increment);
                double m2 = positive_slope.Eval(t2);
                if (m1 * m2 <= 0.0)
                {
                    /* There is a change of slope polarity within the time range [t1, t2]. */
                    /* Therefore this time range contains an apsis. */
                    /* Figure out whether it is perigee or apogee. */

                    AstroTime search;
                    ApsisKind kind;
                    if (m1 < 0.0 || m2 > 0.0)
                    {
                        /* We found a minimum-distance event: perigee. */
                        /* Search the time range for the time when the slope goes from negative to positive. */
                        search = Search(positive_slope, t1, t2, 1.0);
                        kind = ApsisKind.Pericenter;
                    }
                    else if (m1 > 0.0 || m2 < 0.0)
                    {
                        /* We found a maximum-distance event: apogee. */
                        /* Search the time range for the time when the slope goes from positive to negative. */
                        search = Search(negative_slope, t1, t2, 1.0);
                        kind = ApsisKind.Apocenter;
                    }
                    else
                    {
                        /* This should never happen. It should not be possible for both slopes to be zero. */
                        throw new Exception("Internal error with slopes in SearchLunarApsis");
                    }

                    if (search == null)
                        throw new Exception("Failed to find slope transition in lunar apsis search.");

                    double dist_au = SearchContext_MoonDistanceSlope.MoonDistance(search);
                    return new ApsisInfo(search, kind, dist_au);
                }
                /* We have not yet found a slope polarity change. Keep searching. */
                t1 = t2;
                m1 = m2;
            }

            /* It should not be possible to fail to find an apsis within 2 synodic months. */
            throw new Exception("Internal error: should have found lunar apsis within 2 synodic months.");
        }

        /// <summary>
        /// Finds the next lunar perigee or apogee event in a series.
        /// </summary>
        /// <remarks>
        /// This function requires an #ApsisInfo value obtained from a call
        /// to #Astronomy.SearchLunarApsis or `Astronomy.NextLunarApsis`. Given
        /// an apogee event, this function finds the next perigee event, and vice versa.
        ///
        /// See #Astronomy_SearchLunarApsis for more details.
        /// </remarks>
        /// <param name="apsis">
        /// An apsis event obtained from a call to #Astronomy.SearchLunarApsis or `Astronomy.NextLunarApsis`.
        /// See #Astronomy.SearchLunarApsis for more details.
        /// </param>
        /// <returns>
        /// Same as the return value for #Astronomy_SearchLunarApsis.
        /// </returns>

        public static ApsisInfo NextLunarApsis(ApsisInfo apsis)
        {
            const double skip = 11.0;   // number of days to skip to start looking for next apsis event

            if (apsis.kind != ApsisKind.Pericenter && apsis.kind != ApsisKind.Apocenter)
                throw new ArgumentException("Invalid apsis kind");

            AstroTime time = apsis.time.AddDays(skip);
            ApsisInfo next =  SearchLunarApsis(time);
            if ((int)next.kind + (int)apsis.kind != 1)
                throw new Exception(string.Format("Internal error: previous apsis was {0}, but found {1} for next apsis.", apsis.kind, next.kind));
            return next;
        }

        /// <summary>
        /// Finds visual magnitude, phase angle, and other illumination information about a celestial body.
        /// </summary>
        /// <remarks>
        /// This function calculates information about how bright a celestial body appears from the Earth,
        /// reported as visual magnitude, which is a smaller (or even negative) number for brighter objects
        /// and a larger number for dimmer objects.
        ///
        /// For bodies other than the Sun, it reports a phase angle, which is the angle in degrees between
        /// the Sun and the Earth, as seen from the center of the body. Phase angle indicates what fraction
        /// of the body appears illuminated as seen from the Earth. For example, when the phase angle is
        /// near zero, it means the body appears "full" as seen from the Earth.  A phase angle approaching
        /// 180 degrees means the body appears as a thin crescent as seen from the Earth.  A phase angle
        /// of 90 degrees means the body appears "half full".
        /// For the Sun, the phase angle is always reported as 0; the Sun emits light rather than reflecting it,
        /// so it doesn't have a phase angle.
        ///
        /// When the body is Saturn, the returned structure contains a field `ring_tilt` that holds
        /// the tilt angle in degrees of Saturn's rings as seen from the Earth. A value of 0 means
        /// the rings appear edge-on, and are thus nearly invisible from the Earth. The `ring_tilt` holds
        /// 0 for all bodies other than Saturn.
        /// </remarks>
        /// <param name="body">The Sun, Moon, or any planet other than the Earth.</param>
        /// <param name="time">The date and time of the observation.</param>
        /// <returns>An #IllumInfo structure with fields as documented above.</returns>
        public static IllumInfo Illumination(Body body, AstroTime time)
        {
            if (body == Body.Earth)
                throw new EarthNotAllowedException();

            AstroVector earth = CalcEarth(time);

            AstroVector gc;
            AstroVector hc;
            double phase_angle;
            if (body == Body.Sun)
            {
                gc = new AstroVector(-earth.x, -earth.y, -earth.z, time);
                hc = new AstroVector(0.0, 0.0, 0.0, time);
                // The Sun emits light instead of reflecting it,
                // so we report a placeholder phase angle of 0.
                phase_angle = 0.0;
            }
            else
            {
                if (body == Body.Moon)
                {
                    // For extra numeric precision, use geocentric Moon formula directly.
                    gc = GeoMoon(time);
                    hc = new AstroVector(earth.x + gc.x, earth.y + gc.y, earth.z + gc.z, time);
                }
                else
                {
                    // For planets, the heliocentric vector is more direct to calculate.
                    hc = HelioVector(body, time);
                    gc = new AstroVector(hc.x - earth.x, hc.y - earth.y, hc.z - earth.z, time);
                }

                phase_angle = AngleBetween(gc, hc);
            }

            double geo_dist = gc.Length();
            double helio_dist = hc.Length();
            double ring_tilt = 0.0;

            double mag;
            switch (body)
            {
                case Body.Sun:
                    mag = -0.17 + 5.0*Math.Log10(geo_dist / AU_PER_PARSEC);
                    break;

                case Body.Moon:
                    mag = MoonMagnitude(phase_angle, helio_dist, geo_dist);
                    break;

                case Body.Saturn:
                    mag = SaturnMagnitude(phase_angle, helio_dist, geo_dist, gc, time, out ring_tilt);
                    break;

                default:
                    mag = VisualMagnitude(body, phase_angle, helio_dist, geo_dist);
                    break;
            }

            return new IllumInfo(time, mag, phase_angle, helio_dist, ring_tilt);
        }

        private static double MoonMagnitude(double phase, double helio_dist, double geo_dist)
        {
            /* https://astronomy.stackexchange.com/questions/10246/is-there-a-simple-analytical-formula-for-the-lunar-phase-brightness-curve */
            double rad = phase * DEG2RAD;
            double rad2 = rad * rad;
            double rad4 = rad2 * rad2;
            double mag = -12.717 + 1.49*Math.Abs(rad) + 0.0431*rad4;
            double moon_mean_distance_au = 385000.6 / KM_PER_AU;
            double geo_au = geo_dist / moon_mean_distance_au;
            mag += 5.0 * Math.Log10(helio_dist * geo_au);
            return mag;
        }

        private static double VisualMagnitude(
            Body body,
            double phase,
            double helio_dist,
            double geo_dist)
        {
            /* For Mercury and Venus, see:  https://iopscience.iop.org/article/10.1086/430212 */
            double c0, c1=0, c2=0, c3=0;
            switch (body)
            {
                case Body.Mercury:
                    c0 = -0.60; c1 = +4.98; c2 = -4.88; c3 = +3.02; break;
                case Body.Venus:
                    if (phase < 163.6)
                    {
                        c0 = -4.47; c1 = +1.03; c2 = +0.57; c3 = +0.13;
                    }
                    else
                    {
                        c0 = 0.98; c1 = -1.02;
                    }
                    break;
                case Body.Mars:        c0 = -1.52; c1 = +1.60;   break;
                case Body.Jupiter:     c0 = -9.40; c1 = +0.50;   break;
                case Body.Uranus:      c0 = -7.19; c1 = +0.25;   break;
                case Body.Neptune:     c0 = -6.87;               break;
                case Body.Pluto:       c0 = -1.00; c1 = +4.00;   break;
                default:
                    throw new ArgumentException(string.Format("Unsupported body {0}", body));
            }

            double x = phase / 100;
            double mag = c0 + x*(c1 + x*(c2 + x*c3));
            mag += 5.0 * Math.Log10(helio_dist * geo_dist);
            return mag;
        }

        private static double SaturnMagnitude(
            double phase,
            double helio_dist,
            double geo_dist,
            AstroVector gc,
            AstroTime time,
            out double ring_tilt)
        {
            /* Based on formulas by Paul Schlyter found here: */
            /* http://www.stjarnhimlen.se/comp/ppcomp.html#15 */

            /* We must handle Saturn's rings as a major component of its visual magnitude. */
            /* Find geocentric ecliptic coordinates of Saturn. */
            Ecliptic eclip = EquatorialToEcliptic(gc);

            double ir = DEG2RAD * 28.06;   /* tilt of Saturn's rings to the ecliptic, in radians */
            double Nr = DEG2RAD * (169.51 + (3.82e-5 * time.tt));    /* ascending node of Saturn's rings, in radians */

            /* Find tilt of Saturn's rings, as seen from Earth. */
            double lat = DEG2RAD * eclip.elat;
            double lon = DEG2RAD * eclip.elon;
            double tilt = Math.Asin(Math.Sin(lat)*Math.Cos(ir) - Math.Cos(lat)*Math.Sin(ir)*Math.Sin(lon-Nr));
            double sin_tilt = Math.Sin(Math.Abs(tilt));

            double mag = -9.0 + 0.044*phase;
            mag += sin_tilt*(-2.6 + 1.2*sin_tilt);
            mag += 5.0 * Math.Log10(helio_dist * geo_dist);

            ring_tilt = RAD2DEG * tilt;

            return mag;
        }

        /// <summary>Searches for the date and time Venus will next appear brightest as seen from the Earth.</summary>
        /// <remarks>
        /// This function searches for the date and time Venus appears brightest as seen from the Earth.
        /// Currently only Venus is supported for the `body` parameter, though this could change in the future.
        /// Mercury's peak magnitude occurs at superior conjunction, when it is virtually impossible to see from the Earth,
        /// so peak magnitude events have little practical value for that planet.
        /// Planets other than Venus and Mercury reach peak magnitude at opposition, which can
        /// be found using #Astronomy.SearchRelativeLongitude.
        /// The Moon reaches peak magnitude at full moon, which can be found using
        /// #Astronomy.SearchMoonQuarter or #Astronomy.SearchMoonPhase.
        /// The Sun reaches peak magnitude at perihelion, which occurs each year in January.
        /// However, the difference is minor and has little practical value.
        /// </remarks>
        ///
        /// <param name="body">
        ///      Currently only `Body.Venus` is allowed. Any other value causes an exception.
        ///      See remarks above for more details.
        /// </param>
        /// <param name="startTime">
        ///     The date and time to start searching for the next peak magnitude event.
        /// </param>
        /// <returns>
        ///      See documentation about the return value from #Astronomy.Illumination.
        /// </returns>
        public static IllumInfo SearchPeakMagnitude(Body body, AstroTime startTime)
        {
            /* s1 and s2 are relative longitudes within which peak magnitude of Venus can occur. */
            const double s1 = 10.0;
            const double s2 = 30.0;

            if (body != Body.Venus)
                throw new ArgumentException("Peak magnitude currently is supported for Venus only.");

            var mag_slope = new SearchContext_MagnitudeSlope(body);

            int iter = 0;
            while (++iter <= 2)
            {
                /* Find current heliocentric relative longitude between the */
                /* inferior planet and the Earth. */
                double plon = EclipticLongitude(body, startTime);
                double elon = EclipticLongitude(Body.Earth, startTime);
                double rlon = LongitudeOffset(plon - elon);     // clamp to (-180, +180].

                /* The slope function is not well-behaved when rlon is near 0 degrees or 180 degrees */
                /* because there is a cusp there that causes a discontinuity in the derivative. */
                /* So we need to guard against searching near such times. */

                double rlon_lo, rlon_hi, adjust_days, syn;
                if (rlon >= -s1 && rlon < +s1)
                {
                    /* Seek to the window [+s1, +s2]. */
                    adjust_days = 0.0;
                    /* Search forward for the time t1 when rel lon = +s1. */
                    rlon_lo = +s1;
                    /* Search forward for the time t2 when rel lon = +s2. */
                    rlon_hi = +s2;
                }
                else if (rlon >= +s2 || rlon < -s2)
                {
                    /* Seek to the next search window at [-s2, -s1]. */
                    adjust_days = 0.0;
                    /* Search forward for the time t1 when rel lon = -s2. */
                    rlon_lo = -s2;
                    /* Search forward for the time t2 when rel lon = -s1. */
                    rlon_hi = -s1;
                }
                else if (rlon >= 0)
                {
                    /* rlon must be in the middle of the window [+s1, +s2]. */
                    /* Search BACKWARD for the time t1 when rel lon = +s1. */
                    syn = SynodicPeriod(body);
                    adjust_days = -syn / 4;
                    rlon_lo = +s1;
                    /* Search forward from t1 to find t2 such that rel lon = +s2. */
                    rlon_hi = +s2;
                }
                else
                {
                    /* rlon must be in the middle of the window [-s2, -s1]. */
                    /* Search BACKWARD for the time t1 when rel lon = -s2. */
                    syn = SynodicPeriod(body);
                    adjust_days = -syn / 4;
                    rlon_lo = -s2;
                    /* Search forward from t1 to find t2 such that rel lon = -s1. */
                    rlon_hi = -s1;
                }
                AstroTime t_start = startTime.AddDays(adjust_days);
                AstroTime t1 = SearchRelativeLongitude(body, rlon_lo, t_start);
                if (t1 == null)
                    throw new Exception("Relative longitude search #1 failed.");
                AstroTime t2 = SearchRelativeLongitude(body, rlon_hi, t1);
                if (t2 == null)
                    throw new Exception("Relative longitude search #2 failed.");

                /* Now we have a time range [t1,t2] that brackets a maximum magnitude event. */
                /* Confirm the bracketing. */
                double m1 = mag_slope.Eval(t1);
                if (m1 >= 0.0)
                    throw new Exception("Internal error: m1 >= 0");    /* should never happen! */

                double m2 = mag_slope.Eval(t2);
                if (m2 <= 0.0)
                    throw new Exception("Internal error: m2 <= 0");    /* should never happen! */

                /* Use the generic search algorithm to home in on where the slope crosses from negative to positive. */
                AstroTime tx = Search(mag_slope, t1, t2, 10.0);
                if (tx == null)
                    throw new Exception("Failed to find magnitude slope transition.");

                if (tx.tt >= startTime.tt)
                    return Illumination(body, tx);

                /* This event is in the past (earlier than startTime). */
                /* We need to search forward from t2 to find the next possible window. */
                /* We never need to search more than twice. */
                startTime = t2.AddDays(1.0);
            }
            // This should never happen. If it does, please report as a bug in Astronomy Engine.
            throw new Exception("Peak magnitude search failed.");
        }
    }

    /// <summary>
    /// Represents a function whose ascending root is to be found.
    /// See #Search.
    /// </summary>
    public abstract class SearchContext
    {
        /// <summary>
        /// Evaluates the function at a given time
        /// </summary>
        /// <param name="time">The time at which to evaluate the function.</param>
        /// <returns>The floating point value of the function at the specified time.</returns>
        public abstract double Eval(AstroTime time);
    }

    internal class SearchContext_MagnitudeSlope: SearchContext
    {
        private readonly Body body;

        public SearchContext_MagnitudeSlope(Body body)
        {
            this.body = body;
        }

        public override double Eval(AstroTime time)
        {
            /*
                The Search() function finds a transition from negative to positive values.
                The derivative of magnitude y with respect to time t (dy/dt)
                is negative as an object gets brighter, because the magnitude numbers
                get smaller. At peak magnitude dy/dt = 0, then as the object gets dimmer,
                dy/dt > 0.
            */
            const double dt = 0.01;
            AstroTime t1 = time.AddDays(-dt/2);
            AstroTime t2 = time.AddDays(+dt/2);
            IllumInfo y1 = Astronomy.Illumination(body, t1);
            IllumInfo y2 = Astronomy.Illumination(body, t2);
            return (y2.mag - y1.mag) / dt;
        }
    }

    internal class SearchContext_NegElongSlope: SearchContext
    {
        private readonly Body body;

        public SearchContext_NegElongSlope(Body body)
        {
            this.body = body;
        }

        public override double Eval(AstroTime time)
        {
            const double dt = 0.1;
            AstroTime t1 = time.AddDays(-dt/2.0);
            AstroTime t2 = time.AddDays(+dt/2.0);

            double e1 = Astronomy.AngleFromSun(body, t1);
            double e2 = Astronomy.AngleFromSun(body, t2);
            return (e1 - e2)/dt;
        }
    }

    internal class SearchContext_SunOffset: SearchContext
    {
        private readonly double targetLon;

        public SearchContext_SunOffset(double targetLon)
        {
            this.targetLon = targetLon;
        }

        public override double Eval(AstroTime time)
        {
            Ecliptic ecl = Astronomy.SunPosition(time);
            return Astronomy.LongitudeOffset(ecl.elon - targetLon);
        }
    }

    internal class SearchContext_MoonOffset: SearchContext
    {
        private readonly double targetLon;

        public SearchContext_MoonOffset(double targetLon)
        {
            this.targetLon = targetLon;
        }

        public override double Eval(AstroTime time)
        {
            double angle = Astronomy.MoonPhase(time);
            return Astronomy.LongitudeOffset(angle - targetLon);
        }
    }

    internal class SearchContext_PeakAltitude: SearchContext
    {
        private readonly Body body;
        private readonly int direction;
        private readonly Observer observer;
        private readonly double body_radius_au;

        public SearchContext_PeakAltitude(Body body, Direction direction, Observer observer)
        {
            this.body = body;
            this.direction = (int)direction;
            this.observer = observer;

            switch (body)
            {
                case Body.Sun:
                    this.body_radius_au = Astronomy.SUN_RADIUS_AU;
                    break;

                case Body.Moon:
                    this.body_radius_au = Astronomy.MOON_RADIUS_AU;
                    break;

                default:
                    this.body_radius_au = 0.0;
                    break;
            }
        }

        public override double Eval(AstroTime time)
        {
            /*
                Return the angular altitude above or below the horizon
                of the highest part (the peak) of the given object.
                This is defined as the apparent altitude of the center of the body plus
                the body's angular radius.
                The 'direction' parameter controls whether the angle is measured
                positive above the horizon or positive below the horizon,
                depending on whether the caller wants rise times or set times, respectively.
            */

            Equatorial ofdate = Astronomy.Equator(body, time, observer, EquatorEpoch.OfDate, Aberration.Corrected);

            /* We calculate altitude without refraction, then add fixed refraction near the horizon. */
            /* This gives us the time of rise/set without the extra work. */
            Topocentric hor = Astronomy.Horizon(time, observer, ofdate.ra, ofdate.dec, Refraction.None);

            return direction * (hor.altitude + Astronomy.RAD2DEG*(body_radius_au / ofdate.dist) + Astronomy.REFRACTION_NEAR_HORIZON);
        }
    }

    internal class SearchContext_MoonDistanceSlope: SearchContext
    {
        private readonly int direction;

        public SearchContext_MoonDistanceSlope(int direction)
        {
            this.direction = direction;
        }

        public static double MoonDistance(AstroTime time)
        {
            var context = new MoonContext(time.tt / 36525.0);
            MoonResult moon = context.CalcMoon();
            return moon.distance_au;
        }

        public override double Eval(AstroTime time)
        {
            const double dt = 0.001;
            AstroTime t1 = time.AddDays(-dt/2.0);
            AstroTime t2 = time.AddDays(+dt/2.0);
            double dist1 = MoonDistance(t1);
            double dist2 = MoonDistance(t2);
            return direction * (dist2 - dist1)/dt;
        }
    }

    internal class PascalArray2<ElemType>
    {
        private readonly int xmin;
        private readonly int xmax;
        private readonly int ymin;
        private readonly int ymax;
        private readonly ElemType[,] array;

        public PascalArray2(int xmin, int xmax, int ymin, int ymax)
        {
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;
            this.array = new ElemType[(xmax - xmin) + 1, (ymax - ymin) + 1];
        }

        public ElemType this[int x, int y]
        {
            get { return array[x - xmin, y - ymin]; }
            set { array[x - xmin, y - ymin] = value; }
        }
    }

    internal class MoonContext
    {
        double T;
        double DGAM;
        double DLAM, N, GAM1C, SINPI;
        double L0, L, LS, F, D, S;
        double DL0, DL, DLS, DF, DD, DS;
        PascalArray2<double> CO = new PascalArray2<double>(-6, 6, 1, 4);
        PascalArray2<double> SI = new PascalArray2<double>(-6, 6, 1, 4);

        static double Frac(double x)
        {
            return x - Math.Floor(x);
        }

        static void AddThe(
            double c1, double s1, double c2, double s2,
            out double c, out double s)
        {
            c = c1*c2 - s1*s2;
            s = s1*c2 + c1*s2;
        }

        static double Sine(double phi)
        {
            /* sine, of phi in revolutions, not radians */
            return Math.Sin(2.0 * Math.PI * phi);
        }

        void LongPeriodic()
        {
            double S1 = Sine(0.19833+0.05611*T);
            double S2 = Sine(0.27869+0.04508*T);
            double S3 = Sine(0.16827-0.36903*T);
            double S4 = Sine(0.34734-5.37261*T);
            double S5 = Sine(0.10498-5.37899*T);
            double S6 = Sine(0.42681-0.41855*T);
            double S7 = Sine(0.14943-5.37511*T);

            DL0 = 0.84*S1+0.31*S2+14.27*S3+ 7.26*S4+ 0.28*S5+0.24*S6;
            DL  = 2.94*S1+0.31*S2+14.27*S3+ 9.34*S4+ 1.12*S5+0.83*S6;
            DLS =-6.40*S1                                   -1.89*S6;
            DF  = 0.21*S1+0.31*S2+14.27*S3-88.70*S4-15.30*S5+0.24*S6-1.86*S7;
            DD  = DL0-DLS;
            DGAM  = -3332E-9 * Sine(0.59734-5.37261*T)
                    -539E-9 * Sine(0.35498-5.37899*T)
                    -64E-9 * Sine(0.39943-5.37511*T);
        }

        private readonly int[] I = new int[4];

        void Term(int p, int q, int r, int s, out double x, out double y)
        {
            I[0] = p;
            I[1] = q;
            I[2] = r;
            I[3] = s;
            x = 1.0;
            y = 0.0;

            for (int k=1; k<=4; ++k)
                if (I[k-1] != 0.0)
                    AddThe(x, y, CO[I[k-1], k], SI[I[k-1], k], out x, out y);
        }

        void AddSol(
            double coeffl,
            double coeffs,
            double coeffg,
            double coeffp,
            int p,
            int q,
            int r,
            int s)
        {
            double x, y;
            Term(p, q, r, s, out x, out y);
            DLAM += coeffl*y;
            DS += coeffs*y;
            GAM1C += coeffg*x;
            SINPI += coeffp*x;
        }

        void ADDN(double coeffn, int p, int q, int r, int s, out double x, out double y)
        {
            Term(p, q, r, s, out x, out y);
            N += coeffn * y;
        }

        void SolarN()
        {
            double x, y;

            N = 0.0;
            ADDN(-526.069, 0, 0,1,-2, out x, out y);
            ADDN(  -3.352, 0, 0,1,-4, out x, out y);
            ADDN( +44.297,+1, 0,1,-2, out x, out y);
            ADDN(  -6.000,+1, 0,1,-4, out x, out y);
            ADDN( +20.599,-1, 0,1, 0, out x, out y);
            ADDN( -30.598,-1, 0,1,-2, out x, out y);
            ADDN( -24.649,-2, 0,1, 0, out x, out y);
            ADDN(  -2.000,-2, 0,1,-2, out x, out y);
            ADDN( -22.571, 0,+1,1,-2, out x, out y);
            ADDN( +10.985, 0,-1,1,-2, out x, out y);
        }

        void Planetary()
        {
            DLAM +=
                +0.82*Sine(0.7736  -62.5512*T)+0.31*Sine(0.0466 -125.1025*T)
                +0.35*Sine(0.5785  -25.1042*T)+0.66*Sine(0.4591+1335.8075*T)
                +0.64*Sine(0.3130  -91.5680*T)+1.14*Sine(0.1480+1331.2898*T)
                +0.21*Sine(0.5918+1056.5859*T)+0.44*Sine(0.5784+1322.8595*T)
                +0.24*Sine(0.2275   -5.7374*T)+0.28*Sine(0.2965   +2.6929*T)
                +0.33*Sine(0.3132   +6.3368*T);
        }

        internal MoonContext(double centuries_since_j2000)
        {
            int I, J, MAX;
            double T2, ARG, FAC;
            double c, s;

            T = centuries_since_j2000;
            T2 = T*T;
            DLAM = 0;
            DS = 0;
            GAM1C = 0;
            SINPI = 3422.7000;
            LongPeriodic();
            L0 = Astronomy.PI2*Frac(0.60643382+1336.85522467*T-0.00000313*T2) + DL0/Astronomy.ARC;
            L  = Astronomy.PI2*Frac(0.37489701+1325.55240982*T+0.00002565*T2) + DL /Astronomy.ARC;
            LS = Astronomy.PI2*Frac(0.99312619+  99.99735956*T-0.00000044*T2) + DLS/Astronomy.ARC;
            F  = Astronomy.PI2*Frac(0.25909118+1342.22782980*T-0.00000892*T2) + DF /Astronomy.ARC;
            D  = Astronomy.PI2*Frac(0.82736186+1236.85308708*T-0.00000397*T2) + DD /Astronomy.ARC;
            for (I=1; I<=4; ++I)
            {
                switch(I)
                {
                    case 1:  ARG=L;  MAX=4; FAC=1.000002208;               break;
                    case 2:  ARG=LS; MAX=3; FAC=0.997504612-0.002495388*T; break;
                    case 3:  ARG=F;  MAX=4; FAC=1.000002708+139.978*DGAM;  break;
                    default: ARG=D;  MAX=6; FAC=1.0;                       break;
                }
                CO[0,I] = 1.0;
                CO[1,I] = Math.Cos(ARG)*FAC;
                SI[0,I] = 0.0;
                SI[1,I] = Math.Sin(ARG)*FAC;
                for (J=2; J<=MAX; ++J)
                {
                    AddThe(CO[J-1,I], SI[J-1,I], CO[1,I], SI[1,I], out c, out s);
                    CO[J,I] = c;
                    SI[J,I] = s;
                }

                for (J=1; J<=MAX; ++J)
                {
                    CO[-J,I] =  CO[J,I];
                    SI[-J,I] = -SI[J,I];
                }
            }
        }

        internal MoonResult CalcMoon()
        {

            AddSol(    13.9020,    14.0600,    -0.0010,     0.2607, 0, 0, 0, 4);
            AddSol(     0.4030,    -4.0100,     0.3940,     0.0023, 0, 0, 0, 3);
            AddSol(  2369.9120,  2373.3600,     0.6010,    28.2333, 0, 0, 0, 2);
            AddSol(  -125.1540,  -112.7900,    -0.7250,    -0.9781, 0, 0, 0, 1);
            AddSol(     1.9790,     6.9800,    -0.4450,     0.0433, 1, 0, 0, 4);
            AddSol(   191.9530,   192.7200,     0.0290,     3.0861, 1, 0, 0, 2);
            AddSol(    -8.4660,   -13.5100,     0.4550,    -0.1093, 1, 0, 0, 1);
            AddSol( 22639.5000, 22609.0700,     0.0790,   186.5398, 1, 0, 0, 0);
            AddSol(    18.6090,     3.5900,    -0.0940,     0.0118, 1, 0, 0,-1);
            AddSol( -4586.4650, -4578.1300,    -0.0770,    34.3117, 1, 0, 0,-2);
            AddSol(     3.2150,     5.4400,     0.1920,    -0.0386, 1, 0, 0,-3);
            AddSol(   -38.4280,   -38.6400,     0.0010,     0.6008, 1, 0, 0,-4);
            AddSol(    -0.3930,    -1.4300,    -0.0920,     0.0086, 1, 0, 0,-6);
            AddSol(    -0.2890,    -1.5900,     0.1230,    -0.0053, 0, 1, 0, 4);
            AddSol(   -24.4200,   -25.1000,     0.0400,    -0.3000, 0, 1, 0, 2);
            AddSol(    18.0230,    17.9300,     0.0070,     0.1494, 0, 1, 0, 1);
            AddSol(  -668.1460,  -126.9800,    -1.3020,    -0.3997, 0, 1, 0, 0);
            AddSol(     0.5600,     0.3200,    -0.0010,    -0.0037, 0, 1, 0,-1);
            AddSol(  -165.1450,  -165.0600,     0.0540,     1.9178, 0, 1, 0,-2);
            AddSol(    -1.8770,    -6.4600,    -0.4160,     0.0339, 0, 1, 0,-4);
            AddSol(     0.2130,     1.0200,    -0.0740,     0.0054, 2, 0, 0, 4);
            AddSol(    14.3870,    14.7800,    -0.0170,     0.2833, 2, 0, 0, 2);
            AddSol(    -0.5860,    -1.2000,     0.0540,    -0.0100, 2, 0, 0, 1);
            AddSol(   769.0160,   767.9600,     0.1070,    10.1657, 2, 0, 0, 0);
            AddSol(     1.7500,     2.0100,    -0.0180,     0.0155, 2, 0, 0,-1);
            AddSol(  -211.6560,  -152.5300,     5.6790,    -0.3039, 2, 0, 0,-2);
            AddSol(     1.2250,     0.9100,    -0.0300,    -0.0088, 2, 0, 0,-3);
            AddSol(   -30.7730,   -34.0700,    -0.3080,     0.3722, 2, 0, 0,-4);
            AddSol(    -0.5700,    -1.4000,    -0.0740,     0.0109, 2, 0, 0,-6);
            AddSol(    -2.9210,   -11.7500,     0.7870,    -0.0484, 1, 1, 0, 2);
            AddSol(     1.2670,     1.5200,    -0.0220,     0.0164, 1, 1, 0, 1);
            AddSol(  -109.6730,  -115.1800,     0.4610,    -0.9490, 1, 1, 0, 0);
            AddSol(  -205.9620,  -182.3600,     2.0560,     1.4437, 1, 1, 0,-2);
            AddSol(     0.2330,     0.3600,     0.0120,    -0.0025, 1, 1, 0,-3);
            AddSol(    -4.3910,    -9.6600,    -0.4710,     0.0673, 1, 1, 0,-4);
            AddSol(     0.2830,     1.5300,    -0.1110,     0.0060, 1,-1, 0, 4);
            AddSol(    14.5770,    31.7000,    -1.5400,     0.2302, 1,-1, 0, 2);
            AddSol(   147.6870,   138.7600,     0.6790,     1.1528, 1,-1, 0, 0);
            AddSol(    -1.0890,     0.5500,     0.0210,     0.0000, 1,-1, 0,-1);
            AddSol(    28.4750,    23.5900,    -0.4430,    -0.2257, 1,-1, 0,-2);
            AddSol(    -0.2760,    -0.3800,    -0.0060,    -0.0036, 1,-1, 0,-3);
            AddSol(     0.6360,     2.2700,     0.1460,    -0.0102, 1,-1, 0,-4);
            AddSol(    -0.1890,    -1.6800,     0.1310,    -0.0028, 0, 2, 0, 2);
            AddSol(    -7.4860,    -0.6600,    -0.0370,    -0.0086, 0, 2, 0, 0);
            AddSol(    -8.0960,   -16.3500,    -0.7400,     0.0918, 0, 2, 0,-2);
            AddSol(    -5.7410,    -0.0400,     0.0000,    -0.0009, 0, 0, 2, 2);
            AddSol(     0.2550,     0.0000,     0.0000,     0.0000, 0, 0, 2, 1);
            AddSol(  -411.6080,    -0.2000,     0.0000,    -0.0124, 0, 0, 2, 0);
            AddSol(     0.5840,     0.8400,     0.0000,     0.0071, 0, 0, 2,-1);
            AddSol(   -55.1730,   -52.1400,     0.0000,    -0.1052, 0, 0, 2,-2);
            AddSol(     0.2540,     0.2500,     0.0000,    -0.0017, 0, 0, 2,-3);
            AddSol(     0.0250,    -1.6700,     0.0000,     0.0031, 0, 0, 2,-4);
            AddSol(     1.0600,     2.9600,    -0.1660,     0.0243, 3, 0, 0, 2);
            AddSol(    36.1240,    50.6400,    -1.3000,     0.6215, 3, 0, 0, 0);
            AddSol(   -13.1930,   -16.4000,     0.2580,    -0.1187, 3, 0, 0,-2);
            AddSol(    -1.1870,    -0.7400,     0.0420,     0.0074, 3, 0, 0,-4);
            AddSol(    -0.2930,    -0.3100,    -0.0020,     0.0046, 3, 0, 0,-6);
            AddSol(    -0.2900,    -1.4500,     0.1160,    -0.0051, 2, 1, 0, 2);
            AddSol(    -7.6490,   -10.5600,     0.2590,    -0.1038, 2, 1, 0, 0);
            AddSol(    -8.6270,    -7.5900,     0.0780,    -0.0192, 2, 1, 0,-2);
            AddSol(    -2.7400,    -2.5400,     0.0220,     0.0324, 2, 1, 0,-4);
            AddSol(     1.1810,     3.3200,    -0.2120,     0.0213, 2,-1, 0, 2);
            AddSol(     9.7030,    11.6700,    -0.1510,     0.1268, 2,-1, 0, 0);
            AddSol(    -0.3520,    -0.3700,     0.0010,    -0.0028, 2,-1, 0,-1);
            AddSol(    -2.4940,    -1.1700,    -0.0030,    -0.0017, 2,-1, 0,-2);
            AddSol(     0.3600,     0.2000,    -0.0120,    -0.0043, 2,-1, 0,-4);
            AddSol(    -1.1670,    -1.2500,     0.0080,    -0.0106, 1, 2, 0, 0);
            AddSol(    -7.4120,    -6.1200,     0.1170,     0.0484, 1, 2, 0,-2);
            AddSol(    -0.3110,    -0.6500,    -0.0320,     0.0044, 1, 2, 0,-4);
            AddSol(     0.7570,     1.8200,    -0.1050,     0.0112, 1,-2, 0, 2);
            AddSol(     2.5800,     2.3200,     0.0270,     0.0196, 1,-2, 0, 0);
            AddSol(     2.5330,     2.4000,    -0.0140,    -0.0212, 1,-2, 0,-2);
            AddSol(    -0.3440,    -0.5700,    -0.0250,     0.0036, 0, 3, 0,-2);
            AddSol(    -0.9920,    -0.0200,     0.0000,     0.0000, 1, 0, 2, 2);
            AddSol(   -45.0990,    -0.0200,     0.0000,    -0.0010, 1, 0, 2, 0);
            AddSol(    -0.1790,    -9.5200,     0.0000,    -0.0833, 1, 0, 2,-2);
            AddSol(    -0.3010,    -0.3300,     0.0000,     0.0014, 1, 0, 2,-4);
            AddSol(    -6.3820,    -3.3700,     0.0000,    -0.0481, 1, 0,-2, 2);
            AddSol(    39.5280,    85.1300,     0.0000,    -0.7136, 1, 0,-2, 0);
            AddSol(     9.3660,     0.7100,     0.0000,    -0.0112, 1, 0,-2,-2);
            AddSol(     0.2020,     0.0200,     0.0000,     0.0000, 1, 0,-2,-4);
            AddSol(     0.4150,     0.1000,     0.0000,     0.0013, 0, 1, 2, 0);
            AddSol(    -2.1520,    -2.2600,     0.0000,    -0.0066, 0, 1, 2,-2);
            AddSol(    -1.4400,    -1.3000,     0.0000,     0.0014, 0, 1,-2, 2);
            AddSol(     0.3840,    -0.0400,     0.0000,     0.0000, 0, 1,-2,-2);
            AddSol(     1.9380,     3.6000,    -0.1450,     0.0401, 4, 0, 0, 0);
            AddSol(    -0.9520,    -1.5800,     0.0520,    -0.0130, 4, 0, 0,-2);
            AddSol(    -0.5510,    -0.9400,     0.0320,    -0.0097, 3, 1, 0, 0);
            AddSol(    -0.4820,    -0.5700,     0.0050,    -0.0045, 3, 1, 0,-2);
            AddSol(     0.6810,     0.9600,    -0.0260,     0.0115, 3,-1, 0, 0);
            AddSol(    -0.2970,    -0.2700,     0.0020,    -0.0009, 2, 2, 0,-2);
            AddSol(     0.2540,     0.2100,    -0.0030,     0.0000, 2,-2, 0,-2);
            AddSol(    -0.2500,    -0.2200,     0.0040,     0.0014, 1, 3, 0,-2);
            AddSol(    -3.9960,     0.0000,     0.0000,     0.0004, 2, 0, 2, 0);
            AddSol(     0.5570,    -0.7500,     0.0000,    -0.0090, 2, 0, 2,-2);
            AddSol(    -0.4590,    -0.3800,     0.0000,    -0.0053, 2, 0,-2, 2);
            AddSol(    -1.2980,     0.7400,     0.0000,     0.0004, 2, 0,-2, 0);
            AddSol(     0.5380,     1.1400,     0.0000,    -0.0141, 2, 0,-2,-2);
            AddSol(     0.2630,     0.0200,     0.0000,     0.0000, 1, 1, 2, 0);
            AddSol(     0.4260,     0.0700,     0.0000,    -0.0006, 1, 1,-2,-2);
            AddSol(    -0.3040,     0.0300,     0.0000,     0.0003, 1,-1, 2, 0);
            AddSol(    -0.3720,    -0.1900,     0.0000,    -0.0027, 1,-1,-2, 2);
            AddSol(     0.4180,     0.0000,     0.0000,     0.0000, 0, 0, 4, 0);
            AddSol(    -0.3300,    -0.0400,     0.0000,     0.0000, 3, 0, 2, 0);

            SolarN();
            Planetary();
            S = F + DS/Astronomy.ARC;

            double lat_seconds = (1.000002708 + 139.978*DGAM)*(18518.511+1.189+GAM1C)*Math.Sin(S)-6.24*Math.Sin(3*S) + N;

            return new MoonResult(
                Astronomy.PI2 * Frac((L0+DLAM/Astronomy.ARC) / Astronomy.PI2),
                lat_seconds * (Astronomy.DEG2RAD / 3600.0),
                (Astronomy.ARC * (Astronomy.ERAD / Astronomy.AU)) / (0.999953253 * SINPI)
            );
        }
    }

    internal struct MoonResult
    {
        public readonly double geo_eclip_lon;
        public readonly double geo_eclip_lat;
        public readonly double distance_au;

        public MoonResult(double lon, double lat, double dist)
        {
            this.geo_eclip_lon = lon;
            this.geo_eclip_lat = lat;
            this.distance_au = dist;
        }
    }
}