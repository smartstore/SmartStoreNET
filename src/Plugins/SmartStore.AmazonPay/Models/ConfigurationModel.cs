using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
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
		[DataType(DataType.Password)]
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

		#region Registration data

		public string RegisterUrl { get; set; }
		public string SoftwareVersion { get; set; }
		public string PluginVersion { get; set; }
		public string LeadCode { get; set; }
		public string PlatformId { get; set; }
		public string PublicKey { get; set; }
		public string KeyShareUrl { get; set; }
		public string LanguageLocale { get; set; }

		/// <summary>
		/// Including all domains and sub domains where the login button appears. SSL mandatory.
		/// </summary>
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.MerchantLoginDomains")]
		public HashSet<string> MerchantLoginDomains { get; set; }
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.MerchantLoginDomains")]
		public HashSet<string> CurrentMerchantLoginDomains { get; set; }

		/// <summary>
		/// Used to populate Allowed Return URLs on the Login with Amazon application. SSL mandatory. Max 512 characters.
		/// </summary>
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.MerchantLoginRedirectUrls")]
		public HashSet<string> MerchantLoginRedirectUrls { get; set; }
		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.MerchantLoginRedirectUrls")]
		public HashSet<string> CurrentMerchantLoginRedirectUrls { get; set; }

		public string MerchantStoreDescription { get; set; }
		public string MerchantPrivacyNoticeUrl { get; set; }
		public string MerchantCountry { get; set; }
		public string MerchantSandboxIpnUrl { get; set; }
		public string MerchantProductionIpnUrl { get; set; }

		#endregion

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
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;
				AddOrderNotes = settings.AddOrderNotes;
				InformCustomerAboutErrors = settings.InformCustomerAboutErrors;
				InformCustomerAddErrors = settings.InformCustomerAddErrors;

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
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.AddOrderNotes = AddOrderNotes;
				settings.InformCustomerAboutErrors = InformCustomerAboutErrors;
				settings.InformCustomerAddErrors = InformCustomerAddErrors;

				settings.PayButtonColor = PayButtonColor;
				settings.PayButtonSize = PayButtonSize;
				settings.AuthButtonType = AuthButtonType;
				settings.AuthButtonColor = AuthButtonColor;
				settings.AuthButtonSize = AuthButtonSize;
			}
		}
	}
}
