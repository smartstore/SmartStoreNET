using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.ComponentModel.DataAnnotations;

namespace SmartStore.OfflinePayment.Models
{ 
	public abstract class ConfigurationModelBase : ModelBase
    {
		public string PrimaryStoreCurrencyCode { get; set; }

		[AllowHtml]
		[SmartResourceDisplayName("Plugins.SmartStore.OfflinePayment.DescriptionText")]
		public string DescriptionText { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.OfflinePayment.PaymentMethodLogo")]
		[UIHint("Picture")]
		public int PaymentMethodLogo { get; set; }
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