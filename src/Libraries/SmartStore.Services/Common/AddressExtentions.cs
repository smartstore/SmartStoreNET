using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Services.Common
{
    public static class AddressExtentions
    {
		/// <summary>
		/// Find first occurrence of an address
		/// </summary>
		/// <param name="source">Addresses to be searched</param>
		/// <param name="address">Address to find</param>
		/// <returns>First occurrence of address</returns>
		public static Address FindAddress(this ICollection<Address> source, Address address)
		{
			return source.FindAddress(
				address.FirstName,
				address.LastName,
				address.PhoneNumber,
				address.Email,
				address.FaxNumber,
				address.Company,
				address.Address1,
				address.Address2,
				address.City,
				address.StateProvinceId,
				address.ZipPostalCode,
				address.CountryId
			);
		}

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
		public static Address FindAddress(
			this ICollection<Address> source,
            string firstName, 
			string lastName, 
			string phoneNumber,
            string email, 
			string faxNumber, 
			string company, 
			string address1,
            string address2, 
			string city, 
			int? stateProvinceId,
            string zipPostalCode, 
			int? countryId)
        {
			Func<Address, bool> addressMatcher = (x) => 
			{
				return x.Email.IsCaseInsensitiveEqual(email)
					&& x.LastName.IsCaseInsensitiveEqual(lastName)
					&& x.FirstName.IsCaseInsensitiveEqual(firstName)
					&& x.Address1.IsCaseInsensitiveEqual(address1)
					&& x.Address2.IsCaseInsensitiveEqual(address2)
					&& x.Company.IsCaseInsensitiveEqual(company)
					&& x.ZipPostalCode.IsCaseInsensitiveEqual(zipPostalCode)
					&& x.City.IsCaseInsensitiveEqual(city)
					&& x.PhoneNumber.IsCaseInsensitiveEqual(phoneNumber)
					&& x.FaxNumber.IsCaseInsensitiveEqual(faxNumber)
					&& x.StateProvinceId == stateProvinceId
					&& x.CountryId == countryId;
			};

			return source.FirstOrDefault(addressMatcher);
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
