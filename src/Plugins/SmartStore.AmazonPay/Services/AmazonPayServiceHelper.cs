﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using AmazonPay;
using AmazonPay.CommonRequests;
using AmazonPay.Responses;
using SmartStore.AmazonPay.Services.Internal;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Services.Common;
using SmartStore.Utilities;

namespace SmartStore.AmazonPay.Services
{
	/// <summary>
	/// Helper with utilities to keep the AmazonPayService tidy.
	/// </summary>
	public partial class AmazonPayService
	{
		public static string PlatformId
		{
			get { return "A3OJ83WFYM72IY"; }
		}

		public static string LeadCode
		{
			get { return "SPEXDEAPA-SmartStore.Net-CP-DP"; }
		}

		private string GetPluginUrl(string action, bool useSsl = false)
		{
			var pluginUrl = "{0}Plugins/SmartStore.AmazonPay/AmazonPay/{1}".FormatInvariant(_services.WebHelper.GetStoreLocation(useSsl), action);
			return pluginUrl;
		}

		private void SerializeOrderAttribute(AmazonPayOrderAttribute attribute, Order order)
		{
			if (attribute != null)
			{
				var sb = new StringBuilder();
				using (var writer = new StringWriter(sb))
				{
					var serializer = new XmlSerializer(typeof(AmazonPayOrderAttribute));
					serializer.Serialize(writer, attribute);

					_genericAttributeService.SaveAttribute<string>(order, AmazonPayPlugin.SystemName + ".OrderAttribute", sb.ToString(), order.StoreId);
				}
			}
		}

		private AmazonPayOrderAttribute DeserializeOrderAttribute(Order order)
		{
			var serialized = order.GetAttribute<string>(AmazonPayPlugin.SystemName + ".OrderAttribute", _genericAttributeService, order.StoreId);

			if (!serialized.HasValue())
			{
				var attribute = new AmazonPayOrderAttribute();

				// legacy < v.1.14
				attribute.OrderReferenceId = order.GetAttribute<string>(AmazonPayPlugin.SystemName + ".OrderReferenceId", order.StoreId);

				return attribute;
			}

			using (var reader = new StringReader(serialized))
			{
				var serializer = new XmlSerializer(typeof(AmazonPayOrderAttribute));
				return (AmazonPayOrderAttribute)serializer.Deserialize(reader);
			}
		}

		private bool IsPaymentMethodActive(int storeId, bool logInactive = false)
		{
			var isActive = _paymentService.IsPaymentMethodActive(AmazonPayPlugin.SystemName, storeId);

			if (!isActive && logInactive)
			{
				Logger.Error(null, T("Plugins.Payments.AmazonPay.PaymentMethodNotActive", _services.StoreContext.CurrentStore.Name));
			}

			return isActive;
		}

		private void AddOrderNote(AmazonPaySettings settings, Order order, string anyString = null, bool isIpn = false)
		{
			try
			{
				if (!settings.AddOrderNotes || order == null)
					return;

				var sb = new StringBuilder();
				var faviconUrl = "{0}Plugins/{1}/Content/images/favicon.png".FormatInvariant(_services.WebHelper.GetStoreLocation(false), AmazonPayPlugin.SystemName);

				sb.AppendFormat("<img src=\"{0}\" style=\"float: left; width: 16px; height: 16px;\" />", faviconUrl);
				sb.AppendFormat("<span style=\"padding-left: 4px;\">{0}</span>", T("Plugins.Payments.AmazonPay.AmazonDataProcessed"));
				sb.Append(":<br />");
				sb.Append(anyString.NaIfEmpty());

				if (isIpn)
				{
					order.HasNewPaymentNotification = true;
				}

				order.OrderNotes.Add(new OrderNote
				{
					Note = sb.ToString(),
					DisplayToCustomer = false,
					CreatedOnUtc = DateTime.UtcNow
				});

				_orderService.UpdateOrder(order);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
		}

		private Regions.currencyCode ConvertCurrency(string currencyCode)
		{
			switch (currencyCode.EmptyNull().ToLower())
			{
				case "usd":
					return Regions.currencyCode.USD;
				case "gbp":
					return Regions.currencyCode.GBP;
				case "jpy":
					return Regions.currencyCode.JPY;
				default:
					return Regions.currencyCode.EUR;
			}
		}

		private Address CreateAddress(
			string email,
			string buyerName,
			string addressLine1,
			string addressLine2,
			string addressLine3,
			string city,
			string postalCode,
			string phone,
			string countryCode,
			string stateRegion,
			string county,
			string destrict,
			out bool countryAllowsShipping,
			out bool countryAllowsBilling)
		{
			countryAllowsShipping = countryAllowsBilling = true;

			var address = new Address();
			address.CreatedOnUtc = DateTime.UtcNow;
			address.Email = email;
			address.ToFirstAndLastName(buyerName);
			address.Address1 = addressLine1.EmptyNull().Trim().Truncate(4000);
			address.Address2 = addressLine2.EmptyNull().Trim().Truncate(4000);
			address.Address2 = address.Address2.Grow(addressLine3.EmptyNull().Trim(), ", ").Truncate(4000);
			address.City = county.Grow(destrict, " ").Grow(city, " ").EmptyNull().Trim().Truncate(4000);
			address.ZipPostalCode = postalCode.EmptyNull().Trim().Truncate(4000);
			address.PhoneNumber = phone.EmptyNull().Trim().Truncate(4000);

			if (countryCode.HasValue())
			{
				var country = _countryService.GetCountryByTwoOrThreeLetterIsoCode(countryCode);
				if (country != null)
				{
					address.CountryId = country.Id;
					countryAllowsShipping = country.AllowsShipping;
					countryAllowsBilling = country.AllowsBilling;
				}
			}

			if (stateRegion.HasValue())
			{
				var stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateRegion);
				if (stateProvince != null)
				{
					address.StateProvinceId = stateProvince.Id;
				}
			}

			// Normalize.
			if (address.Address1.IsEmpty() && address.Address2.HasValue())
			{
				address.Address1 = address.Address2;
				address.Address2 = null;
			}
			else if (address.Address1.HasValue() && address.Address1 == address.Address2)
			{
				address.Address2 = null;
			}

			if (address.CountryId == 0)
			{
				address.CountryId = null;
			}

			if (address.StateProvinceId == 0)
			{
				address.StateProvinceId = null;
			}

			return address;
		}

