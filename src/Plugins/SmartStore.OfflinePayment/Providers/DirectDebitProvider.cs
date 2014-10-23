using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
	[SystemName("Payments.DirectDebit")]
	[FriendlyName("Direct Debit")]
	[DisplayOrder(1)]
	public class DirectDebitProvider : OfflinePaymentProviderBase, IConfigurable
	{
		private readonly DirectDebitPaymentSettings _settings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;

		public DirectDebitProvider(DirectDebitPaymentSettings settings, IOrderTotalCalculationService orderTotalCalculationService)
		{
			this._settings = settings;
			this._orderTotalCalculationService = orderTotalCalculationService;
		}

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, _settings.AdditionalFee, _settings.AdditionalFeePercentage);
			return result;
		}

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();
			result.AllowStoringDirectDebit = true;
			result.NewPaymentStatus = PaymentStatus.Pending;
			return result;
		}

		protected override string GetActionPrefix()
		{
			return "DirectDebit";
		}

		public override bool RequiresInteraction
		{
			get
			{
				return true;
			}
		}

	}
}