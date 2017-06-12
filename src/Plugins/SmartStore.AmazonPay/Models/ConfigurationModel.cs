using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

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

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.ClientId")]
		public string ClientId { get; set; }

		//[SmartResourceDisplayName("Plugins.Payments.AmazonPay.ClientSecret")]
		//public string ClientSecret { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.Marketplace")]
		public string Marketplace { get; set; }

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

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.UsePopupDialog")]
		public bool UsePopupDialog { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.InformCustomerAboutErrors")]
		public bool InformCustomerAboutErrors { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.InformCustomerAddErrors")]
		public bool InformCustomerAddErrors { get; set; }


		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PayButtonType")]
		public string PayButtonType { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PayButtonColor")]
		public string PayButtonColor { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.PayButtonSize")]
		public string PayButtonSize { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AuthButtonType")]
		public string AuthButtonType { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AuthButtonColor")]
		public string AuthButtonColor { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AuthButtonSize")]
		public string AuthButtonSize { get; set; }

		public void Copy(AmazonPaySettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				UseSandbox = settings.UseSandbox;
				SellerId = settings.SellerId;
				AccessKey = settings.AccessKey;
				SecretKey = settings.SecretKey;
				ClientId = settings.ClientId;
				//ClientSecret = settings.ClientSecret;
				Marketplace = settings.Marketplace;

				DataFetching = settings.DataFetching;
				PollingMaxOrderCreationDays = settings.PollingMaxOrderCreationDays;
				TransactionType = settings.TransactionType;
				SaveEmailAndPhone = settings.SaveEmailAndPhone;
				ShowButtonInMiniShoppingCart = settings.ShowButtonInMiniShoppingCart;
				AddressWidgetWidth = settings.AddressWidgetWidth;
				AddressWidgetHeight = settings.AddressWidgetHeight;
				PaymentWidgetWidth = settings.PaymentWidgetWidth;
				PaymentWidgetHeight = settings.PaymentWidgetHeight;
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;
				AddOrderNotes = settings.AddOrderNotes;
				UsePopupDialog = settings.UsePopupDialog;
				InformCustomerAboutErrors = settings.InformCustomerAboutErrors;
				InformCustomerAddErrors = settings.InformCustomerAddErrors;

				PayButtonType = settings.PayButtonType;
				PayButtonColor = settings.PayButtonColor;
				PayButtonSize = settings.PayButtonSize;
				AuthButtonType = settings.AuthButtonType;
				AuthButtonColor = settings.AuthButtonColor;
				AuthButtonSize = settings.AuthButtonSize;
			}
			else
			{
				settings.UseSandbox = UseSandbox;
				settings.SellerId = SellerId;
				settings.AccessKey = AccessKey;
				settings.SecretKey = SecretKey;
				settings.ClientId = ClientId;
				//settings.ClientSecret = ClientSecret;
				settings.Marketplace = Marketplace;

				settings.DataFetching = DataFetching;
				settings.PollingMaxOrderCreationDays = PollingMaxOrderCreationDays;
				settings.TransactionType = TransactionType;
				settings.SaveEmailAndPhone = SaveEmailAndPhone;
				settings.ShowButtonInMiniShoppingCart = ShowButtonInMiniShoppingCart;
				settings.AddressWidgetWidth = AddressWidgetWidth;
				settings.AddressWidgetHeight = AddressWidgetHeight;
				settings.PaymentWidgetWidth = PaymentWidgetWidth;
				settings.PaymentWidgetHeight = PaymentWidgetHeight;
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.AddOrderNotes = AddOrderNotes;
				settings.UsePopupDialog = UsePopupDialog;
				settings.InformCustomerAboutErrors = InformCustomerAboutErrors;
				settings.InformCustomerAddErrors = InformCustomerAddErrors;

				settings.PayButtonType = PayButtonType;
				settings.PayButtonColor = PayButtonColor;
				settings.PayButtonSize = PayButtonSize;
				settings.AuthButtonType = AuthButtonType;
				settings.AuthButtonColor = AuthButtonColor;
				settings.AuthButtonSize = AuthButtonSize;
			}
		}
	}
}