		//private void GetAddress(OrderReferenceDetailsResponse details, Address address, out bool countryAllowsShipping, out bool countryAllowsBilling)
		//{
		//	countryAllowsShipping = countryAllowsBilling = true;

		//	address.Email = details.GetEmail();
		//	address.ToFirstAndLastName(details.GetBuyerName());
		//	address.Address1 = details.GetAddressLine1().EmptyNull().Trim().Truncate(4000);
		//	address.Address2 = details.GetAddressLine2().EmptyNull().Trim().Truncate(4000);
		//	address.Address2 = address.Address2.Grow(details.GetAddressLine3().EmptyNull().Trim(), ", ").Truncate(4000);
		//	address.City = details.GetCity().EmptyNull().Trim().Truncate(4000);
		//	address.ZipPostalCode = details.GetPostalCode().EmptyNull().Trim().Truncate(4000);
		//	address.PhoneNumber = details.GetPhone().EmptyNull().Trim().Truncate(4000);

		//	var countryCode = details.GetCountryCode();
		//	if (countryCode.HasValue())
		//	{
		//		var country = _countryService.GetCountryByTwoOrThreeLetterIsoCode(countryCode);
		//		if (country != null)
		//		{
		//			address.CountryId = country.Id;
		//			countryAllowsShipping = country.AllowsShipping;
		//			countryAllowsBilling = country.AllowsBilling;
		//		}
		//	}

		//	var stateRegion = details.GetStateOrRegion();
		//	if (stateRegion.HasValue())
		//	{
		//		var stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateRegion);
		//		if (stateProvince != null)
		//		{
		//			address.StateProvinceId = stateProvince.Id;
		//		}
		//	}

		//	// Normalize.
		//	if (address.Address1.IsEmpty() && address.Address2.HasValue())
		//	{
		//		address.Address1 = address.Address2;
		//		address.Address2 = null;
		//	}
		//	else if (address.Address1.HasValue() && address.Address1 == address.Address2)
		//	{
		//		address.Address2 = null;
		//	}

		//	if (address.CountryId == 0)
		//	{
		//		address.CountryId = null;
		//	}

		//	if (address.StateProvinceId == 0)
		//	{
		//		address.StateProvinceId = null;
		//	}
		//}

		//private bool FindAndApplyAddress(OrderReferenceDetailsResponse details, Customer customer, bool isShippable, bool forceToTakeAmazonAddress)
		//{
		//	// PlaceOrder requires billing address but we don't get one from Amazon here. so use shipping address instead until we get it from amazon.
		//	var countryAllowsShipping = true;
		//	var countryAllowsBilling = true;

		//	var address = new Address();
		//	address.CreatedOnUtc = DateTime.UtcNow;

		//	GetAddress(details, address, out countryAllowsShipping, out countryAllowsBilling);

		//	if (isShippable && !countryAllowsShipping)
		//		return false;

		//	if (address.Email.IsEmpty())
		//	{
		//		address.Email = customer.Email;
		//	}

