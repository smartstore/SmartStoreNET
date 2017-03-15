using System.Web;
using SmartStore.Core.Configuration;
using SmartStore.AmazonPay.Services;

namespace SmartStore.AmazonPay.Settings
{
	public class AmazonPaySettings : ISettings
	{
		public AmazonPaySettings()
		{
			Marketplace = "de";
			DataFetching = AmazonPayDataFetchingType.Ipn;
			TransactionType = AmazonPayTransactionType.Authorize;
			SaveEmailAndPhone = AmazonPaySaveDataType.OnlyIfEmpty;
			AmazonButtonColor = "orange";
			AmazonButtonSize = "x-large";
			AddressWidgetWidth = PaymentWidgetWidth = 400;
			AddressWidgetHeight = PaymentWidgetHeight = 260;
			AddOrderNotes = true;
			InformCustomerAboutErrors = true;
			InformCustomerAddErrors = true;
			PollingMaxOrderCreationDays = 31;
		}

		public bool UseSandbox { get; set; }

		public string SellerId { get; set; }
		public string AccessKey { get; set; }
		public string SecretKey { get; set; }
		public string Marketplace { get; set; }
		
		public AmazonPayDataFetchingType DataFetching { get; set; }
		public AmazonPayTransactionType TransactionType { get; set; }

		public AmazonPaySaveDataType? SaveEmailAndPhone { get; set; }
		public bool ShowButtonInMiniShoppingCart { get; set; }

		public int PollingMaxOrderCreationDays { get; set; }

		public string AmazonButtonColor { get; set; }
		public string AmazonButtonSize { get; set; }

		public int AddressWidgetWidth { get; set; }
		public int AddressWidgetHeight { get; set; }

		public int PaymentWidgetWidth { get; set; }
		public int PaymentWidgetHeight { get; set; }

		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }

		public bool AddOrderNotes { get; set; }

		public bool InformCustomerAboutErrors { get; set; }
		public bool InformCustomerAddErrors { get; set; }

		public string GetApiUrl()
		{
			return (UseSandbox ? AmazonPayCore.UrlApiEuSandbox : AmazonPayCore.UrlApiEuProduction);
		}

		public string GetWidgetUrl()
		{
			if (SellerId.IsEmpty())
				return null;

			string url = (UseSandbox ? AmazonPayCore.UrlWidgetSandbox : AmazonPayCore.UrlWidgetProduction);
			url = url.FormatWith(Marketplace ?? "de");

			return "{0}?sellerId={1}".FormatWith(
				url,
				HttpUtility.UrlEncode(SellerId)
			);
		}

		public string GetButtonUrl(AmazonPayRequestType view)
		{
			//bool isGerman = _services.WorkContext.WorkingLanguage.UniqueSeoCode.IsCaseInsensitiveEqual("DE");
			string marketplace = Marketplace ?? "de";
			if (marketplace.IsCaseInsensitiveEqual("uk"))
				marketplace = "co.uk";

			string buttonSize = (view == AmazonPayRequestType.MiniShoppingCart ? "large" : AmazonButtonSize);

			string url = (UseSandbox ? AmazonPayCore.UrlButtonSandbox : AmazonPayCore.UrlButtonProduction);
			url = url.FormatWith(marketplace);

			return "{0}?sellerId={1}&size={2}&color={3}".FormatWith(
				url,
				HttpUtility.UrlEncode(SellerId),
				HttpUtility.UrlEncode(buttonSize ?? "x-large"),
				HttpUtility.UrlEncode(AmazonButtonColor ?? "orange")
			);
		}

		public bool CanSaveEmailAndPhone(string value)
		{
			return (
				SaveEmailAndPhone == AmazonPaySaveDataType.Always || (SaveEmailAndPhone == AmazonPaySaveDataType.OnlyIfEmpty && value.IsEmpty())
			);
		}
	}
}
