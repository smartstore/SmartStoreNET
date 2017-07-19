using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AmazonPay;
using AmazonPay.CommonRequests;
using AmazonPay.Responses;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
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

		#region Utilities

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
				LogError(null, T("Plugins.Payments.AmazonPay.PaymentMethodNotActive", _services.StoreContext.CurrentStore.Name));
			}

			return isActive;
		}

		private void AddOrderNote(AmazonPaySettings settings, Order order, AmazonPayOrderNote note, string anyString = null, bool isIpn = false)
		{
			try
			{
				if (!settings.AddOrderNotes || order == null)
					return;

				var sb = new StringBuilder();

				string[] orderNoteStrings = T("Plugins.Payments.AmazonPay.OrderNoteStrings").Text.SplitSafe(";");
				string faviconUrl = "{0}Plugins/{1}/Content/images/favicon.png".FormatWith(_services.WebHelper.GetStoreLocation(false), AmazonPayPlugin.SystemName);

				sb.AppendFormat("<img src=\"{0}\" style=\"float: left; width: 16px; height: 16px;\" />", faviconUrl);

				if (anyString.HasValue())
				{
					anyString = orderNoteStrings.SafeGet((int)note).FormatWith(anyString);
				}
				else
				{
					anyString = orderNoteStrings.SafeGet((int)note);
					anyString = anyString.Replace("{0}", "");
				}

				if (anyString.HasValue())
				{
					sb.AppendFormat("<span style=\"padding-left: 4px;\">{0}</span>", anyString);
				}

				if (isIpn)
					order.HasNewPaymentNotification = true;

				order.OrderNotes.Add(new OrderNote
				{
					Note = sb.ToString(),
					DisplayToCustomer = false,
					CreatedOnUtc = DateTime.UtcNow
				});

				_orderService.UpdateOrder(order);
			}
			catch (Exception exc)
			{
				LogError(exc);
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

		private void GetAddress(OrderReferenceDetailsResponse details, Address amazonAddress, out bool countryAllowsShipping, out bool countryAllowsBilling)
		{
			countryAllowsShipping = countryAllowsBilling = true;

			amazonAddress.Email = details.GetEmail();
			amazonAddress.ToFirstAndLastName(details.GetBuyerName());
			amazonAddress.Address1 = details.GetAddressLine1().EmptyNull().Trim().Truncate(4000);
			amazonAddress.Address2 = details.GetAddressLine2().EmptyNull().Trim().Truncate(4000);
			amazonAddress.Address2 = amazonAddress.Address2.Grow(details.GetAddressLine3().EmptyNull().Trim(), ", ").Truncate(4000);
			amazonAddress.City = details.GetCity().EmptyNull().Trim().Truncate(4000);
			amazonAddress.ZipPostalCode = details.GetPostalCode().EmptyNull().Trim().Truncate(4000);
			amazonAddress.PhoneNumber = details.GetPhone().EmptyNull().Trim().Truncate(4000);

			var countryCode = details.GetCountryCode();
			if (countryCode.HasValue())
			{
				var country = _countryService.GetCountryByTwoOrThreeLetterIsoCode(countryCode);
				if (country != null)
				{
					amazonAddress.CountryId = country.Id;
					countryAllowsShipping = country.AllowsShipping;
					countryAllowsBilling = country.AllowsBilling;
				}
			}

			var stateRegion = details.GetStateOrRegion();
			if (stateRegion.HasValue())
			{
				var stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateRegion);
				if (stateProvince != null)
				{
					amazonAddress.StateProvinceId = stateProvince.Id;
				}
			}

			// Normalize.
			if (amazonAddress.Address1.IsEmpty() && amazonAddress.Address2.HasValue())
			{
				amazonAddress.Address1 = amazonAddress.Address2;
				amazonAddress.Address2 = null;
			}
			else if (amazonAddress.Address1.HasValue() && amazonAddress.Address1 == amazonAddress.Address2)
			{
				amazonAddress.Address2 = null;
			}

			if (amazonAddress.CountryId == 0)
			{
				amazonAddress.CountryId = null;
			}

			if (amazonAddress.StateProvinceId == 0)
			{
				amazonAddress.StateProvinceId = null;
			}
		}

		private string GetRandomId(string prefix)
		{
			var str = prefix + CommonHelper.GenerateRandomDigitCode(20);
			return str.Truncate(32);
		}

		private void LogError(IResponse response, IList<string> errors = null, bool isWarning = false)
		{
			var message = $"{response.GetErrorMessage().NaIfEmpty()} ({response.GetErrorCode().NaIfEmpty()})";

			if (isWarning)
			{
				Logger.Warn(message);
			}
			else
			{
				Logger.Error(message);
			}

			if (errors != null)
			{
				errors.Add(message);
			}
		}

		#endregion

		private Order FindOrder(AmazonPayApiData data)
		{
			Order order = null;
			string errorId = null;

			if (data.MessageType.IsCaseInsensitiveEqual("AuthorizationNotification"))
			{
				if ((order = _orderService.GetOrderByPaymentAuthorization(AmazonPayPlugin.SystemName, data.AuthorizationId)) == null)
					errorId = "AuthorizationId {0}".FormatWith(data.AuthorizationId);
			}
			else if (data.MessageType.IsCaseInsensitiveEqual("CaptureNotification"))
			{
				if ((order = _orderService.GetOrderByPaymentCapture(AmazonPayPlugin.SystemName, data.CaptureId)) == null)
					order = _orderRepository.GetOrderByAmazonId(data.AnyAmazonId);

				if (order == null)
					errorId = "CaptureId {0}".FormatWith(data.CaptureId);
			}
			else if (data.MessageType.IsCaseInsensitiveEqual("RefundNotification"))
			{
				var attribute = _genericAttributeService.GetAttributes(AmazonPayPlugin.SystemName + ".RefundId", "Order")
					.Where(x => x.Value == data.RefundId)
					.FirstOrDefault();

				if (attribute == null || (order = _orderService.GetOrderById(attribute.EntityId)) == null)
					order = _orderRepository.GetOrderByAmazonId(data.AnyAmazonId);

				if (order == null)
					errorId = "RefundId {0}".FormatWith(data.RefundId);
			}

			if (errorId.HasValue())
			{
				Logger.Warn(T("Plugins.Payments.AmazonPay.OrderNotFound", errorId));
			}

			return order;
		}

		private bool FindAndApplyAddress(OrderReferenceDetailsResponse details, Customer customer, bool isShippable, bool forceToTakeAmazonAddress)
		{
			// PlaceOrder requires billing address but we don't get one from Amazon here. so use shipping address instead until we get it from amazon.
			var countryAllowsShipping = true;
			var countryAllowsBilling = true;

			var amazonAddress = new Address();
			amazonAddress.CreatedOnUtc = DateTime.UtcNow;

			GetAddress(details, amazonAddress, out countryAllowsShipping, out countryAllowsBilling);

			if (isShippable && !countryAllowsShipping)
				return false;

			if (amazonAddress.Email.IsEmpty())
			{
				amazonAddress.Email = customer.Email;
			}

			if (forceToTakeAmazonAddress)
			{
				// First time to get in touch with an amazon address.
				var existingAddress = customer.Addresses.ToList().FindAddress(amazonAddress, true);
				if (existingAddress == null)
				{
					customer.Addresses.Add(amazonAddress);
					customer.BillingAddress = amazonAddress;
				}
				else
				{
					customer.BillingAddress = existingAddress;
				}
			}
			else
			{
				if (customer.BillingAddress == null)
				{
					customer.Addresses.Add(amazonAddress);
					customer.BillingAddress = amazonAddress;
				}

				GetAddress(details, customer.BillingAddress, out countryAllowsShipping, out countryAllowsBilling);

				// But now we could have dublicates.
				int newAddressId = customer.BillingAddress.Id;
				var addresses = customer.Addresses.Where(x => x.Id != newAddressId).ToList();

				var existingAddress = addresses.FindAddress(customer.BillingAddress, false);
				if (existingAddress != null)
				{
					// Remove the new and take the old one.
					customer.RemoveAddress(customer.BillingAddress);
					customer.BillingAddress = existingAddress;

					try
					{
						_addressService.DeleteAddress(newAddressId);
					}
					catch (Exception exception)
					{
						exception.Dump();
					}
				}
			}

			customer.ShippingAddress = (isShippable ? customer.BillingAddress : null);

			return true;
		}

		/// <summary>
		/// Creates an API client.
		/// </summary>
		/// <param name="settings">AmazonPay settings</param>
		/// <param name="currencyCode">Currency code of primary store currency</param>
		/// <returns>AmazonPay client</returns>
		private Client CreateClient(AmazonPaySettings settings, string currencyCode = null)
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
				.WithSandbox(settings.UseSandbox)
				.WithApplicationName("SmartStore.Net " + AmazonPayPlugin.SystemName)
				.WithApplicationVersion(appVersion)
				.WithRegion(region);

			if (currencyCode.HasValue())
			{
				var currency = ConvertCurrency(currencyCode);
				config = config.WithCurrencyCode(currency);
			}

			var client = new Client(config);
			return client;
		}
	}
}