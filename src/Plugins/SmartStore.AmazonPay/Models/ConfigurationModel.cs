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
		public string PrimaryStoreCurrencyCode { get; set; }

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

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.AuthorizeMethod")]
		public AmazonPayAuthorizeMethod AuthorizeMethod { get; set; }
		public SelectList AuthorizeMethods { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.SaveEmailAndPhone")]
		public AmazonPaySaveDataType? SaveEmailAndPhone { get; set; }
		public List<SelectListItem> SaveEmailAndPhones { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.ShowPayButtonForAdminOnly")]
		public bool ShowPayButtonForAdminOnly { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.AmazonPay.ShowButtonInMiniShoppingCart")]
		public bool ShowButtonInMiniShoppingCart { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AdditionalFeePercentage")]
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
	}
}
