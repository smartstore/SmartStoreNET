using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.OfflinePayment.Models
{
	public abstract class PaymentInfoModelBase : ModelBase
    {
		public string DescriptionText { get; set; }
		public string ThumbnailUrl { get; set; }
	}

	public class CashOnDeliveryPaymentInfoModel : PaymentInfoModelBase
	{
	}

	public class DirectDebitPaymentInfoModel : PaymentInfoModelBase
	{
		public DirectDebitPaymentInfoModel()
		{
			EnterIBAN = "iban";
		}
		
		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.EnterIBAN")]
		public string EnterIBAN { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitAccountHolder")]
		public string DirectDebitAccountHolder { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitAccountNumber")]
		public string DirectDebitAccountNumber { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBankCode")]
		public string DirectDebitBankCode { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitCountry")]
		public string DirectDebitCountry { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBankName")]
		public string DirectDebitBankName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitIban")]
		public string DirectDebitIban { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.DirectDebitBic")]
		public string DirectDebitBic { get; set; }

		public List<SelectListItem> Countries { get; set; }
	}

	public class InvoicePaymentInfoModel : PaymentInfoModelBase
	{
	}

	public class ManualPaymentInfoModel : PaymentInfoModelBase
	{
		public ManualPaymentInfoModel()
        {
            CreditCardTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }
		
		[SmartResourceDisplayName("Payment.SelectCreditCard")]
		[AllowHtml]
		public string CreditCardType { get; set; }
		[SmartResourceDisplayName("Payment.SelectCreditCard")]
		public IList<SelectListItem> CreditCardTypes { get; set; }

		[SmartResourceDisplayName("Payment.CardholderName")]
		[AllowHtml]
		public string CardholderName { get; set; }

		[SmartResourceDisplayName("Payment.CardNumber")]
		[AllowHtml]
		public string CardNumber { get; set; }

		[SmartResourceDisplayName("Payment.ExpirationDate")]
		[AllowHtml]
		public string ExpireMonth { get; set; }
		[SmartResourceDisplayName("Payment.ExpirationDate")]
		[AllowHtml]
		public string ExpireYear { get; set; }
		public IList<SelectListItem> ExpireMonths { get; set; }
		public IList<SelectListItem> ExpireYears { get; set; }

		[SmartResourceDisplayName("Payment.CardCode")]
		[AllowHtml]
		public string CardCode { get; set; }
	}

	public class PayInStorePaymentInfoModel : PaymentInfoModelBase
	{
	}

	public class PrepaymentPaymentInfoModel : PaymentInfoModelBase
	{
	}

    public class PurchaseOrderNumberPaymentInfoModel : PaymentInfoModelBase
	{
        [SmartResourceDisplayName("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber")]
        [AllowHtml]
        public string PurchaseOrderNumber { get; set; }
	}
}