		//	if (forceToTakeAmazonAddress)
		//	{
		//		// First time to get in touch with an amazon address.
		//		var existingAddress = customer.Addresses.ToList().FindAddress(address, true);
		//		if (existingAddress == null)
		//		{
		//			customer.Addresses.Add(address);
		//			customer.BillingAddress = address;
		//		}
		//		else
		//		{
		//			customer.BillingAddress = existingAddress;
		//		}
		//	}
		//	else
		//	{
		//		if (customer.BillingAddress == null)
		//		{
		//			customer.Addresses.Add(address);
		//			customer.BillingAddress = address;
		//		}

		//		GetAddress(details, customer.BillingAddress, out countryAllowsShipping, out countryAllowsBilling);

		//		// But now we could have dublicates.
		//		var newAddressId = customer.BillingAddress.Id;
		//		var addresses = customer.Addresses.Where(x => x.Id != newAddressId).ToList();

		//		var existingAddress = addresses.FindAddress(customer.BillingAddress, false);
		//		if (existingAddress != null)
		//		{
		//			// Remove the new and take the old one.
		//			customer.RemoveAddress(customer.BillingAddress);
		//			customer.BillingAddress = existingAddress;

		//			try
		//			{
		//				_addressService.DeleteAddress(newAddressId);
		//			}
		//			catch (Exception exception)
		//			{
		//				exception.Dump();
		//			}
		//		}
		//	}

		//	customer.ShippingAddress = (isShippable ? customer.BillingAddress : null);

		//	return true;
		//}

		private AmazonPayData GetDetails(AuthorizeResponse response)
		{
			var data = new AmazonPayData();
			data.MessageType = "GetAuthorizationDetails";
			data.MessageId = response.GetRequestId();
			data.AuthorizationId = response.GetAuthorizationId();
			data.ReferenceId = response.GetAuthorizationReferenceId();

			var ids = response.GetCaptureIdList();
			if (ids.Any())
			{
				data.CaptureId = ids.First();
			}

			data.Fee = new AmazonPayPrice(response.GetAuthorizationFee(), response.GetAuthorizationFeeCurrencyCode());
			data.AuthorizedAmount = new AmazonPayPrice(response.GetAuthorizationAmount(), response.GetAuthorizationAmountCurrencyCode());
			data.CapturedAmount = new AmazonPayPrice(response.GetCapturedAmount(), response.GetCapturedAmountCurrencyCode());
			data.CaptureNow = response.GetCaptureNow();
			data.Creation = response.GetCreationTimestamp();
			data.Expiration = response.GetExpirationTimestamp();
			data.ReasonCode = response.GetReasonCode();
			data.ReasonDescription = response.GetReasonDescription();
			data.State = response.GetAuthorizationState();
			data.StateLastUpdate = response.GetLastUpdateTimestamp();

			return data;
		}
		private AmazonPayData GetDetails(CaptureResponse response)
		{
			var data = new AmazonPayData();
			data.MessageType = "GetCaptureDetails";
			data.MessageId = response.GetRequestId();
			data.CaptureId = response.GetCaptureId();
			data.ReferenceId = response.GetCaptureReferenceId();
			data.Fee = new AmazonPayPrice(response.GetCaptureFee(), response.GetCaptureFeeCurrencyCode());
			data.CapturedAmount = new AmazonPayPrice(response.GetCaptureAmount(), response.GetCaptureAmountCurrencyCode());
			data.RefundedAmount = new AmazonPayPrice(response.refundedAmount, response.refundedAmountCurrencyCode);
			data.Creation = response.GetCreationTimestamp();
			data.ReasonCode = response.GetReasonCode();
			data.ReasonDescription = response.GetReasonDescription();
			data.State = response.GetCaptureState();
			data.StateLastUpdate = response.GetLastUpdatedTimestamp();

			return data;
		}
		private AmazonPayData GetDetails(RefundResponse response)
		{
			var data = new AmazonPayData();
			data.MessageType = "GetRefundDetails";
			data.MessageId = response.GetRequestId();
			data.ReferenceId = response.GetRefundReferenceId();
			data.Creation = response.GetCreationTimestamp();
			data.Fee = new AmazonPayPrice(response.GetRefundFee(), response.GetRefundFeeCurrencyCode());
			data.RefundedAmount = new AmazonPayPrice(response.GetRefundAmount(), response.GetRefundAmountCurrencyCode());
			data.ReasonCode = response.GetReasonCode();
			data.ReasonDescription = response.GetReasonDescription();
			data.State = response.GetRefundState();
			data.StateLastUpdate = response.GetLastUpdateTimestamp();

			return data;
		}

		private string GetRandomId(string prefix)
		{
			var str = prefix + CommonHelper.GenerateRandomDigitCode(20);
			return str.Truncate(32);
		}

