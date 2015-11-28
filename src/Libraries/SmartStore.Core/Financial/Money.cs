using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SmartStore.Financial
{

    [Serializable]
    public struct Money : IComparable, IComparable<Money>, IFormattable, IConvertible
    {
        #region Fields

        public static readonly Money Zero = new Money(0, Currency.Empty);

        #endregion

        #region Ctor

        public Money(decimal amount)
            : this(amount, Currency.Default)
        {
        }

        public Money(decimal amount, string currencyISOCode)
            : this(amount, Currency.GetCurrency(currencyISOCode))
        {
        }

        //public Money(decimal amount, int currencyCode)
        //{
        //    // TODO
        //}

        public Money(decimal amount, Currency currency)
            : this()
        {
            this.Value = amount;
            this.Currency = currency;
        }

        public Money(Money money)
            : this()
        {
            this.Value = money.Value;
            this.Currency = money.Currency;
        }

        #endregion

        #region Properties

        public decimal Value { get; private set; }

        public Currency Currency { get; private set; }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            if (this.Value == 0)
                return 0;

            return this.Value.GetHashCode() | this.Currency.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(Money))
                return false;

            Money other = (Money)obj;

            if (other.Value == 0 && this.Value == 0)
                return true;

            return other.Value == this.Value && other.Currency == this.Currency;
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

            if (this.Value == other.Value)
                return 0;
            if (this.Value < other.Value)
                return -1;
            return 1;
        }

        private static void GuardCurrenciesAreEqual(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot operate on money values with different currencies.");
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns a string-representation of the money value.
        /// </summary>
        /// <returns>The string value of the money.</returns>
        public override string ToString()
        {
            return this.ToString("C", null, false);
        }

        public string ToString(bool useISOCodeAsSymbol)
        {
            return this.ToString("C", null, useISOCodeAsSymbol);
        }

        public string ToString(string format)
        {
            return this.ToString(format, null, false);
        }

        public string ToString(string format, bool useISOCodeAsSymbol)
        {
            return this.ToString(format, null, useISOCodeAsSymbol);
        }

        public string ToString(IFormatProvider provider)
        {
            return this.ToString("C", provider, false);
        }

        public string ToString(IFormatProvider provider, bool useISOCodeAsSymbol)
        {
            return this.ToString("C", provider, useISOCodeAsSymbol);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return this.ToString(format, provider, false);
        }

        public string ToString(string format, IFormatProvider provider, bool useISOCodeAsSymbol)
        {
            // REVIEW: was ist mit Digits, Groups, Decimal usw. Müssen wir die nicht von DB oder sonstwo beziehen?

            Guard.ArgumentNotEmpty(format, "format");

            NumberFormatInfo info = null;

            CultureInfo ci;

            if (provider == null)
                provider = CultureInfo.GetCultureInfo(this.Currency.Region.Name).NumberFormat;

            info = provider as NumberFormatInfo;
            if (info == null)
            {
                ci = provider as CultureInfo;
                if (ci != null)
                    info = ci.NumberFormat;
            }

            if (info != null)
            {
                info = (NumberFormatInfo)info.Clone();

                if (Currency != Currency.Empty)
                {
                    info.CurrencySymbol = useISOCodeAsSymbol ?
                                          Currency.ThreeLetterISOCode :
                                          Currency.Symbol.NullEmpty() ?? Currency.ThreeLetterISOCode;
                }
                else
                {
                    info.CurrencySymbol = "?";
                }
            }

            return Value.ToString(format, info);
        }

        #endregion

        #region Implicit/explicit operator overloads

        public static implicit operator Money(byte value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(int value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(long value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(sbyte value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(short value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(uint value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(ulong value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(ushort value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(decimal value)
        {
            return new Money(value);
        }

        public static implicit operator Money(double value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static implicit operator Money(float value)
        {
            return new Money(System.Convert.ToDecimal(value));
        }

        public static explicit operator byte(Money money)
        {
            return System.Convert.ToByte(money.Value);
        }

        public static explicit operator decimal(Money money)
        {
            return money.Value;
        }

        public static explicit operator double(Money money)
        {
            return System.Convert.ToDouble(money.Value);
        }

        public static explicit operator float(Money money)
        {
            return System.Convert.ToSingle(money.Value);
        }

        public static explicit operator int(Money money)
        {
            return System.Convert.ToInt32(money.Value);
        }

        public static explicit operator long(Money money)
        {
            return System.Convert.ToInt64(money.Value);
        }

        public static explicit operator sbyte(Money money)
        {
            return System.Convert.ToSByte(money.Value);
        }

        public static explicit operator short(Money money)
        {
            return System.Convert.ToInt16(money.Value);
        }

        public static explicit operator ushort(Money money)
        {
            return System.Convert.ToUInt16(money.Value);
        }

        public static explicit operator uint(Money money)
        {
            return System.Convert.ToUInt32(money.Value);
        }

        public static explicit operator ulong(Money money)
        {
            return System.Convert.ToUInt64(money.Value);
        }

        #endregion

        #region Equality/Comparison

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
            if (b.Value == 0 && a.Value > 0)
                return true;

            if (b.Value < 0 && a.Value >= 0)
                return true;

            if (a == Money.Zero)
                return b.Value < 0;

            if (b == Money.Zero)
                return a.Value > 0;

            GuardCurrenciesAreEqual(a, b);

            return a.Value > b.Value;

        }

        public static bool operator <(Money a, Money b)
        {
            if (a.Value == 0 && b.Value > 0)
                return true;

            if (a.Value < 0 && b.Value >= 0)
                return true;

            if (b == Money.Zero)
                return a.Value < 0;

            if (a == Money.Zero)
                return b.Value > 0;

            GuardCurrenciesAreEqual(a, b);

            return a.Value < b.Value;
        }

        public static bool operator <=(Money a, Money b)
        {
            if (a.Value < 0 && b.Value >= 0)
                return true;

            if (a.Value == 0 && b.Value >= 0)
                return true;

            GuardCurrenciesAreEqual(a, b);

            return a.Value < b.Value || a.Equals(b);

        }

        public static bool operator >=(Money a, Money b)
        {
            if (b.Value < 0 && a.Value >= 0)
                return true;

            if (b.Value == 0 && a.Value >= 0)
                return true;

            GuardCurrenciesAreEqual(a, b);

            return a.Value > b.Value || a.Equals(b);

        }

        #endregion

        #region Addition

        public static Money operator ++(Money a)
        {
            a.Value++;

            return a;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a == Money.Zero)
                return b;

            if (b == Money.Zero)
                return a;

            GuardCurrenciesAreEqual(a, b);

            return new Money(a.Value + b.Value, a.Currency);
        }

        #endregion

        #region Subtraction

        public static Money operator --(Money a)
        {
            a.Value--;

            return a;
        }

        public static Money operator -(Money a, Money b)
        {
            if (a == Money.Zero)
                return new Money(0 - b.Value, a.Currency);

            if (b == Money.Zero)
                return a;

            GuardCurrenciesAreEqual(a, b);

            return new Money(a.Value - b.Value, a.Currency);
        }

        #endregion

        #region Multiplication

        public static Money operator *(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Value * b.Value, a.Currency);
        }

        public static Money operator *(Money a, decimal b)
        {
            return new Money(a.Value * b, a.Currency);
        }

        public static Money operator *(decimal a, Money b)
        {
            return new Money(b.Value * a, b.Currency);
        }

        public static Money operator *(Money a, int b)
        {
            return new Money(a.Value * b, a.Currency);
        }

        public static Money operator *(int a, Money b)
        {
            return new Money(b.Value * a, b.Currency);
        }

        #endregion

        #region Division

        public static Money operator /(Money a, Money b)
        {
            GuardCurrenciesAreEqual(a, b);
            return new Money(a.Value / b.Value, a.Currency);
        }

        public static Money operator /(Money a, decimal b)
        {
            return new Money(a.Value / b, a.Currency);
        }

        public static Money operator /(decimal a, Money b)
        {
            return new Money(a / b.Value, b.Currency);
        }

        public static Money operator /(Money a, int b)
        {
            return new Money(a.Value / b, a.Currency);
        }

        public static Money operator /(int a, Money b)
        {
            return new Money(a / b.Value, b.Currency);
        }

        #endregion

        #region Parse

        // [...] TODO

        #endregion

        #region Exchange & Math

        /// <summary>
        /// Gets the ratio of one money to another.
        /// </summary>
        /// <param name="numerator">The numerator of the operation.</param>
        /// <param name="denominator">The denominator of the operation.</param>
        /// <returns>A decimal from 0.0 to 1.0 of the ratio between the two money values.</returns>
        public static decimal GetRatio(Money numerator, Money denominator)
        {
            if (numerator == Money.Zero)
                return 0;

            if (denominator == Money.Zero)
                throw new DivideByZeroException("Attempted to divide by zero!");

            GuardCurrenciesAreEqual(numerator, denominator);

            return numerator.Value / denominator.Value;
        }

        /// <summary>
        /// Gets the smallest money, given the two values.
        /// </summary>
        /// <param name="m1">The first money to compare.</param>
        /// <param name="m2">The second money to compare.</param>
        /// <returns>The smallest money value of the arguments.</returns>
        public static Money Min(Money m1, Money m2)
        {
            if (m1 == m2) // This will check currency.
                return m1;

            return new Money(Math.Min(m1.Value, m2.Value), m1.Currency);
        }

        /// <summary>
        /// Gets the largest money, given the two values.
        /// </summary>
        /// <param name="m1">The first money to compare.</param>
        /// <param name="m2">The second money to compare.</param>
        /// <returns>The largest money value of the arguments.</returns>
        public static Money Max(Money m1, Money m2)
        {
            if (m1 == m2) // This will check currency.
                return m1;

            return new Money(Math.Max(m1.Value, m2.Value), m1.Currency);
        }

        /// <summary>
        /// Gets the absolute value of the <see cref="Money"/>.
        /// </summary>
        /// <param name="value">The value of money to convert.</param>
        /// <returns>The money value as an absolute value.</returns>
        public static Money Abs(Money value)
        {
            return new Money(Math.Abs(value.Value), value.Currency);
        }

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw Error.InvalidCast(typeof(Money), typeof(bool));
        }

        public byte ToByte(IFormatProvider provider)
        {
            return (byte)this.Value;
        }

        public char ToChar(IFormatProvider provider)
        {
            throw Error.InvalidCast(typeof(Money), typeof(bool));
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw Error.InvalidCast(typeof(Money), typeof(bool));
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return this.Value;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return (double)this.Value;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return (short)this.Value;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return (int)this.Value;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return (long)this.Value;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return (sbyte)this.Value;
        }

        public float ToSingle(IFormatProvider provider)
        {
            return (float)this.Value;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return System.Convert.ChangeType(this.Value, conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return (ushort)this.Value;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return (uint)this.Value;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return (ulong)this.Value;
        }

        #endregion

    }

}
