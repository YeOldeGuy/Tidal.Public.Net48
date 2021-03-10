using System;
using MaxMind.GeoIP2.Responses;

namespace Tidal.Models
{
    /// <summary>
    /// A distilled version of the MaxMind CityResponse data with
    /// only what we need.
    /// </summary>
    public class GeoLocation
    {
        private GeoLocation()
        {
            IsCityValid = false;
            IsCountryValid = false;
            IsSubDivisionValid = false;
            IsIsoValid = false;
        }

        /// <summary>
        ///   Factory method to create a new <see cref="GeoLocation"/> from a
        ///   string input. The string should be formatted like, <c>"Madison, WI,
        ///   USA"</c>, or <c>"Brussels, Belgium"</c>
        /// </summary>
        /// <param name="location">
        ///   A formatted location string like <c>"Honolulu, HI, USA"</c>
        /// </param>
        /// <returns>
        ///   A new <see cref="GeoLocation"/> populated with info from the
        ///   <paramref name="location"/> value.
        /// </returns>
        public static GeoLocation FromString(string location)
        {
            string[] tokens = location.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var res = new GeoLocation();
            if (tokens.Length <= 0)
                return res;

            if (tokens.Length == 1)
            {
                res.Country = tokens[0];
            }
            else if (tokens.Length == 2)
            {
                res.City = tokens[0];
                res.Country = tokens[1];
            }
            else
            {
                res.City = tokens[0];
                res.SubDivision = tokens[1];
                res.Country = tokens[2];
                if (res.Country.StartsWith("Canada"))
                    res.CountryCode = "CA";
                else if (res.Country.StartsWith("United States"))
                    res.CountryCode = "US";
            }
            return res;
        }

        /// <summary>
        /// Factory method to create a new <see cref="GeoLocation"/>, with an
        /// imaginary place in Kansas, childhood home of a super man. Suitable
        /// for when a location can't be found.
        /// </summary>
        /// <returns>A new <see cref="GeoLocation"/>.</returns>
        public static GeoLocation FromNotFoundResponse(string attemptedIpAddress)
        {
            var geo = new GeoLocation
            {
                City = attemptedIpAddress,
                Country = "Unknown Location",
                CountryCode = "XX",
                SubDivision = null,
                IsCityValid = true,
                IsCountryValid = true,
                IsSubDivisionValid = true,
                Latitude = 39.05,
                Longitude = -95.697,
            };

            return geo;
        }

        /// <summary>
        /// Factory method to create a <see cref="GeoLocation"/> from the
        /// <see cref="CityResponse"/>.
        /// </summary>
        /// <param name="location">A GeoIp2 <see cref="CityResponse"/> instance.</param>
        /// <returns>A  <see cref="GeoLocation"/> instance or <see langword="null"/>.</returns>
        public static GeoLocation FromMaxMindCityResponse(CityResponse location)
        {
            if (location == null)
            {
                return null;
            }

            var geo = new GeoLocation();

            if (!string.IsNullOrEmpty(location.Country?.Name))
            {
                geo.IsCountryValid = true;
                geo.Country = location.Country.Name;
                if (!string.IsNullOrEmpty(location.Country.IsoCode))
                {
                    geo.IsIsoValid = true;
                    geo.CountryCode = location.Country.IsoCode;
                }
            }
            if (!string.IsNullOrEmpty(location.MostSpecificSubdivision?.Name))
            {
                geo.IsSubDivisionValid = true;
                geo.SubDivision = location.MostSpecificSubdivision.IsoCode;
            }
            if (!string.IsNullOrEmpty(location.City?.Name))
            {
                geo.IsCityValid = true;
                geo.City = location.City.Name;
            }

            if (location.Location.HasCoordinates)
            {
                geo.Latitude = location.Location.Latitude;
                geo.Longitude = location.Location.Longitude;
            }
            return geo;
        }

        public static GeoLocation GetBlankLocation()
        {
            var loc = new GeoLocation();
            return loc;
        }

        public bool IsCityValid { get; private set; }
        public bool IsCountryValid { get; private set; }
        public bool IsSubDivisionValid { get; private set; }
        public bool IsIsoValid { get; private set; }

        /// <summary>
        /// The city associated with the IP address, possibly empty but not
        /// null.
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// If the city is in the US or Canada, the state or province of the
        /// location.
        /// </summary>
        public string SubDivision { get; private set; }

        /// <summary>
        /// The English short-name of the country associated with the IP address.
        /// Shouldn't be empty.
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// The ISO-assigned country code, such as "AU" for Australia, or "VE"
        /// for Venezuela. Should never be empty.
        /// </summary>
        public string CountryCode { get; private set; }

        public double? Latitude { get; private set; }

        public double? Longitude { get; private set; }
    }
}
