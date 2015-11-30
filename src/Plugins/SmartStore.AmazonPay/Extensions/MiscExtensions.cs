using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;

namespace SmartStore.AmazonPay.Extensions
{
	public static class MiscExtensions
	{
		public static void ToFirstAndLastName(this string name, out string firstName, out string lastName)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				int index = name.LastIndexOf(' ');
				if (index == -1)
				{
					firstName = "";
					lastName = name;
				}
				else
				{
					firstName = name.Substring(0, index);
					lastName = name.Substring(index + 1);
				}

				firstName = firstName.EmptyNull().Truncate(4000);
				lastName = lastName.EmptyNull().Truncate(4000);
			}
			else
			{
				firstName = lastName = "";
			}
		}

		public static void ToFirstAndLastName(this Address address, string name)
		{
			string firstName, lastName;
			name.ToFirstAndLastName(out firstName, out lastName);

			address.FirstName = firstName;
			address.LastName = lastName;
		}

		public static bool HasAmazonPayState(this HttpContextBase httpContext)
		{
			var checkoutState = httpContext.GetCheckoutState();

			if (checkoutState != null && checkoutState.CustomProperties.ContainsKey(AmazonPayCore.AmazonPayCheckoutStateKey))
			{
				var state = checkoutState.CustomProperties[AmazonPayCore.AmazonPayCheckoutStateKey] as AmazonPayCheckoutState;

				return (state != null && state.OrderReferenceId.HasValue());
			}
			return false;
		}

		public static AmazonPayCheckoutState GetAmazonPayState(this HttpContextBase httpContext, ILocalizationService localizationService)
		{
			AmazonPayCheckoutState state = null;
			var checkoutState = httpContext.GetCheckoutState();
			
			if (checkoutState == null || (state = (AmazonPayCheckoutState)checkoutState.CustomProperties[AmazonPayCore.AmazonPayCheckoutStateKey]) == null)
				throw new SmartException(localizationService.GetResource("Plugins.Payments.AmazonPay.MissingCheckoutSessionState"));

			return state;
		}

		public static Address FindAddress(this List<Address> addresses, Address address, bool uncompleteToo)
		{
			var match = addresses.FindAddress(address.FirstName, address.LastName,
				address.PhoneNumber, address.Email, address.FaxNumber, address.Company,
				address.Address1, address.Address2,
				address.City, address.StateProvinceId, address.ZipPostalCode, address.CountryId);

			if (match == null && uncompleteToo)
			{
				// compare with AmazonPayApiExtensions.ToAddress

				match = addresses.FirstOrDefault(x =>
					x.FirstName == null && x.LastName == null &&
					x.Address1 == null && x.Address2 == null &&
					x.City == address.City && x.ZipPostalCode == address.ZipPostalCode &&
					x.PhoneNumber == null &&
					x.CountryId == address.CountryId && x.StateProvinceId == address.StateProvinceId
				);
			}

			return match;
		}
	}
}
