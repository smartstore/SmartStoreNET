using System;
using System.Globalization;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Directory;

namespace SmartStore
{
    public class Money : IConvertible, IFormattable, IComparable, IComparable<Money>, IEquatable<Money>
    {
        public Money(Currency currency)
            : this(0m, currency)
        {
        }

        public Money(float amount, Currency currency)
            : this((decimal)amount, currency, false)
        {
        }

        public Money(double amount, Currency currency)
            : this((decimal)amount, currency, false)
        {
        }

        public Money(decimal amount, Currency currency)
            : this(amount, currency, false)
        {
        }

        public Money(decimal amount, Currency currency, bool hideCurrency)
        {
            Guard.NotNull(currency, nameof(currency));

            Amount = amount;
            Currency = currency;
            HideCurrency = hideCurrency;
        }

        [IgnoreDataMember]
        public bool HideCurrency
        {
            get;
            internal set;
        }

        [IgnoreDataMember]
        public Currency Currency
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of decimal digits for the associated currency.
        /// </summary>
        public int DecimalDigits => string.Equals(Currency?.CurrencyCode, "btc", StringComparison.OrdinalIgnoreCase) ? 8 : Currency.NumberFormat.CurrencyDecimalDigits;

        /// <summary>
        /// The internal unrounded raw amount
        /// </summary>
        public decimal Amount
        {
            get;
            set;
        }

        /// <summary>
        /// Rounds the amount to the number of significant decimal digits
        /// of the associated currency using MidpointRounding.AwayFromZero.
        /// </summary>
        public decimal RoundedAmount => decimal.Round(Amount, DecimalDigits);

        /// <summary>
        /// Truncates the amount to the number of significant decimal digits
        /// of the associated currency.
        /// </summary>
        public decimal TruncatedAmount => (decimal)((long)Math.Truncate(Amount * DecimalDigits)) / DecimalDigits;

        /// <summary>
        /// The formatted amount
        /// </summary>
        public string Formatted => ToString(true, false);

        private static void GuardCurrenciesAreEqual(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot operate on money values with different currencies.");
        }

        #region Compare

        public override int GetHashCode()
        {
            if (Amount == 0)
                return 0;

            return Amount.GetHashCode() ^ Currency.GetHashCode();
        }

        public int CompareTo(Money other)
        {
            return ((IComparable)this).CompareTo(other);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null || !(obj is Money))
                return 1;

            Money other = (Money)obj;

            if (this.Amount == other.Amount)
                return 0;
            if (this.Amount < other.Amount)
                return -1;

            return 1;
        }

        public override bool Equals(object obj)
        {
            // Prevent stack overflow.
            if (obj != null)
            {
                return Equals(obj as Money);
            }

            return false;
        }

        bool IEquatable<Money>.Equals(Money other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.Amount == 0 && this.Amount == 0)
                return true;

