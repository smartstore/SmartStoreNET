using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using OffAmazonPaymentsService;
using OffAmazonPaymentsService.Model;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.AmazonPay
{
	internal static class AmazonPayExtensions
	{
		internal static void ToFirstAndLastName(this string name, out string firstName, out string lastName)
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

		internal static void ToFirstAndLastName(this SmartStore.Core.Domain.Common.Address address, string name)
		{
			string firstName, lastName;
			name.ToFirstAndLastName(out firstName, out lastName);

			address.FirstName = firstName;
			address.LastName = lastName;
		}

		internal static SmartStore.Core.Domain.Common.Address FindAddress(
			this List<SmartStore.Core.Domain.Common.Address> addresses,
			SmartStore.Core.Domain.Common.Address address,
			bool uncompleteToo)
		{
			var match = addresses.FindAddress(address.FirstName, address.LastName,
				address.PhoneNumber, address.Email, address.FaxNumber, address.Company,
				address.Address1, address.Address2,
				address.City, address.StateProvinceId, address.ZipPostalCode, address.CountryId);

			if (match == null && uncompleteToo)
			{
				// Compare with ToAddress
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

		internal static bool HasAmazonPayState(this HttpContextBase httpContext)
		{
			var checkoutState = httpContext.GetCheckoutState();
			var checkoutStateKey = AmazonPayPlugin.SystemName + ".CheckoutState";

			if (checkoutState != null && checkoutState.CustomProperties.ContainsKey(checkoutStateKey))
			{
				var state = checkoutState.CustomProperties[checkoutStateKey] as AmazonPayCheckoutState;

				return (state != null && state.OrderReferenceId.HasValue());
			}
			return false;
		}

		internal static AmazonPayCheckoutState GetAmazonPayState(this HttpContextBase httpContext, ILocalizationService localizationService)
		{
			var checkoutState = httpContext.GetCheckoutState();

			if (checkoutState == null)
				throw new SmartException(localizationService.GetResource("Plugins.Payments.AmazonPay.MissingCheckoutSessionState"));

			var state = checkoutState.CustomProperties.Get(AmazonPayPlugin.SystemName + ".CheckoutState") as AmazonPayCheckoutState;

			if (state == null)
				throw new SmartException(localizationService.GetResource("Plugins.Payments.AmazonPay.MissingCheckoutSessionState"));

			return state;
		}


		internal static bool GetErrorStrings(this OffAmazonPaymentsServiceException exception, out string shortMessage, out string fullMessage)
		{
			shortMessage = fullMessage = null;

			try
			{
				if (exception.Message.HasValue())
				{
					shortMessage = exception.Message;
					var sb = new StringBuilder();

					sb.AppendLine("Caught Exception: " + exception.Message);
					sb.AppendLine("Response Status Code: " + exception.StatusCode);
					sb.AppendLine("Error Code: " + exception.ErrorCode);
					sb.AppendLine("Error Type: " + exception.ErrorType);
					sb.AppendLine("Request ID: " + exception.RequestId);
					sb.AppendLine("XML: " + exception.XML);

					if (exception.ResponseHeaderMetadata != null)
						sb.AppendLine("ResponseHeaderMetadata: " + exception.ResponseHeaderMetadata.ToString());

					fullMessage = sb.ToString();
				}
			}
			catch (Exception) { }

			return shortMessage.HasValue();
		}

		internal static string ToFormatedAddress(this Address amazonAddress, ICountryService countryService, IStateProvinceService stateProvinceService)
		{
			var sb = new StringBuilder();

			try
			{
				var city = (amazonAddress.IsSetCity() ? amazonAddress.City : null);
				var zip = (amazonAddress.IsSetPostalCode() ? amazonAddress.PostalCode : null);

				sb.AppendLine("");

				if (amazonAddress.Name.HasValue())
					sb.AppendLine(amazonAddress.Name);

				if (amazonAddress.AddressLine1.HasValue())
					sb.AppendLine(amazonAddress.AddressLine1);

				if (amazonAddress.AddressLine2.HasValue())
					sb.AppendLine(amazonAddress.AddressLine2);

				if (amazonAddress.AddressLine3.HasValue())
					sb.AppendLine(amazonAddress.AddressLine3);

				sb.AppendLine(zip.Grow(city, " "));

				if (amazonAddress.IsSetStateOrRegion())
				{
					var stateProvince = stateProvinceService.GetStateProvinceByAbbreviation(amazonAddress.StateOrRegion);

					if (stateProvince == null)
						sb.AppendLine(amazonAddress.StateOrRegion);
					else
						sb.AppendLine("{0} {1}".FormatWith(amazonAddress.StateOrRegion, stateProvince.GetLocalized(x => x.Name)));
				}

				if (amazonAddress.IsSetCountryCode())
				{
					var country = countryService.GetCountryByTwoOrThreeLetterIsoCode(amazonAddress.CountryCode);

					if (country == null)
						sb.AppendLine(amazonAddress.CountryCode);
					else
						sb.AppendLine("{0} {1}".FormatWith(amazonAddress.CountryCode, country.GetLocalized(x => x.Name)));
				}

				if (amazonAddress.Phone.HasValue())
				{
					sb.AppendLine(amazonAddress.Phone);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return sb.ToString();
		}

		internal static void ToAddress(this Address amazonAddress, SmartStore.Core.Domain.Common.Address address, ICountryService countryService,
			IStateProvinceService stateProvinceService, out bool countryAllowsShipping, out bool countryAllowsBilling)
		{
			countryAllowsShipping = countryAllowsBilling = true;

			if (amazonAddress.IsSetName())
			{
				address.ToFirstAndLastName(amazonAddress.Name);
			}

			if (amazonAddress.IsSetAddressLine1())
			{
				address.Address1 = amazonAddress.AddressLine1.TrimSafe().Truncate(4000);
			}

			if (amazonAddress.IsSetAddressLine2())
			{
				address.Address2 = amazonAddress.AddressLine2.TrimSafe().Truncate(4000);
			}

			if (amazonAddress.IsSetAddressLine3())
			{
				address.Address2 = address.Address2.Grow(amazonAddress.AddressLine3.TrimSafe(), ", ").Truncate(4000);
			}

			// normalize
			if (address.Address1.IsEmpty() && address.Address2.HasValue())
			{
				address.Address1 = address.Address2;
				address.Address2 = null;
			}
			else if (address.Address1.HasValue() && address.Address1 == address.Address2)
			{
				address.Address2 = null;
			}

			if (amazonAddress.IsSetCity())
			{
				address.City = amazonAddress.City.TrimSafe().Truncate(4000);
			}

			if (amazonAddress.IsSetPostalCode())
			{
				address.ZipPostalCode = amazonAddress.PostalCode.TrimSafe().Truncate(4000);
			}

			if (amazonAddress.IsSetPhone())
			{
				address.PhoneNumber = amazonAddress.Phone.TrimSafe().Truncate(4000);
			}

			if (amazonAddress.IsSetCountryCode())
			{
				var country = countryService.GetCountryByTwoOrThreeLetterIsoCode(amazonAddress.CountryCode);

				if (country != null)
				{
					address.CountryId = country.Id;
					countryAllowsShipping = country.AllowsShipping;
					countryAllowsBilling = country.AllowsBilling;
				}
			}

			if (amazonAddress.IsSetStateOrRegion())
			{
				var stateProvince = stateProvinceService.GetStateProvinceByAbbreviation(amazonAddress.StateOrRegion);

				if (stateProvince != null)
					address.StateProvinceId = stateProvince.Id;
			}

			//amazonAddress.District, amazonAddress.County ??

			if (address.CountryId == 0)
				address.CountryId = null;

			if (address.StateProvinceId == 0)
				address.StateProvinceId = null;
		}

		internal static void ToAddress(this OrderReferenceDetails details, SmartStore.Core.Domain.Common.Address address, ICountryService countryService,
			IStateProvinceService stateProvinceService, out bool countryAllowsShipping, out bool countryAllowsBilling)
		{
			countryAllowsShipping = countryAllowsBilling = true;

			if (details.IsSetBuyer() && details.Buyer.IsSetEmail())
			{
				address.Email = details.Buyer.Email;
			}

			if (details.IsSetDestination() && details.Destination.IsSetPhysicalDestination())
			{
				details.Destination.PhysicalDestination.ToAddress(address, countryService, stateProvinceService, out countryAllowsShipping, out countryAllowsBilling);
			}
		}

		internal static Order GetOrderByAmazonId(this IRepository<Order> orderRepository, string amazonId)
		{
			// S02-9777218-8608106				OrderReferenceId
			// S02-9777218-8608106-A088344		Auth ID
			// S02-9777218-8608106-C088344		Capture ID

			if (amazonId.HasValue())
			{
				string amazonOrderReferenceId = amazonId.Substring(0, amazonId.LastIndexOf('-'));
				if (amazonOrderReferenceId.HasValue())
				{
					var orders = orderRepository.Table
						.Where(x => x.PaymentMethodSystemName == AmazonPayPlugin.SystemName && x.AuthorizationTransactionId.StartsWith(amazonOrderReferenceId))
						.ToList();

					if (orders.Count() == 1)
						return orders.FirstOrDefault();
				}
			}
			return null;
		}
	}
}