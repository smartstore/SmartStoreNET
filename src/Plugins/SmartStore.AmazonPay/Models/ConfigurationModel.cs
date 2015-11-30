using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.AmazonPay.Models
{
	public class ConfigurationModel : ModelBase
	{
		public string[] ConfigGroups { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.SellerId")]
		public string SellerId { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AccessKey")]
		public string AccessKey { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.SecretKey")]
		public string SecretKey { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.Marketplace")]
		public string Marketplace { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AmazonButtonColor")]
		public string AmazonButtonColor { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AmazonButtonSize")]
		public string AmazonButtonSize { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.DataFetching")]
		public AmazonPayDataFetchingType DataFetching { get; set; }
		public List<SelectListItem> DataFetchings { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.IpnUrl")]
		public string IpnUrl { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PollingTaskMinutes")]
		public int PollingTaskMinutes { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PollingMaxOrderCreationDays")]
		public int PollingMaxOrderCreationDays { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.TransactionType")]
		public AmazonPayTransactionType TransactionType { get; set; }
		public List<SelectListItem> TransactionTypes { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.SaveEmailAndPhone")]
		public AmazonPaySaveDataType? SaveEmailAndPhone { get; set; }
		public List<SelectListItem> SaveEmailAndPhones { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.ShowButtonInMiniShoppingCart")]
		public bool ShowButtonInMiniShoppingCart { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AddressWidgetWidth")]
		public int AddressWidgetWidth { get; set; }
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AddressWidgetHeight")]
		public int AddressWidgetHeight { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PaymentWidgetWidth")]
		public int PaymentWidgetWidth { get; set; }
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PaymentWidgetHeight")]
		public int PaymentWidgetHeight { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AddOrderNotes")]
		public bool AddOrderNotes { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.InformCustomerAboutErrors")]
		public bool InformCustomerAboutErrors { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.InformCustomerAddErrors")]
		public bool InformCustomerAddErrors { get; set; }

		public void Copy(AmazonPaySettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				UseSandbox = settings.UseSandbox;
				SellerId = settings.SellerId;
				AccessKey = settings.AccessKey;
				SecretKey = settings.SecretKey;
				Marketplace = settings.Marketplace;
				DataFetching = settings.DataFetching;
				PollingMaxOrderCreationDays = settings.PollingMaxOrderCreationDays;
				TransactionType = settings.TransactionType;
				SaveEmailAndPhone = settings.SaveEmailAndPhone;
				ShowButtonInMiniShoppingCart = settings.ShowButtonInMiniShoppingCart;
				AmazonButtonColor = settings.AmazonButtonColor;
				AmazonButtonSize = settings.AmazonButtonSize;
				AddressWidgetWidth = settings.AddressWidgetWidth;
				AddressWidgetHeight = settings.AddressWidgetHeight;
				PaymentWidgetWidth = settings.PaymentWidgetWidth;
				PaymentWidgetHeight = settings.PaymentWidgetHeight;
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;
				AddOrderNotes = settings.AddOrderNotes;
				InformCustomerAboutErrors = settings.InformCustomerAboutErrors;
				InformCustomerAddErrors = settings.InformCustomerAddErrors;
			}
			else
			{
				settings.UseSandbox = UseSandbox;
				settings.SellerId = SellerId;
				settings.AccessKey = AccessKey;
				settings.SecretKey = SecretKey;
				settings.Marketplace = Marketplace;
				settings.DataFetching = DataFetching;
				settings.PollingMaxOrderCreationDays = PollingMaxOrderCreationDays;
				settings.TransactionType = TransactionType;
				settings.SaveEmailAndPhone = SaveEmailAndPhone;
				settings.ShowButtonInMiniShoppingCart = ShowButtonInMiniShoppingCart;
				settings.AmazonButtonColor = AmazonButtonColor;
				settings.AmazonButtonSize = AmazonButtonSize;
				settings.AddressWidgetWidth = AddressWidgetWidth;
				settings.AddressWidgetHeight = AddressWidgetHeight;
				settings.PaymentWidgetWidth = PaymentWidgetWidth;
				settings.PaymentWidgetHeight = PaymentWidgetHeight;
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.AddOrderNotes = AddOrderNotes;
				settings.InformCustomerAboutErrors = InformCustomerAboutErrors;
				settings.InformCustomerAddErrors = InformCustomerAddErrors;
			}
		}
	}
}
