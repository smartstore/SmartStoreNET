using SmartStore.AmazonPay.Services;
using SmartStore.Core.Configuration;

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
			AddressWidgetWidth = PaymentWidgetWidth = 400;
			AddressWidgetHeight = PaymentWidgetHeight = 260;
			AddOrderNotes = true;
			UsePopupDialog = true;
			InformCustomerAboutErrors = true;
			InformCustomerAddErrors = true;
			PollingMaxOrderCreationDays = 31;

			PayButtonType = "PwA";
			PayButtonColor = "Gold";
			PayButtonSize = "small";
			AuthButtonType = "Login";
			AuthButtonColor = "Gold";
			AuthButtonSize = "medium";
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

		public int AddressWidgetWidth { get; set; }
		public int AddressWidgetHeight { get; set; }

		public int PaymentWidgetWidth { get; set; }
		public int PaymentWidgetHeight { get; set; }

		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }

		public bool AddOrderNotes { get; set; }
		public bool UsePopupDialog { get; set; }

		public bool InformCustomerAboutErrors { get; set; }
		public bool InformCustomerAddErrors { get; set; }

		public string WidgetUrl
		{
			get
			{
				if (SellerId.IsEmpty())
					return null;

				return UseSandbox
					? "https://static-eu.payments-amazon.com/OffAmazonPayments/eur/sandbox/lpa/js/Widgets.js"
					: "https://static-eu.payments-amazon.com/OffAmazonPayments/eur/lpa/js/Widgets.js";

				//string url = (UseSandbox ? AmazonPayCore.UrlWidgetSandboxOld : AmazonPayCore.UrlWidgetProductionOld);
				//url = url.FormatWith(Marketplace ?? "de");

				//return "{0}?sellerId={1}".FormatWith(
				//	url,
				//	HttpUtility.UrlEncode(SellerId)
				//);
			}
		}

		public string PayButtonType { get; set; }
		public string PayButtonColor { get; set; }
		public string PayButtonSize { get; set; }

		public string AuthButtonType { get; set; }
		public string AuthButtonColor { get; set; }
		public string AuthButtonSize { get; set; }

		//public string GetButtonUrl(AmazonPayRequestType view)
		//{
		//	//bool isGerman = _services.WorkContext.WorkingLanguage.UniqueSeoCode.IsCaseInsensitiveEqual("DE");
		//	string marketplace = Marketplace ?? "de";
		//	if (marketplace.IsCaseInsensitiveEqual("uk"))
		//		marketplace = "co.uk";

		//	string buttonSize = (view == AmazonPayRequestType.MiniShoppingCart ? "large" : AmazonButtonSize);

		//	string url = (UseSandbox ? AmazonPayCore.UrlButtonSandbox : AmazonPayCore.UrlButtonProduction);
		//	url = url.FormatWith(marketplace);

		//	return "{0}?sellerId={1}&size={2}&color={3}".FormatWith(
		//		url,
		//		HttpUtility.UrlEncode(SellerId),
		//		HttpUtility.UrlEncode(buttonSize ?? "x-large"),
		//		HttpUtility.UrlEncode(AmazonButtonColor ?? "orange")
		//	);
		//}

		public bool CanSaveEmailAndPhone(string value)
		{
			return (SaveEmailAndPhone == AmazonPaySaveDataType.Always || (SaveEmailAndPhone == AmazonPaySaveDataType.OnlyIfEmpty && value.IsEmpty()));
		}
	}
}
