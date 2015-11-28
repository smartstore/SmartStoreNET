using System;
using System.Collections.Generic;
using System.Text;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Services.Common
{
    public static class AddressExtentions
    {
        /// <summary>
        /// Find an address
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="email">Email</param>
        /// <param name="faxNumber">Fax number</param>
        /// <param name="company">Company</param>
        /// <param name="address1">Address 1</param>
        /// <param name="address2">Address 2</param>
        /// <param name="city">City</param>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <param name="zipPostalCode">Zip postal code</param>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Address</returns>
        public static Address FindAddress(this List<Address> source,
            string firstName, string lastName, string phoneNumber,
            string email, string faxNumber, string company, string address1,
            string address2, string city, int? stateProvinceId,
            string zipPostalCode, int? countryId)
        {
            return source.Find((a) => ((String.IsNullOrEmpty(a.FirstName) && String.IsNullOrEmpty(firstName)) || a.FirstName == firstName) &&
                ((String.IsNullOrEmpty(a.LastName) && String.IsNullOrEmpty(lastName)) || a.LastName == lastName) &&
                ((String.IsNullOrEmpty(a.PhoneNumber) && String.IsNullOrEmpty(phoneNumber)) || a.PhoneNumber == phoneNumber) &&
                ((String.IsNullOrEmpty(a.Email) && String.IsNullOrEmpty(email)) || a.Email == email) &&
                ((String.IsNullOrEmpty(a.FaxNumber) && String.IsNullOrEmpty(faxNumber)) || a.FaxNumber == faxNumber) &&
                ((String.IsNullOrEmpty(a.Company) && String.IsNullOrEmpty(company)) || a.Company == company) &&
                ((String.IsNullOrEmpty(a.Address1) && String.IsNullOrEmpty(address1)) || a.Address1 == address1) &&
                ((String.IsNullOrEmpty(a.Address2) && String.IsNullOrEmpty(address2)) || a.Address2 == address2) &&
                ((String.IsNullOrEmpty(a.City) && String.IsNullOrEmpty(city)) || a.City == city) &&
                ((a.StateProvinceId.IsNullOrDefault() && stateProvinceId.IsNullOrDefault()) || a.StateProvinceId == stateProvinceId) &&
                ((String.IsNullOrEmpty(a.ZipPostalCode) && String.IsNullOrEmpty(zipPostalCode)) || a.ZipPostalCode == zipPostalCode) &&
                ((a.CountryId.IsNullOrDefault() && countryId.IsNullOrDefault()) || a.CountryId == countryId));
        }

		/// <summary>Returns the full name of the address.</summary>
		public static string GetFullName(this Address address)
		{
			if (address != null)
			{
				var sb = new StringBuilder(address.FirstName);

				sb.Grow(address.LastName, " ");		

				if (address.Company.HasValue())
				{
					sb.Grow("({0})".FormatWith(address.Company), " ");
				}
				return sb.ToString();
			}
			return null;
		}

		/// <summary>
		/// Checks whether the postal data of two addresses are equal.
		/// </summary>
		public static bool IsPostalDataEqual(this Address address, Address other)
		{
			if (address != null && other != null)
			{
				if (address.FirstName.IsCaseInsensitiveEqual(other.FirstName) && 
					address.LastName.IsCaseInsensitiveEqual(other.LastName) && 
					address.Company.IsCaseInsensitiveEqual(other.Company) &&
					address.Address1.IsCaseInsensitiveEqual(other.Address1) && 
					address.Address2.IsCaseInsensitiveEqual(other.Address2) &&
					address.ZipPostalCode.IsCaseInsensitiveEqual(other.ZipPostalCode) && 
					address.City.IsCaseInsensitiveEqual(other.City) && 
					address.StateProvinceId == other.StateProvinceId && 
					address.CountryId == other.CountryId)
				{
					return true;
				}
			}

			return false;
		}
    }
}
