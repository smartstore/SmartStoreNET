using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Country service interface
    /// </summary>
    public partial interface ICountryService
    {
        /// <summary>
        /// Deletes a country
        /// </summary>
        /// <param name="country">Country</param>
        void DeleteCountry(Country country);

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        IList<Country> GetAllCountries(bool showHidden = false);

        /// <summary>
        /// Gets all countries that allow billing
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        IList<Country> GetAllCountriesForBilling(bool showHidden = false);

        /// <summary>
        /// Gets all countries that allow shipping
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Country collection</returns>
        IList<Country> GetAllCountriesForShipping(bool showHidden = false);

        /// <summary>
        /// Gets a country 
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Country</returns>
        Country GetCountryById(int countryId);

		/// <summary>
		/// Gets a country by two or three letter ISO code
		/// </summary>
		/// <param name="letterIsoCode">Country two or three letter ISO code</param>
		/// <returns>Country</returns>
		Country GetCountryByTwoOrThreeLetterIsoCode(string letterIsoCode);

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        /// <param name="twoLetterIsoCode">Country two letter ISO code</param>
        /// <returns>Country</returns>
        Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode);

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        /// <param name="threeLetterIsoCode">Country three letter ISO code</param>
        /// <returns>Country</returns>
        Country GetCountryByThreeLetterIsoCode(string threeLetterIsoCode);

        /// <summary>
        /// Inserts a country
        /// </summary>
        /// <param name="country">Country</param>
        void InsertCountry(Country country);

        /// <summary>
        /// Updates the country
        /// </summary>
        /// <param name="country">Country</param>
        void UpdateCountry(Country country);

		/// <summary>
		/// Formats the address according to the countries address formatting template
		/// </summary>
		/// <param name="address">Address to format</param>
		/// <param name="newLineToBr">Whether new lines should be replaced with html BR tags</param>
		/// <returns>The formatted address</returns>
		string FormatAddress(Address address, bool newLineToBr = false);

		/// <summary>
		/// Formats the address according to the countries address formatting template
		/// </summary>
		/// <param name="address">Address to format. Usually passed by the template engine as a dictionary</param>
		/// <param name="country">The country to get the formatting template from. If <c>null</c>, the system global template will be used.</param>
		/// <returns>The formatted address</returns>
		string FormatAddress(object address, Country country = null, IFormatProvider formatProvider = null);
	}
}