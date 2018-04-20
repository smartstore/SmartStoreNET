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
			AuthorizeMethod = AmazonPayAuthorizeMethod.Omnichronous;
			SaveEmailAndPhone = AmazonPaySaveDataType.OnlyIfEmpty;
			AddOrderNotes = true;
			InformCustomerAboutErrors = true;
			InformCustomerAddErrors = true;
			PollingMaxOrderCreationDays = 31;

			PayButtonColor = "Gold";
			PayButtonSize = "small";
			AuthButtonType = "LwA";
			AuthButtonColor = "Gold";
			AuthButtonSize = "medium";
		}

		public bool UseSandbox { get; set; }

		public string SellerId { get; set; }
		public string AccessKey { get; set; }
		public string SecretKey { get; set; }
		public string ClientId { get; set; }
		public string Marketplace { get; set; }
		
		public AmazonPayDataFetchingType DataFetching { get; set; }
		public AmazonPayTransactionType TransactionType { get; set; }
		public AmazonPayAuthorizeMethod AuthorizeMethod { get; set; }

		public AmazonPaySaveDataType? SaveEmailAndPhone { get; set; }
		public bool ShowPayButtonForAdminOnly { get; set; }
		public bool ShowButtonInMiniShoppingCart { get; set; }

		public int PollingMaxOrderCreationDays { get; set; }

		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }

		public bool AddOrderNotes { get; set; }

		public bool InformCustomerAboutErrors { get; set; }
		public bool InformCustomerAddErrors { get; set; }

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
