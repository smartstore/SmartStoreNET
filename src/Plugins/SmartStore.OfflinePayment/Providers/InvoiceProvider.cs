using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
	[SystemName("Payments.Invoice")]
	[FriendlyName("Invoice")]
	[DisplayOrder(1)]
	public class InvoiceProvider : OfflinePaymentProviderBase, IConfigurable
	{
		private readonly InvoicePaymentSettings _settings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;

		public InvoiceProvider(InvoicePaymentSettings settings, IOrderTotalCalculationService orderTotalCalculationService)
		{
			this._settings = settings;
			this._orderTotalCalculationService = orderTotalCalculationService;
		}

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, _settings.AdditionalFee, _settings.AdditionalFeePercentage);
			return result;
		}

		protected override string GetActionPrefix()
		{
			return "Invoice";
		}

	}
}