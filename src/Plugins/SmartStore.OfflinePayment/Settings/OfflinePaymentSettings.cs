using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;

namespace SmartStore.OfflinePayment.Settings
{
	public abstract class PaymentSettingsBase : ISettings
	{
		public string DescriptionText { get; set; }
		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
	}

	public class CashOnDeliveryPaymentSettings : PaymentSettingsBase, ISettings
	{
	}

	public class DirectDebitPaymentSettings : PaymentSettingsBase, ISettings
	{
	}

	public class InvoicePaymentSettings : PaymentSettingsBase, ISettings
	{
	}

	public class ManualPaymentSettings : PaymentSettingsBase, ISettings
	{
		public TransactMode TransactMode { get; set; }
	}

    public class PurchaseOrderNumberPaymentSettings : PaymentSettingsBase, ISettings
    {
        public TransactMode TransactMode { get; set; }
    }

	public class PayInStorePaymentSettings : PaymentSettingsBase, ISettings
	{
	}

	public class PrepaymentPaymentSettings : PaymentSettingsBase, ISettings
	{
	}

	/// <summary>
	/// Represents manual payment processor transaction mode
	/// </summary>
	public enum TransactMode : int
	{
		/// <summary>
		/// Pending
		/// </summary>
		Pending = 0,
		/// <summary>
		/// Authorize
		/// </summary>
		Authorize = 1,
		/// <summary>
		/// Authorize and capture
		/// </summary>
		AuthorizeAndCapture = 2
	}
}