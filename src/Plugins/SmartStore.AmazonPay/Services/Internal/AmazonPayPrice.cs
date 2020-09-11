using System.Globalization;

namespace SmartStore.AmazonPay.Services.Internal
{
    internal class AmazonPayPrice
    {
        public AmazonPayPrice(decimal amount, string currencyCode)
        {
            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public decimal Amount { get; private set; }
        public string CurrencyCode { get; private set; }

        public override string ToString()
        {
            var str = Amount.ToString("0.00", CultureInfo.InvariantCulture);
            return str.Grow(CurrencyCode, " ");
        }
    }
}