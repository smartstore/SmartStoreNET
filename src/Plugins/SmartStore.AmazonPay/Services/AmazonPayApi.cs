using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using OffAmazonPaymentsService.Model;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Utilities;

namespace SmartStore.AmazonPay.Api
{
	public class AmazonPayApi : IAmazonPayApi
	{
		private readonly ICountryService _countryService;
		private readonly IStateProvinceService _stateProvinceService;
		private readonly IOrderService _orderService;
		private readonly IAddressService _addressService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly CurrencySettings _currencySettings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICommonServices _services;
		private readonly IPluginFinder _pluginFinder;

		public AmazonPayApi(
			ICountryService countryService,
			IStateProvinceService stateProvinceService,
			IOrderService orderService,
			IAddressService addressService,
			IDateTimeHelper dateTimeHelper,
			CurrencySettings currencySettings,
			IOrderTotalCalculationService orderTotalCalculationService,
			ICommonServices services,
			IPluginFinder pluginFinder)
		{
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_orderService = orderService;
			_addressService = addressService;
			_dateTimeHelper = dateTimeHelper;
			_currencySettings = currencySettings;
			_orderTotalCalculationService = orderTotalCalculationService;
			_services = services;
			_pluginFinder = pluginFinder;
		}

		private string GetRandomId(string prefix)
		{
			var str = prefix + CommonHelper.GenerateRandomDigitCode(20);
			return str.Truncate(32);
		}

		public static string PlatformId
		{
			get	{ return "A3OJ83WFYM72IY"; }
		}

		public static string LeadCode
		{
			get { return "SPEXDEAPA-SmartStore.Net-CP-DP"; }
		}

		public AmazonPayApiClient CreateClient(AmazonPaySettings settings)
		{
			var appVersion = "1.0";
			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(AmazonPayPlugin.SystemName);

			if (descriptor != null)
			{
				appVersion = descriptor.Version.ToString();
			}

			return new AmazonPayApiClient(settings, appVersion);
		}

		public bool FulfillBillingAddress(AmazonPaySettings settings, Order order, AuthorizationDetails details, out string formattedAddress)
		{
			formattedAddress = "";

			if (details == null)
			{
				AmazonPayApiData data;
				details = GetAuthorizationDetails(CreateClient(settings), order.AuthorizationTransactionId, out data);
			}

			if (details == null || !details.IsSetAuthorizationBillingAddress())
				return false;

			bool countryAllowsShipping, countryAllowsBilling;

			// override billing address cause it is just copy of the shipping address
			details.AuthorizationBillingAddress.ToAddress(order.BillingAddress, _countryService, _stateProvinceService, out countryAllowsShipping, out countryAllowsBilling);

			if (!countryAllowsBilling)
			{
				formattedAddress = details.AuthorizationBillingAddress.ToFormatedAddress(_countryService, _stateProvinceService);
				return false;
			}

			order.BillingAddress.CreatedOnUtc = DateTime.UtcNow;

			if (order.BillingAddress.Email.IsEmpty())
				order.BillingAddress.Email = order.Customer.Email;

			_orderService.UpdateOrder(order);

			formattedAddress = details.AuthorizationBillingAddress.ToFormatedAddress(_countryService, _stateProvinceService);

			return true;
		}
	
