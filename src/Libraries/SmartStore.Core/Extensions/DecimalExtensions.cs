using SmartStore.Core.Domain.Directory;
using System;
using System.Globalization;

namespace SmartStore
{
    public static class DecimalExtensions
    {
		/// <summary>
		/// Calculates the tax (percentage) from a gross and a net value.
		/// </summary>
		/// <param name="inclTax">Gross value</param>
		/// <param name="exclTax">Net value</param>
		/// <param name="decimals">Rounding decimal number</param>
		/// <returns>Tax percentage</returns>
		public static decimal ToTaxPercentage(this decimal inclTax, decimal exclTax, int? decimals = null)
		{
			if (exclTax == decimal.Zero)
			{
				return decimal.Zero;
			}

			var result = ((inclTax / exclTax) - 1.0M) * 100.0M;

			return (decimals.HasValue ? Math.Round(result, decimals.Value) : result);
		}

		/// <summary>
		/// Converts to smallest currency uint, e.g. cents
		/// </summary>
		/// <returns>Smallest currency unit</returns>
		public static int ToSmallestCurrencyUnit(this decimal value, MidpointRounding rounding = MidpointRounding.AwayFromZero)
		{
			var result = Math.Round(value * 100, 0, rounding);
			return Convert.ToInt32(result);
		}

		//public static decimal RoundToNearest(this decimal value, decimal nearest, bool roundUp)
		//{
		//    if (nearest == decimal.Zero)
		//    {
		//        return value;
		//    }

		//    if (roundUp)
		//    {
		//        return Math.Ceiling(value / nearest) * nearest;
		//    }
		//    else
		//    {
		//        return Math.Floor(value / nearest) * nearest;
		//    }
		//}

		/// <summary>
		/// Rounds and formats a decimal culture invariant
		/// </summary>
		/// <param name="value">Decimal to round</param>
		/// <param name="decimals">Rounding decimal number</param>
		/// <returns>Rounded and formated value</returns>
		public static string FormatInvariant(this decimal value, int decimals = 2)
		{
			return Math.Round(value, decimals).ToString("0.00", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Rounds and formats a currency value culture invariant
		/// </summary>
		/// <param name="value">Value to round</param>
		/// <param name="currency">Rounding method providing currency</param>
		/// <returns>Rounded and formated value</returns>
		public static string FormatInvariant(this decimal value, Currency currency)
		{
			Guard.NotNull(currency, nameof(currency));

			var result = value
				.Round(currency.RoundingMethod)
				.ToString("0.00", CultureInfo.InvariantCulture);

			return result;
		}

		/// <summary>
		/// Rounds a currency value
		/// </summary>
		/// <param name="value">Value to round</param>
		/// <param name="currency">Rounding method providing currency</param>
		/// <returns>Rounded value</returns>
		public static decimal Round(this decimal value, Currency currency)
		{
			Guard.NotNull(currency, nameof(currency));

			return value.Round(currency.RoundingMethod);
		}

		/// <summary>
		/// Rounds a currency value
		/// </summary>
		/// <param name="value">Value to round</param>
		/// <param name="method">Currency rounding method</param>
		/// <returns>Rounded value</returns>
		/// <see cref="https://en.wikipedia.org/wiki/Cash_rounding"/>
		public static decimal Round(this decimal value, CurrencyRoundingMethod method)
        {
            var result = Math.Round(value, 2);
			// TODO: where must be rounded and where not?
			//var frac = (result - Math.Truncate(result)) * 10;

			//if (frac == decimal.Zero)
			//    return result;

			//switch (method)
			//{
			//    case CurrencyRoundingMethod.Down005:
			//    case CurrencyRoundingMethod.Up005:
			//        frac = (frac - Math.Truncate(frac)) * 10;

			//        if (method == CurrencyRoundingMethod.Down005)
			//        {
			//            frac = frac < 5 ? -1 * frac : 5 - frac;
			//        }
			//        else
			//        {
			//            frac = (frac < 5 ? 5 : 10) - frac;
			//        }

			//        result += frac / 100;
			//        break;

			//    case CurrencyRoundingMethod.Down01:
			//    case CurrencyRoundingMethod.Up01:
			//        frac = (frac - Math.Truncate(frac)) * 10;

			//        if (method == CurrencyRoundingMethod.Down01 && frac == 5)
			//        {
			//            frac = -5;
			//        }
			//        else
			//        {
			//            frac = frac < 5 ? -1 * frac : 10 - frac;
			//        }

			//        result += frac / 100;
			//        break;

			//    case CurrencyRoundingMethod.Interval05:
			//        frac *= 10;

			//        if (frac < 25)
			//        {
			//            frac *= -1;
			//        }
			//        else
			//        {
			//            frac = (frac < 50 || frac < 75 ? 50 : 100) - frac;
			//        }

			//        result += frac / 100;
			//        break;

			//    case CurrencyRoundingMethod.Interval1:
			//    case CurrencyRoundingMethod.Up1:
			//        frac *= 10;

			//        if (method == CurrencyRoundingMethod.Up1 && frac > 0)
			//        {
			//            result = Math.Truncate(result) + 1;
			//        }
			//        else
			//        {
			//            result = frac < 50 ? Math.Truncate(result) : Math.Truncate(result) + 1;
			//        }
			//        break;
			//}

			return result;
        }
    }
}
