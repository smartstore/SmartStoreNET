using System.Globalization;

namespace SmartStore.AmazonPay.Services
{
	public class AmazonPayApiPrice
	{
		public AmazonPayApiPrice()
		{
		}

		public AmazonPayApiPrice(double amount, string currenycCode)
		{
			Amount = amount;
			CurrencyCode = currenycCode;
		}

		public AmazonPayApiPrice(string amount, string currenycCode)
		{
			double d;
			if (amount.HasValue() && double.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
			{
				Amount = d;
			}

			CurrencyCode = currenycCode;
		}

		public double Amount { get; set; }
		public string CurrencyCode { get; set; }

		public override string ToString()
		{
			var str = Amount.ToString("0.00", CultureInfo.InvariantCulture);
			return str.Grow(CurrencyCode, " ");
		}
	}
}