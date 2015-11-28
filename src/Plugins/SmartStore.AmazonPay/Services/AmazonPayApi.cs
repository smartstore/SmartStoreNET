using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using OffAmazonPaymentsService;
using OffAmazonPaymentsService.Model;
using SmartStore.AmazonPay.Extensions;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Infrastructure;
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

		public AmazonPayApi(
			ICountryService countryService,
			IStateProvinceService stateProvinceService,
			IOrderService orderService,
			IAddressService addressService,
			IDateTimeHelper dateTimeHelper,
			CurrencySettings currencySettings,
			IOrderTotalCalculationService orderTotalCalculationService,
			ICommonServices services)
		{
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_orderService = orderService;
			_addressService = addressService;
			_dateTimeHelper = dateTimeHelper;
			_currencySettings = currencySettings;
			_orderTotalCalculationService = orderTotalCalculationService;
			_services = services;
		}

		private string GetRandomId(string prefix)
		{
			string str = prefix + CommonHelper.GenerateRandomDigitCode(20);
			return str.Truncate(32);
		}

		public void GetConstraints(OrderReferenceDetails details, IList<string> warnings)
		{
			try
			{
				if (details != null && warnings != null && details.IsSetConstraints())
				{
					foreach (var constraint in details.Constraints.Constraint)
					{
						string warning = "{0} ({1})".FormatWith(constraint.Description, constraint.ConstraintID);
						warnings.Add(warning);
					}
				}
			}
			catch (Exception) { }
		}

		public bool FindAndApplyAddress(OrderReferenceDetails details, Customer customer, bool isShippable, bool forceToTakeAmazonAddress)
		{
			// PlaceOrder requires billing address but we don't get one from Amazon here. so use shipping address instead until we get it from amazon.
			bool countryAllowsShipping, countryAllowsBilling;

			var amazonAddress = new SmartStore.Core.Domain.Common.Address()
			{
				CreatedOnUtc = DateTime.UtcNow
			};

			details.ToAddress(amazonAddress, _countryService, _stateProvinceService, out countryAllowsShipping, out countryAllowsBilling);

			if (isShippable && !countryAllowsShipping)
				return false;

			if (amazonAddress.Email.IsEmpty())
				amazonAddress.Email = customer.Email;

			if (forceToTakeAmazonAddress)
			{
				// first time to get in touch with an amazon address
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

				// we already have the address but it is uncomplete, so just complete it
				details.ToAddress(customer.BillingAddress, _countryService, _stateProvinceService, out countryAllowsShipping, out countryAllowsBilling);

				// but now we could have dublicates
				int newAddressId = customer.BillingAddress.Id;
				var addresses = customer.Addresses.Where(x => x.Id != newAddressId).ToList();

				var existingAddress = addresses.FindAddress(customer.BillingAddress, false);

				if (existingAddress != null)
				{
					// remove the new and take the old one
					customer.RemoveAddress(customer.BillingAddress);
					customer.BillingAddress = existingAddress;

					try
					{
						_addressService.DeleteAddress(newAddressId);
					}
					catch (Exception exc)
					{
						exc.Dump();
					}
				}
			}

			customer.ShippingAddress = (isShippable ? customer.BillingAddress : null);

			return true;
		}
		public bool FulfillBillingAddress(AmazonPaySettings settings, Order order, AuthorizationDetails details, out string formattedAddress)
		{
			formattedAddress = "";

			if (details == null)
			{
				AmazonPayApiData data;
				details = GetAuthorizationDetails(new AmazonPayClient(settings), order.AuthorizationTransactionId, out data);
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

		public OrderReferenceDetails GetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, string addressConsentToken = null)
		{
			var request = new GetOrderReferenceDetailsRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;
			request.AddressConsentToken = addressConsentToken;

			var response = client.Service.GetOrderReferenceDetails(request);

			if (response != null && response.IsSetGetOrderReferenceDetailsResult())
			{
				var detailsResult = response.GetOrderReferenceDetailsResult;

				if (detailsResult.IsSetOrderReferenceDetails())
					return detailsResult.OrderReferenceDetails;
			}
			return null;
		}

		public OrderReferenceDetails SetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, decimal? orderTotalAmount,
			string currencyCode, string orderGuid = null, string storeName = null)
		{
			var request = new SetOrderReferenceDetailsRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;

			var attributes = new OrderReferenceAttributes();
			//attributes.SellerNote = client.Settings.SellerNoteOrderReference.Truncate(1024);
			attributes.PlatformId = AmazonPayCore.PlatformId;

			if (orderTotalAmount.HasValue)
			{
				attributes.OrderTotal = new OrderTotal
				{
					Amount = orderTotalAmount.Value.ToString("0.00", CultureInfo.InvariantCulture),
					CurrencyCode = currencyCode ?? "EUR"
				};
			}

			if (orderGuid.HasValue())
			{
				attributes.SellerOrderAttributes = new SellerOrderAttributes
				{
					SellerOrderId = orderGuid,
					StoreName = storeName
				};
			}

			request.OrderReferenceAttributes = attributes;

			var response = client.Service.SetOrderReferenceDetails(request);

			if (response != null && response.IsSetSetOrderReferenceDetailsResult())
			{
				var detailsResult = response.SetOrderReferenceDetailsResult;

				if (detailsResult.IsSetOrderReferenceDetails())
					return detailsResult.OrderReferenceDetails;
			}
			return null;
		}

		public OrderReferenceDetails SetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, string currencyCode, List<OrganizedShoppingCartItem> cart)
		{
			decimal orderTotalDiscountAmountBase = decimal.Zero;
			Discount orderTotalAppliedDiscount = null;
			List<AppliedGiftCard> appliedGiftCards = null;
			int redeemedRewardPoints = 0;
			decimal redeemedRewardPointsAmount = decimal.Zero;

			decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
				out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);

			if (shoppingCartTotalBase.HasValue)
			{
				return SetOrderReferenceDetails(client, orderReferenceId, shoppingCartTotalBase, currencyCode);
			}

			return null;
		}

		/// <summary>Confirm an order reference informs Amazon that the buyer has placed the order.</summary>
		public void ConfirmOrderReference(AmazonPayClient client, string orderReferenceId)
		{
			var request = new ConfirmOrderReferenceRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;

			var response = client.Service.ConfirmOrderReference(request);
		}

		public void CancelOrderReference(AmazonPayClient client, string orderReferenceId)
		{
			var request = new CancelOrderReferenceRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;

			var response = client.Service.CancelOrderReference(request);
		}

		public void CloseOrderReference(AmazonPayClient client, string orderReferenceId)
		{
			var request = new CloseOrderReferenceRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;

			var response = client.Service.CloseOrderReference(request);
		}

		/// <summary>Asynchronous as long as we do not set TransactionTimeout to 0. So transaction is always in pending state after return.</summary>
		public void Authorize(AmazonPayClient client, ProcessPaymentResult result, List<string> errors, string orderReferenceId, decimal orderTotalAmount, string currencyCode, string orderGuid)
		{
			var request = new AuthorizeRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonOrderReferenceId = orderReferenceId;
			request.AuthorizationReferenceId = GetRandomId("Authorize");
			request.CaptureNow = (client.Settings.TransactionType == AmazonPayTransactionType.AuthorizeAndCapture);
			//request.SellerAuthorizationNote = client.Settings.SellerNoteAuthorization.Truncate(256);

			request.AuthorizationAmount = new Price()
			{
				Amount = orderTotalAmount.ToString("0.00", CultureInfo.InvariantCulture),
				CurrencyCode = currencyCode ?? "EUR"
			};

			var response = client.Service.Authorize(request);

			if (response != null && response.IsSetAuthorizeResult() && response.AuthorizeResult.IsSetAuthorizationDetails())
			{
				var details = response.AuthorizeResult.AuthorizationDetails;

				result.AuthorizationTransactionId = details.AmazonAuthorizationId;
				result.AuthorizationTransactionCode = details.AuthorizationReferenceId;

				if (details.IsSetAuthorizationStatus())
				{
					var status = details.AuthorizationStatus;

					if (status.IsSetState())
					{
						result.AuthorizationTransactionResult = status.State.ToString();
					}

					if (request.CaptureNow && details.IsSetIdList() && details.IdList.IsSetmember() && details.IdList.member.Count() > 0)
					{
						result.CaptureTransactionId = details.IdList.member[0];
					}

					if (status.IsSetReasonCode())
					{
						if (status.ReasonCode.IsCaseInsensitiveEqual("InvalidPaymentMethod") || status.ReasonCode.IsCaseInsensitiveEqual("AmazonRejected") ||
							status.ReasonCode.IsCaseInsensitiveEqual("ProcessingFailure") || status.ReasonCode.IsCaseInsensitiveEqual("TransactionTimedOut") ||
							status.ReasonCode.IsCaseInsensitiveEqual("TransactionTimeout"))
						{
							if (status.IsSetReasonDescription())
								errors.Add("{0}: {1}".FormatWith(status.ReasonCode, status.ReasonDescription));
							else
								errors.Add(status.ReasonCode);							
						}
					}
				}
			}

			// The response to the Authorize call includes the AuthorizationStatus response element, which will be always be
			// set to Pending if you have selected the asynchronous mode of operation.

			result.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Pending;
		}
	
		public AuthorizationDetails GetAuthorizationDetails(AmazonPayClient client, string authorizationId, out AmazonPayApiData data)
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

		public void Capture(AmazonPayClient client, CapturePaymentRequest capture, CapturePaymentResult result)
		{
			result.NewPaymentStatus = capture.Order.PaymentStatus;

			var request = new CaptureRequest();
			var store = _services.StoreService.GetStoreById(capture.Order.StoreId);

			request.SellerId = client.Settings.SellerId;
			request.AmazonAuthorizationId = capture.Order.AuthorizationTransactionId;
			request.CaptureReferenceId = GetRandomId("Capture");
			//request.SellerCaptureNote = client.Settings.SellerNoteCapture.Truncate(255);

			request.CaptureAmount = new Price
			{
				Amount = capture.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture),
				CurrencyCode = store.PrimaryStoreCurrency.CurrencyCode
			};

			var response = client.Service.Capture(request);

			if (response != null && response.IsSetCaptureResult() && response.CaptureResult.IsSetCaptureDetails())
			{
				var details = response.CaptureResult.CaptureDetails;

				result.CaptureTransactionId = details.AmazonCaptureId;

				if (details.IsSetCaptureStatus() && details.CaptureStatus.IsSetState())
				{
					result.CaptureTransactionResult = details.CaptureStatus.State.ToString().Grow(details.CaptureStatus.ReasonCode, " ");

					if (details.CaptureStatus.State == PaymentStatus.COMPLETED)
						result.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
				}
			}
		}

		public CaptureDetails GetCaptureDetails(AmazonPayClient client, string captureId, out AmazonPayApiData data)
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

		public string Refund(AmazonPayClient client, RefundPaymentRequest refund, RefundPaymentResult result)
		{
			result.NewPaymentStatus = refund.Order.PaymentStatus;

			string amazonRefundId = null;
			var store = _services.StoreService.GetStoreById(refund.Order.StoreId);

			var request = new RefundRequest();
			request.SellerId = client.Settings.SellerId;
			request.AmazonCaptureId = refund.Order.CaptureTransactionId;
			request.RefundReferenceId = GetRandomId("Refund");
			//request.SellerRefundNote = client.Settings.SellerNoteRefund.Truncate(255);

			request.RefundAmount = new Price
			{
				Amount = refund.AmountToRefund.ToString("0.00", CultureInfo.InvariantCulture),
				CurrencyCode = store.PrimaryStoreCurrency.CurrencyCode
			};

			var response = client.Service.Refund(request);

			if (response != null && response.IsSetRefundResult() && response.RefundResult.IsSetRefundDetails())
			{
				var details = response.RefundResult.RefundDetails;

				amazonRefundId = details.AmazonRefundId;

				if (details.IsSetRefundStatus() && details.RefundStatus.IsSetState())
				{
					if (refund.IsPartialRefund)
						result.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.PartiallyRefunded;
					else
						result.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Refunded;
				}
			}
			return amazonRefundId;
		}

		public RefundDetails GetRefundDetails(AmazonPayClient client, string refundId, out AmazonPayApiData data)
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


	public class AmazonPayClient
	{
		public AmazonPayClient(AmazonPaySettings settings)
		{
			string appVersion = "1.0";

			try
			{
				appVersion = EngineContext.Current.Resolve<IPluginFinder>().GetPluginDescriptorBySystemName(AmazonPayCore.SystemName).Version.ToString();
			}
			catch (Exception) { }

			var config = new OffAmazonPaymentsServiceConfig()
			{
				ServiceURL = (settings.UseSandbox ? AmazonPayCore.UrlApiEuSandbox : AmazonPayCore.UrlApiEuProduction)
			};

			config.SetUserAgent(AmazonPayCore.AppName, appVersion);

			Settings = settings;
			Service = new OffAmazonPaymentsServiceClient(AmazonPayCore.AppName, appVersion, settings.AccessKey, settings.SecretKey, config);
		}

		public IOffAmazonPaymentsService Service { get; private set; }
		public AmazonPaySettings Settings { get; private set; }
	}
}