		public AuthorizationDetails GetAuthorizationDetails(AmazonPayApiClient client, string authorizationId, out AmazonPayApiData data)
		{
			data = new AmazonPayApiData();

			AuthorizationDetails details = null;
			var request = new GetAuthorizationDetailsRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonAuthorizationId = authorizationId;

			var response = client.Service.GetAuthorizationDetails(request);

			if (response.IsSetGetAuthorizationDetailsResult())
			{
				var result = response.GetAuthorizationDetailsResult;

				if (result != null && result.IsSetAuthorizationDetails())
					details = result.AuthorizationDetails;
			}

			try
			{
				data.MessageType = "GetAuthorizationDetails";

				if (response.IsSetResponseMetadata() && response.ResponseMetadata.IsSetRequestId())
					data.MessageId = response.ResponseMetadata.RequestId;

				if (details != null)
				{
					if (details.IsSetAmazonAuthorizationId())
						data.AuthorizationId = details.AmazonAuthorizationId;

					if (details.IsSetAuthorizationReferenceId())
						data.ReferenceId = details.AuthorizationReferenceId;

					if (details.IsSetIdList() && details.IdList.IsSetmember())
						data.CaptureId = (details.IdList.member != null && details.IdList.member.Count > 0 ? details.IdList.member[0] : null);

					if (details.IsSetAuthorizationFee())
						data.Fee = new AmazonPayApiPrice(details.AuthorizationFee.Amount, details.AuthorizationFee.CurrencyCode);

					if (details.IsSetAuthorizationAmount())
						data.AuthorizedAmount = new AmazonPayApiPrice(details.AuthorizationAmount.Amount, details.AuthorizationAmount.CurrencyCode);

					if (details.IsSetCapturedAmount())
						data.CapturedAmount = new AmazonPayApiPrice(details.CapturedAmount.Amount, details.CapturedAmount.CurrencyCode);

					if (details.IsSetCaptureNow())
						data.CaptureNow = details.CaptureNow;

					if (details.IsSetCreationTimestamp())
						data.Creation = details.CreationTimestamp;

					if (details.IsSetExpirationTimestamp())
						data.Expiration = details.ExpirationTimestamp;

					if (details.IsSetAuthorizationStatus())
					{
						data.ReasonCode = details.AuthorizationStatus.ReasonCode;
						data.ReasonDescription = details.AuthorizationStatus.ReasonDescription;
						data.State = details.AuthorizationStatus.State.ToString();
						data.StateLastUpdate = details.AuthorizationStatus.LastUpdateTimestamp;
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return details;
		}

		public CaptureDetails GetCaptureDetails(AmazonPayApiClient client, string captureId, out AmazonPayApiData data)
		{
			data = new AmazonPayApiData();

			CaptureDetails details = null;
			var request = new GetCaptureDetailsRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonCaptureId = captureId;

			var response = client.Service.GetCaptureDetails(request);

			if (response != null && response.IsSetGetCaptureDetailsResult())
			{
				var result = response.GetCaptureDetailsResult;
				if (result != null && result.IsSetCaptureDetails())
					details = result.CaptureDetails;
			}

			try
			{
				data.MessageType = "GetCaptureDetails";

				if (response.IsSetResponseMetadata() && response.ResponseMetadata.IsSetRequestId())
					data.MessageId = response.ResponseMetadata.RequestId;

				if (details != null)
				{
					if (details.IsSetAmazonCaptureId())
						data.CaptureId = details.AmazonCaptureId;

					if (details.IsSetCaptureReferenceId())
						data.ReferenceId = details.CaptureReferenceId;

					if (details.IsSetCaptureFee())
						data.Fee = new AmazonPayApiPrice(details.CaptureFee.Amount, details.CaptureFee.CurrencyCode);

					if (details.IsSetCaptureAmount())
						data.CapturedAmount = new AmazonPayApiPrice(details.CaptureAmount.Amount, details.CaptureAmount.CurrencyCode);

					if (details.IsSetRefundedAmount())
						data.RefundedAmount = new AmazonPayApiPrice(details.RefundedAmount.Amount, details.RefundedAmount.CurrencyCode);

					if (details.IsSetCreationTimestamp())
						data.Creation = details.CreationTimestamp;

					if (details.IsSetCaptureStatus())
					{
						data.ReasonCode = details.CaptureStatus.ReasonCode;
						data.ReasonDescription = details.CaptureStatus.ReasonDescription;
						data.State = details.CaptureStatus.State.ToString();
						data.StateLastUpdate = details.CaptureStatus.LastUpdateTimestamp;
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return details;
		}

		public RefundDetails GetRefundDetails(AmazonPayApiClient client, string refundId, out AmazonPayApiData data)
		{
			data = new AmazonPayApiData();

			RefundDetails details = null;
			var request = new GetRefundDetailsRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonRefundId = refundId;

			var response = client.Service.GetRefundDetails(request);

			if (response != null && response.IsSetGetRefundDetailsResult())
			{
				var result = response.GetRefundDetailsResult;
				if (result != null && result.IsSetRefundDetails())
					details = result.RefundDetails;
			}

			try
			{
				data.MessageType = "GetRefundDetails";

				if (response.IsSetResponseMetadata() && response.ResponseMetadata.IsSetRequestId())
					data.MessageId = response.ResponseMetadata.RequestId;

				if (details != null)
				{
					if (details.IsSetAmazonRefundId())
						data.RefundId = details.AmazonRefundId;

					if (details.IsSetRefundReferenceId())
						data.ReferenceId = details.RefundReferenceId;

					if (details.IsSetFeeRefunded())
						data.Fee = new AmazonPayApiPrice(details.FeeRefunded.Amount, details.FeeRefunded.CurrencyCode);

					if (details.IsSetRefundAmount())
						data.RefundedAmount = new AmazonPayApiPrice(details.RefundAmount.Amount, details.RefundAmount.CurrencyCode);

					if (details.IsSetCreationTimestamp())
						data.Creation = details.CreationTimestamp;

					if (details.IsSetRefundStatus())
					{
						data.ReasonCode = details.RefundStatus.ReasonCode;
						data.ReasonDescription = details.RefundStatus.ReasonDescription;
						data.State = details.RefundStatus.State.ToString();
						data.StateLastUpdate = details.RefundStatus.LastUpdateTimestamp;
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return details;
		}

		public string ToInfoString(AmazonPayApiData data)
		{
			var sb = new StringBuilder();

			try
			{
				string[] strings = _services.Localization.GetResource("Plugins.Payments.AmazonPay.MessageStrings").SplitSafe(";");

				string state = data.State.Grow(data.ReasonCode, " ");
				if (data.ReasonDescription.HasValue())
					state = "{0} ({1})".FormatWith(state, data.ReasonDescription);

				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.MessageTyp), data.MessageType.NaIfEmpty()));

				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.State), state));

				var stateDate = _dateTimeHelper.ConvertToUserTime(data.StateLastUpdate, DateTimeKind.Utc);
				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.StateUpdate), stateDate.ToString()));

				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.MessageId), data.MessageId.NaIfEmpty()));

