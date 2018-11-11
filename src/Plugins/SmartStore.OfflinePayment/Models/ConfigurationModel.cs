using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

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
		public List<SelectListItem> TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Manual.ExcludedCreditCards")]
		public string[] ExcludedCreditCards { get; set; }
		public List<SelectListItem> AvailableCreditCards { get; set; }
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