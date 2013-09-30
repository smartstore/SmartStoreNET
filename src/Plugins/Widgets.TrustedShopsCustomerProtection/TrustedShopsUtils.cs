using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection
{
    public static class TrustedShopsUtils
    {
        public static string ConvertPaymentSystemNameToTrustedShopsCode(string paymentSystemName)
        {
            Dictionary<string, string> payments = new Dictionary<string, string>();
            payments.Add("Payments.IPaymentDirectDebit", "DIRECT_DEBIT");
            payments.Add("Payments.DirectDebit", "DIRECT_DEBIT");
            payments.Add("Payments.IPaymentCreditCard", "CREDIT_CARD");
            payments.Add("Payments.Invoice", "INVOICE");
            payments.Add("Payments.CashOnDelivery", "CASH_ON_DELIVERY");
            payments.Add("Payments.CheckMoneyOrder", "CHEQUE");
            payments.Add("Payments.PayPalDirect", "PAYPAL");
            payments.Add("Payments.PayPalStandard", "PAYPAL");
            payments.Add("Payments.Sofortueberweisung", "DIRECT_E_BANKING");
            payments.Add("Payments.PayInStore", "CASH_ON_PICKUP");
            payments.Add("", "OTHER");

            string trustedShopsPaymentCode = "OTHER";

            if (paymentSystemName != null)
            {
                payments.TryGetValue(paymentSystemName, out trustedShopsPaymentCode);
            }
            else {
                trustedShopsPaymentCode = "NOT_CHOOSEN_YET";
            }

            return trustedShopsPaymentCode;
        }

        public static string GetTrustedShopsProductSku(string currency, decimal amount) {

            var tsProductSku = "TS080501_500_30_EUR";

            if (currency == "EUR") {
                if (amount < 20000) tsProductSku = "TS080501_20000_30_EUR";
                if (amount < 10000) tsProductSku = "TS080501_10000_30_EUR";
                if (amount < 5000) tsProductSku = "TS080501_5000_30_EUR";
                if (amount < 2500) tsProductSku = "TS080501_2500_30_EUR";
                if(amount < 1500) tsProductSku = "TS080501_1500_30_EUR";
                if (amount < 500) tsProductSku = "TS080501_500_30_EUR";
            }

            if (currency == "USD")
            {
                if (amount < 20000) tsProductSku = "TS080501_20000_30_USD";
                if (amount < 10000) tsProductSku = "TS080501_10000_30_USD";
                if (amount < 5000) tsProductSku = "TS080501_5000_30_USD";
                if (amount < 2500) tsProductSku = "TS080501_2500_30_USD";
                if (amount < 1500) tsProductSku = "TS080501_1500_30_USD";
                if (amount < 500) tsProductSku = "TS080501_500_30_USD";
            }

            if (currency == "GBP")
            {
                if (amount < 20000) tsProductSku = "TS100629_20000_30_GBP";
                if (amount < 10000) tsProductSku = "TS100629_10000_30_GBP";
                if (amount < 5000) tsProductSku = "TS100629_5000_30_GBP";
                if (amount < 2500) tsProductSku = "TS100629_2500_30_GBP";
                if (amount < 1500) tsProductSku = "TS100629_1500_30_GBP";
                if (amount < 500) tsProductSku = "TS100629_500_30_GBP";
            }

            if (currency == "PLN")
            {
                if (amount < 20000) tsProductSku = "TS100809_20000_30_PLN";
                if (amount < 10000) tsProductSku = "TS100809_10000_30_PLN";
                if (amount < 5000) tsProductSku = "TS100809_5000_30_PLN";
                if (amount < 2500) tsProductSku = "TS100809_2500_30_PLN";
                if (amount < 1500) tsProductSku = "TS100809_1500_30_PLN";
                if (amount < 500) tsProductSku = "TS100809_500_30_PLN";
            }

            return tsProductSku;
        }
    }
}
