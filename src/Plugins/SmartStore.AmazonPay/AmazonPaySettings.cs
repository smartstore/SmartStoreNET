using SmartStore.AmazonPay.Services;
using SmartStore.Core.Configuration;

namespace SmartStore.AmazonPay
{
	public class AmazonPaySettings : ISettings
	{
		public AmazonPaySettings()
		{
			Marketplace = "de";
			DataFetching = AmazonPayDataFetchingType.Ipn;
			TransactionType = AmazonPayTransactionType.Authorize;
			SaveEmailAndPhone = AmazonPaySaveDataType.OnlyIfEmpty;
			AddOrderNotes = true;
			InformCustomerAboutErrors = true;
			InformCustomerAddErrors = true;
			PollingMaxOrderCreationDays = 31;

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
		public string ClientId { get; set; }
		//public string ClientSecret { get; set; }
		public string Marketplace { get; set; }
		
		public AmazonPayDataFetchingType DataFetching { get; set; }
		public AmazonPayTransactionType TransactionType { get; set; }

		public AmazonPaySaveDataType? SaveEmailAndPhone { get; set; }
		public bool ShowButtonInMiniShoppingCart { get; set; }

		public int PollingMaxOrderCreationDays { get; set; }

		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }

		public bool AddOrderNotes { get; set; }

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

		public string PayButtonColor { get; set; }
		public string PayButtonSize { get; set; }

		public string AuthButtonType { get; set; }
		public string AuthButtonColor { get; set; }
		public string AuthButtonSize { get; set; }

		public bool CanSaveEmailAndPhone(string value)
		{
			return (SaveEmailAndPhone == AmazonPaySaveDataType.Always || (SaveEmailAndPhone == AmazonPaySaveDataType.OnlyIfEmpty && value.IsEmpty()));
		}
	}
}