				if (data.AuthorizationId.HasValue())
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.AuthorizationID), data.AuthorizationId));

				if (data.CaptureId.HasValue())
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.CaptureID), data.CaptureId));

				if (data.RefundId.HasValue())
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.RefundID), data.RefundId));

				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.ReferenceID), data.ReferenceId.NaIfEmpty()));

				if (data.Fee != null && data.Fee.Amount != 0.0)
				{
					bool isSigned = (data.MessageType.IsCaseInsensitiveEqual("RefundNotification") || data.MessageType.IsCaseInsensitiveEqual("GetRefundDetails"));
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.Fee), (isSigned ? "-" : "") + data.Fee.ToString()));
				}

				if (data.AuthorizedAmount != null && data.AuthorizedAmount.Amount != 0.0)
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.AuthorizedAmount), data.AuthorizedAmount.ToString()));

				if (data.CapturedAmount != null && data.CapturedAmount.Amount != 0.0)
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.CapturedAmount), data.CapturedAmount.ToString()));

				if (data.RefundedAmount != null && data.RefundedAmount.Amount != 0.0)
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.RefundedAmount), data.RefundedAmount.ToString()));

				if (data.CaptureNow.HasValue)
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.CaptureNow), data.CaptureNow.Value.ToString()));

				var creationDate = _dateTimeHelper.ConvertToUserTime(data.Creation, DateTimeKind.Utc);
				sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.Creation), creationDate.ToString()));

				if (data.Expiration.HasValue)
				{
					var expirationDate = _dateTimeHelper.ConvertToUserTime(data.Expiration.Value, DateTimeKind.Utc);
					sb.AppendLine("{0}: {1}".FormatWith(strings.SafeGet((int)AmazonPayMessage.Expiration), expirationDate.ToString()));
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return sb.ToString();
		}

		public AmazonPayApiData ParseNotification(HttpRequestBase request)
		{
			string json = null;

			using (var reader = new StreamReader(request.InputStream))
			{
				json = reader.ReadToEnd();
			}

			var parser = new OffAmazonPaymentsNotifications.NotificationsParser();
			var message = parser.ParseRawMessage(request.Headers, json);

			var data = new AmazonPayApiData()
			{
				MessageType = message.NotificationType.ToString(),
				MessageId = ((OffAmazonPaymentsNotifications.IpnNotificationMetadata)message.NotificationMetadata).NotificationReferenceId
			};

			if (message.NotificationType == OffAmazonPaymentsNotifications.NotificationType.AuthorizationNotification)
			{
				var details = ((OffAmazonPaymentsNotifications.AuthorizationNotification)message).AuthorizationDetails;

				data.AuthorizationId = details.AmazonAuthorizationId;
				data.CaptureId = details.IdList.SafeGet(0);
				data.ReferenceId = details.AuthorizationReferenceId;
				data.CaptureNow = details.CaptureNow;
				data.Creation = details.CreationTimestamp;

				if (details.AuthorizationFee != null)
					data.Fee = new AmazonPayApiPrice(details.AuthorizationFee.Amount, details.AuthorizationFee.CurrencyCode);

				if (details.AuthorizationAmount != null)
					data.AuthorizedAmount = new AmazonPayApiPrice(details.AuthorizationAmount.Amount, details.AuthorizationAmount.CurrencyCode);

				if (details.CapturedAmount != null)
					data.CapturedAmount = new AmazonPayApiPrice(details.CapturedAmount.Amount, details.CapturedAmount.CurrencyCode);

				if (details.ExpirationTimestampSpecified)
					data.Expiration = details.ExpirationTimestamp;

				if (details.AuthorizationStatus != null)
				{
					data.ReasonCode = details.AuthorizationStatus.ReasonCode;
					data.ReasonDescription = details.AuthorizationStatus.ReasonDescription;
					data.State = details.AuthorizationStatus.State;
					data.StateLastUpdate = details.AuthorizationStatus.LastUpdateTimestamp;
				}
			}
			else if (message.NotificationType == OffAmazonPaymentsNotifications.NotificationType.CaptureNotification)
			{
				var details = ((OffAmazonPaymentsNotifications.CaptureNotification)message).CaptureDetails;

				data.CaptureId = details.AmazonCaptureId;
				data.ReferenceId = details.CaptureReferenceId;
				data.Creation = details.CreationTimestamp;

				if (details.CaptureFee != null)
					data.Fee = new AmazonPayApiPrice(details.CaptureFee.Amount, details.CaptureFee.CurrencyCode);

				if (details.CaptureAmount != null)
					data.CapturedAmount = new AmazonPayApiPrice(details.CaptureAmount.Amount, details.CaptureAmount.CurrencyCode);

				if (details.RefundedAmount != null)
					data.RefundedAmount = new AmazonPayApiPrice(details.RefundedAmount.Amount, details.RefundedAmount.CurrencyCode);

				if (details.CaptureStatus != null)
				{
					data.ReasonCode = details.CaptureStatus.ReasonCode;
					data.ReasonDescription = details.CaptureStatus.ReasonDescription;
					data.State = details.CaptureStatus.State;
					data.StateLastUpdate = details.CaptureStatus.LastUpdateTimestamp;
				}
			}
			else if (message.NotificationType == OffAmazonPaymentsNotifications.NotificationType.RefundNotification)
			{
				var details = ((OffAmazonPaymentsNotifications.RefundNotification)message).RefundDetails;

				data.RefundId = details.AmazonRefundId;
				data.ReferenceId = details.RefundReferenceId;
				data.Creation = details.CreationTimestamp;

				if (details.FeeRefunded != null)
					data.Fee = new AmazonPayApiPrice(details.FeeRefunded.Amount, details.FeeRefunded.CurrencyCode);

				if (details.RefundAmount != null)
					data.RefundedAmount = new AmazonPayApiPrice(details.RefundAmount.Amount, details.RefundAmount.CurrencyCode);

				if (details.RefundStatus != null)
				{
					data.ReasonCode = details.RefundStatus.ReasonCode;
					data.ReasonDescription = details.RefundStatus.ReasonDescription;
					data.State = details.RefundStatus.State;
					data.StateLastUpdate = details.RefundStatus.LastUpdateTimestamp;
				}
			}
			
			return data;
		}
	}
}