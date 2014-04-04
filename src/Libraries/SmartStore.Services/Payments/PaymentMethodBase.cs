using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Payments
{

	/// <summary>
	/// Base class for payment methods
	/// </summary>
	public abstract class PaymentMethodBase : BasePlugin, IPaymentMethod
	{

		#region Methods

		/// <summary>
		/// Process payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public abstract ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

		/// <summary>
		/// Post process payment (used by payment gateways that require redirecting to a third-party URL)
		/// </summary>
		/// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
		public virtual void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			// NoImpl
		}

		/// <summary>
		/// Gets additional handling fee
		/// </summary>
		/// <returns>Additional handling fee</returns>
		public virtual decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			return 0;
		}

		/// <summary>
		/// Captures payment
		/// </summary>
		/// <param name="capturePaymentRequest">Capture payment request</param>
		/// <returns>Capture payment result</returns>
		public virtual CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
		{
			throw Error.NotSupported();
		}

		/// <summary>
		/// Refunds a payment
		/// </summary>
		/// <param name="refundPaymentRequest">Request</param>
		/// <returns>Result</returns>
		public virtual RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
		{
			throw Error.NotSupported();
		}

		/// <summary>
		/// Voids a payment
		/// </summary>
		/// <param name="voidPaymentRequest">Request</param>
		/// <returns>Result</returns>
		public virtual VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
		{
			throw Error.NotSupported();
		}

		/// <summary>
		/// Process recurring payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public virtual ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
		{
			throw Error.NotSupported();
		}

		/// <summary>
		/// Cancels a recurring payment
		/// </summary>
		/// <param name="cancelPaymentRequest">Request</param>
		/// <returns>Result</returns>
		public virtual CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
		{
			throw Error.NotSupported();
		}

		/// <summary>
		/// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>Result</returns>
		public virtual bool CanRePostProcessPayment(Order order)
		{
			return false;
		}

		/// <summary>
		/// Gets a route for provider configuration
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public abstract void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
		public RouteInfo GetConfigurationRoute()
		{
			string action;
			string controller;
			RouteValueDictionary routeValues;

			this.GetConfigurationRoute(out action, out controller, out routeValues);

			return new RouteInfo(action, controller, routeValues);
		}

		/// <summary>
		/// Gets a route for payment info
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public abstract void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
		public RouteInfo GetPaymentInfoRoute()
		{
			string action;
			string controller;
			RouteValueDictionary routeValues;

			this.GetPaymentInfoRoute(out action, out controller, out routeValues);

			return new RouteInfo(action, controller, routeValues);
		}

		/// <summary>
		/// Gets a route for the payment info handler controller action
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>Result</returns>
		/// <remarks>
		/// The defined route is being redirected to during the checkout process > PaymentInfo page.
		/// Implementors should return <c>null</c> if no redirection occurs.
		/// </remarks>
		public virtual RouteInfo GetPaymentInfoHandlerRoute()
		{
			return null;
		}

		public abstract Type GetControllerType();

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether capture is supported
		/// </summary>
		public virtual bool SupportCapture
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether partial refund is supported
		/// </summary>
		public virtual bool SupportPartiallyRefund
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether refund is supported
		/// </summary>
		public virtual bool SupportRefund
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether void is supported
		/// </summary>
		public virtual bool SupportVoid
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a recurring payment type of payment method
		/// </summary>
		public virtual RecurringPaymentType RecurringPaymentType
		{
			get
			{
				return RecurringPaymentType.NotSupported;
			}
		}

		/// <summary>
		/// Gets a payment method type
		/// </summary>
		public virtual PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Unknown;
			}
		}

		#endregion

	}

}