		private string LogError(IResponse response, bool isWarning = false)
		{
			var message = $"{response.GetErrorMessage().NaIfEmpty()} ({response.GetErrorCode().NaIfEmpty()})";

			Logger.Log(isWarning ? LogLevel.Warning : LogLevel.Error, new Exception(response.GetJson()), message, null);

			return message;
		}

		private string ToInfoString(AmazonPayData data)
		{
			var sb = new StringBuilder();

			try
			{
				var strings = _services.Localization.GetResource("Plugins.Payments.AmazonPay.MessageStrings").SplitSafe(";");
				var state = data.State.Grow(data.ReasonCode, " ");

				if (data.ReasonDescription.HasValue())
					state = $"{state} ({data.ReasonDescription})";

				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.MessageTyp)}: {data.MessageType.NaIfEmpty()}");
				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.State)}: {state}");

				var stateDate = _dateTimeHelper.ConvertToUserTime(data.StateLastUpdate, DateTimeKind.Utc);
				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.StateUpdate)}: {stateDate.ToString()}");

				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.MessageId)}: {data.MessageId.NaIfEmpty()}");

				if (data.AuthorizationId.HasValue())
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.AuthorizationID)}: {data.AuthorizationId}");

				if (data.CaptureId.HasValue())
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.CaptureID)}: {data.CaptureId}");

				if (data.RefundId.HasValue())
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.RefundID)}: {data.RefundId}");

				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.ReferenceID)}: {data.ReferenceId.NaIfEmpty()}");

				if (data.Fee != null && data.Fee.Amount != decimal.Zero)
				{
					var signed = data.MessageType.IsCaseInsensitiveEqual("RefundNotification") || data.MessageType.IsCaseInsensitiveEqual("GetRefundDetails") ? "-" : "";
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.Fee)}: {signed}");
				}

				if (data.AuthorizedAmount != null && data.AuthorizedAmount.Amount != decimal.Zero)
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.AuthorizedAmount)}: {data.AuthorizedAmount.ToString()}");

				if (data.CapturedAmount != null && data.CapturedAmount.Amount != decimal.Zero)
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.CapturedAmount)}: {data.CapturedAmount.ToString()}");

				if (data.RefundedAmount != null && data.RefundedAmount.Amount != decimal.Zero)
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.RefundedAmount)}: {data.RefundedAmount.ToString()}");

				if (data.CaptureNow.HasValue)
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.CaptureNow)}: {data.CaptureNow.Value.ToString()}");

				var creationDate = _dateTimeHelper.ConvertToUserTime(data.Creation, DateTimeKind.Utc);
				sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.Creation)}: {creationDate.ToString()}");

				if (data.Expiration.HasValue)
				{
					var expirationDate = _dateTimeHelper.ConvertToUserTime(data.Expiration.Value, DateTimeKind.Utc);
					sb.AppendLine($"{strings.SafeGet((int)AmazonPayMessage.Expiration)}: {expirationDate.ToString()}");
				}
			}
			catch (Exception exception)
			{
				exception.Dump();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Creates an API client.
		/// </summary>
		/// <param name="settings">AmazonPay settings</param>
		/// <param name="currencyCode">Currency code of primary store currency</param>
		/// <returns>AmazonPay client</returns>
		private Client CreateClient(AmazonPaySettings settings)
		{
			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(AmazonPayPlugin.SystemName);
			var appVersion = descriptor != null ? descriptor.Version.ToString() : "1.0";

			Regions.supportedRegions region;
			switch (settings.Marketplace.EmptyNull().ToLower())
			{
				case "us":
					region = Regions.supportedRegions.us;
					break;
				case "uk":
					region = Regions.supportedRegions.uk;
					break;
				case "jp":
					region = Regions.supportedRegions.jp;
					break;
				default:
					region = Regions.supportedRegions.de;
					break;
			}

			var config = new Configuration()
				.WithAccessKey(settings.AccessKey)
				.WithClientId(settings.ClientId)
				.WithSecretKey(settings.SecretKey)
				.WithSandbox(settings.UseSandbox)
				.WithApplicationName("SmartStore.Net " + AmazonPayPlugin.SystemName)
				.WithApplicationVersion(appVersion)
				.WithRegion(region);

			var client = new Client(config);
			return client;
		}

		private T WorkaroundSdkCurrencyFormattingBug<T>(Func<T> request)
		{
			T result = default(T);
			var oldCulture = Thread.CurrentThread.CurrentCulture;
			var oldUICulture = Thread.CurrentThread.CurrentUICulture;

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			try
			{
				result = request();
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldCulture;
				Thread.CurrentThread.CurrentUICulture = oldUICulture;
			}

			return result;
		}
	}
}