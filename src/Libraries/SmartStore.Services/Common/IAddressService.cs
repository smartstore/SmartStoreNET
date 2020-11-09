using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IAddressService
    {
        /// <summary>
        /// Deletes an address
        /// </summary>
        /// <param name="address">Address</param>
        void DeleteAddress(Address address);

        void DeleteAddress(int id);

        /// <summary>
        /// Gets total number of addresses by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of addresses</returns>
        int GetAddressTotalByCountryId(int countryId);

        /// <summary>
        /// Gets total number of addresses by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <returns>Number of addresses</returns>
        int GetAddressTotalByStateProvinceId(int stateProvinceId);

        /// <summary>
        /// Gets an address by address identifier
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <returns>Address</returns>
        Address GetAddressById(int addressId);

        /// <summary>
        /// Gets addresses by address identifiers
        /// </summary>
        /// <param name="addressIds">Address identifiers</param>
        /// <returns>Addresses</returns>
        IList<Address> GetAddressByIds(int[] addressIds);

        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        void InsertAddress(Address address);

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        void UpdateAddress(Address address);

        /// <summary>
        /// Gets a value indicating whether address is valid (can be saved)
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>Result</returns>
        bool IsAddressValid(Address address);

        /// <summary>
        /// Formats the address according to the countries address formatting template
        /// </summary>
        /// <param name="settings">Address to format</param>
        /// <param name="newLineToBr">Whether new lines should be replaced with html BR tags</param>
        /// <returns>The formatted address</returns>
        string FormatAddress(CompanyInformationSettings settings, bool newLineToBr = false);

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
        /// <param name="template">The (liquid) formatting template. If <c>null</c>, the system global template will be used.</param>
        /// <returns>The formatted address</returns>
        string FormatAddress(object address, string template = null, IFormatProvider formatProvider = null);
    }
}