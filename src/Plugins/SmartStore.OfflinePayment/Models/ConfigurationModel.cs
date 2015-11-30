using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.OfflinePayment.Models
{ 
	public abstract class ConfigurationModelBase : ModelBase
    {
		[AllowHtml]
		[SmartResourceDisplayName("Plugins.SmartStore.OfflinePayment.DescriptionText")]
		public string DescriptionText { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.OfflinePayment.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.OfflinePayment.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }

	public class CashOnDeliveryConfigurationModel : ConfigurationModelBase
	{
	}

	public class DirectDebitConfigurationModel : ConfigurationModelBase
	{
	}

	public class InvoiceConfigurationModel : ConfigurationModelBase
	{
	}

	public class ManualConfigurationModel : ConfigurationModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.Manual.Fields.TransactMode")]
		public TransactMode TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }
	}

	public class PayInStoreConfigurationModel : ConfigurationModelBase
	{
	}

	public class PrepaymentConfigurationModel : ConfigurationModelBase
	{
	}

    public class PurchaseOrderNumberConfigurationModel : ConfigurationModelBase
    {
    }
}