            return other.Amount == this.Amount && other.Currency == this.Currency;
        }

        public static bool operator ==(Money a, Money b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Money a, Money b)
        {
            return !a.Equals(b);
        }

        public static bool operator >(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return a.Amount > b.Amount;
        }

        public static bool operator <(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return a.Amount < b.Amount;
        }

        public static bool operator <=(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return a.Amount <= b.Amount;

        }

        public static bool operator >=(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return a.Amount >= b.Amount;

        }

        public static bool operator ==(Money a, int b)
        {
            return a.Amount == b;
        }

        public static bool operator !=(Money a, int b)
        {
            return a.Amount != b;
        }

        public static bool operator >(Money a, int b)
        {
            return a.Amount > b;
        }

        public static bool operator <(Money a, int b)
        {
            return a.Amount < b;
        }

        public static bool operator <=(Money a, int b)
        {
            return a.Amount <= b;
        }

        public static bool operator >=(Money a, int b)
        {
            return a.Amount >= b;
        }

        public static bool operator ==(Money a, float b)
        {
            return a.Amount == (decimal)b;
        }

        public static bool operator !=(Money a, float b)
        {
            return a.Amount != (decimal)b;
        }

        public static bool operator >(Money a, float b)
        {
            return a.Amount > (decimal)b;
        }

        public static bool operator <(Money a, float b)
        {
            return a.Amount < (decimal)b;
        }

        public static bool operator <=(Money a, float b)
        {
            return a.Amount <= (decimal)b;
        }

        public static bool operator >=(Money a, float b)
        {
            return a.Amount >= (decimal)b;
        }

        public static bool operator ==(Money a, double b)
        {
            return a.Amount == (decimal)b;
        }

        public static bool operator !=(Money a, double b)
        {
            return a.Amount != (decimal)b;
        }

        public static bool operator >(Money a, double b)
        {
            return a.Amount > (decimal)b;
        }

        public static bool operator <(Money a, double b)
        {
            return a.Amount < (decimal)b;
        }

        public static bool operator <=(Money a, double b)
        {
            return a.Amount <= (decimal)b;
        }

        public static bool operator >=(Money a, double b)
        {
            return a.Amount >= (decimal)b;
        }

        public static bool operator ==(Money a, decimal b)
        {
            return a.Amount == b;
        }

        public static bool operator !=(Money a, decimal b)
        {
            return a.Amount != b;
        }

        public static bool operator >(Money a, decimal b)
        {
            return a.Amount > b;
        }

        public static bool operator <(Money a, decimal b)
        {
            return a.Amount < b;
        }

        public static bool operator <=(Money a, decimal b)
        {
            return a.Amount <= b;
        }

        public static bool operator >=(Money a, decimal b)
        {
            return a.Amount >= b;
        }

        #endregion

        #region Format

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return this.ToString(!HideCurrency, false);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.ToString(!HideCurrency, false);
        }

        public override string ToString()
        {
            return this.ToString(!HideCurrency, false);
        }

        public string ToString(bool showCurrency)
        {
            return this.ToString(showCurrency, false);
        }

        public string ToString(bool showCurrency, bool useISOCodeAsSymbol)
        {
            var fmt = Currency.NumberFormat;

            if (Currency.CustomFormatting.HasValue())
            {
                return RoundedAmount.ToString(Currency.CustomFormatting, fmt);
            }
            else
            {
                if (!showCurrency || useISOCodeAsSymbol)
                {
                    fmt = (NumberFormatInfo)Currency.NumberFormat.Clone();
                    fmt.CurrencySymbol = !showCurrency ? "" : Currency.CurrencyCode;
                }

                return RoundedAmount.ToString("C", fmt);
            }
        }

        #endregion

        #region Convert

        // For truthy checks in templating
        public static explicit operator bool(Money money)
        {
            return money.Amount != 0;
        }

        public static explicit operator string(Money money)
        {
            return money.ToString(true, false);
        }

        public static explicit operator byte(Money money)
        {
            return System.Convert.ToByte(money.Amount);
        }

        public static explicit operator decimal(Money money)
        {
            return money.Amount;
        }

        public static explicit operator double(Money money)
        {
            return System.Convert.ToDouble(money.Amount);
        }

        public static explicit operator float(Money money)
        {
            return System.Convert.ToSingle(money.Amount);
        }

        public static explicit operator int(Money money)
        {
            return System.Convert.ToInt32(money.Amount);
        }

        public static explicit operator long(Money money)
        {
            return System.Convert.ToInt64(money.Amount);
        }

        public static explicit operator sbyte(Money money)
        {
            return System.Convert.ToSByte(money.Amount);
        }

        public static explicit operator short(Money money)
        {
            return System.Convert.ToInt16(money.Amount);
        }

        public static explicit operator ushort(Money money)
        {
            return System.Convert.ToUInt16(money.Amount);
        }

        public static explicit operator uint(Money money)
        {
            return System.Convert.ToUInt32(money.Amount);
        }

        public static explicit operator ulong(Money money)
        {
            return System.Convert.ToUInt64(money.Amount);
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return System.Convert.ChangeType(this.Amount, conversionType, provider);
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Amount != 0;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw Error.InvalidCast(typeof(Money), typeof(char));
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw Error.InvalidCast(typeof(Money), typeof(DateTime));
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return (byte)this.Amount;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.Amount;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return (double)this.Amount;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return (short)this.Amount;
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return (int)this.Amount;
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return (long)this.Amount;
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return (sbyte)this.Amount;
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (float)this.Amount;
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return (ushort)this.Amount;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return (uint)this.Amount;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return (ulong)this.Amount;
        }

        #endregion

        #region Add

        public static Money operator ++(Money a)
        {
            a.Amount++;
            return a;
        }

        public static Money operator +(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator +(Money a, int b)
        {
            return a + (decimal)b;
        }

        public static Money operator +(Money a, float b)
        {
            return a + (decimal)b;
        }

        public static Money operator +(Money a, double b)
        {
            return a + (decimal)b;
        }

        public static Money operator +(Money a, decimal b)
        {
            return new Money(a.Amount + b, a.Currency);
        }

        #endregion

        #region Substract

        public static Money operator --(Money a)
        {
            a.Amount--;
            return a;
        }

        public static Money operator -(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator -(Money a, int b)
        {
            return a + (decimal)b;
        }

        public static Money operator -(Money a, float b)
        {
            return a + (decimal)b;
        }

        public static Money operator -(Money a, double b)
        {
            return a + (decimal)b;
        }

        public static Money operator -(Money a, decimal b)
        {
            return new Money(a.Amount - b, a.Currency);
        }

        #endregion

        #region Multiply

        public static Money operator *(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator *(Money a, int b)
        {
            return a * (decimal)b;
        }

        public static Money operator *(Money a, float b)
        {
            return a * (decimal)b;
        }

        public static Money operator *(Money a, double b)
        {
            return a * (decimal)b;
        }

        public static Money operator *(Money a, decimal b)
        {
            return new Money(a.Amount * b, a.Currency);
        }

        #endregion

        #region Divide

        public static Money operator /(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Amount / b.Amount, a.Currency);
        }

        public static Money operator /(Money a, int b)
        {
            return a / (decimal)b;
        }

        public static Money operator /(Money a, float b)
        {
            return a / (decimal)b;
        }

        public static Money operator /(Money a, double b)
        {
            return a / (decimal)b;
        }

        public static Money operator /(Money a, decimal b)
        {
            return new Money(a.Amount / b, a.Currency);
        }

        #endregion

        #region Exchange & Math

        ///// <summary>
        ///// Rounds the amount if enabled for the currency or if <paramref name="force"/> is <c>true</c>
        ///// </summary>
        ///// <param name="force">Round also if disabled for the currency</param>
        ///// <returns>A new instance with the rounded amount</returns>
        //public Money Round(bool force = false)
        //{

        //}

        public Money ConvertTo(Currency toCurrency)
        {
            if (Currency == toCurrency)
                return this;

            return new Money((Amount * Currency.Rate) / toCurrency.Rate, toCurrency);
        }

        /// <summary>
        /// Evenly distributes the amount over n parts, resolving remainders that occur due to rounding 
        /// errors, thereby garuanteeing the postcondition: result->sum(r|r.amount) = this.amount and
        /// x elements in result are greater than at least one of the other elements, where x = amount mod n.
        /// </summary>
        /// <param name="n">Number of parts over which the amount is to be distibuted.</param>
        /// <returns>Array with distributed Money amounts.</returns>
        public Money[] Allocate(int n)
        {
            var cents = Math.Pow(10, DecimalDigits);
            var lowResult = ((long)Math.Truncate((double)Amount / n * cents)) / cents;
            var highResult = lowResult + 1.0d / cents;
            var results = new Money[n];
            var remainder = (int)(((double)Amount * cents) % n);

            for (var i = 0; i < remainder; i++)
                results[i] = new Money((decimal)highResult, Currency);

            for (var i = remainder; i < n; i++)
                results[i] = new Money((decimal)lowResult, Currency);

            return results;
        }

        /// <summary>
        /// Gets the ratio of one money to another.
        /// </summary>
        /// <param name="numerator">The numerator of the operation.</param>
        /// <param name="denominator">The denominator of the operation.</param>
        /// <returns>A decimal from 0.0 to 1.0 of the ratio between the two money values.</returns>
        public static decimal GetRatio(Money numerator, Money denominator)
        {
            if (numerator == 0)
                return 0;

            if (denominator == 0)
                throw new DivideByZeroException("Attempted to divide by zero!");

            GuardCurrenciesAreEqual(numerator, denominator);

            return numerator.Amount / denominator.Amount;
        }

        /// <summary>
        /// Gets the smallest money, given the two values.
        /// </summary>
        /// <param name="m1">The first money to compare.</param>
        /// <param name="m2">The second money to compare.</param>
        /// <returns>The smallest money value of the arguments.</returns>
        public static Money Min(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);

            if (a == b)
                return a;
            else if (a > b)
                return b;
            else
                return a;
        }

        /// <summary>
        /// Gets the largest money, given the two values.
        /// </summary>
        /// <param name="m1">The first money to compare.</param>
        /// <param name="m2">The second money to compare.</param>
        /// <returns>The largest money value of the arguments.</returns>
        public static Money Max(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);

            if (a == b)
                return a;
            else if (a > b)
                return a;
            else
                return b;
        }

        /// <summary>
        /// Gets the absolute value of the <see cref="Money"/>.
        /// </summary>
        /// <param name="value">The value of money to convert.</param>
        /// <returns>The money value as an absolute value.</returns>
        public static Money Abs(Money value)
        {
            return new Money(Math.Abs(value.Amount), value.Currency);
        }

        #endregion
    }
}
