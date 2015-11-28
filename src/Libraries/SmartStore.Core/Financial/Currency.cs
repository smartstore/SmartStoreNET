using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace SmartStore.Financial
{

    [Serializable]
    public struct Currency : IEquatable<Currency>
    {
        #region Fields

        internal readonly RegionInfo Region;
        public static readonly Currency Empty = new Currency();

        #endregion

        #region Ctor

        public Currency(RegionInfo region)
            : this()
        {
            Guard.ArgumentNotNull(() => region);

            Region = region;

            this.ThreeLetterISOCode = region.ISOCurrencySymbol;
            this.Symbol = region.CurrencySymbol;
            this.NativeName = region.CurrencyNativeName;
            this.EnglishName = region.CurrencyEnglishName;
        }

        public Currency(Currency currency)
            : this()
        {
            this.ThreeLetterISOCode = currency.ThreeLetterISOCode;
            this.Symbol = currency.Symbol;
            this.NativeName = currency.NativeName;
            this.EnglishName = currency.EnglishName;
            this.Region = currency.Region;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ISO standard three-letter name.
        /// </summary>
        public string ThreeLetterISOCode { get; private set; }

        ///// <summary>
        ///// Gets the integer-based ISO code.
        ///// </summary>
        //public int NumericISOCode { get; private set; }

        /// <summary>
        /// Gets the prefixing symbol of the currency.
        /// </summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Gets the native name of the currency.
        /// </summary>
        public string NativeName { get; private set; }

        /// <summary>
        /// Gets the english name of the currency.
        /// </summary>
        public string EnglishName { get; private set; }

        // TODO: (?) DecimalDigits, Separator, GroupSeparator, GroupSizes, NegativePatterm, PositivePattern

        #endregion

        #region Methods

        public static Currency Default
        {
            get
            {
                return new Currency(RegionInfo.CurrentRegion);
            }
        }

        public static Currency GetCurrency(string threeLetterISOCode)
        {
            Guard.ArgumentNotEmpty(threeLetterISOCode, "threeLetterISOCode");

            if (threeLetterISOCode.Length != 3)
                throw new ArgumentException("The currency ISO code must be 3 letters in length.", "threeLetterISOCode");

            var query =
                from r in GetValidRegions()
                where r.ISOCurrencySymbol.Equals(threeLetterISOCode, StringComparison.InvariantCultureIgnoreCase)
                select r;
            var region = query.First();

            return new Currency(region);
        }

        public static Currency GetCurrency(RegionInfo region)
        {
            return new Currency(region);
        }

        public static bool TryGetCurrency(string threeLetterISOCode, out Currency currency)
        {
            currency = Currency.Empty;

            try
            {
                currency = Currency.GetCurrency(threeLetterISOCode);
                return (currency.ThreeLetterISOCode != null);
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<Currency> AllCurrencies
        {
            get
            {
                return from r in GetValidRegions()
                       select new Currency(r);
            }
        }

        internal static IEnumerable<RegionInfo> GetValidRegions()
        {
            return
                from c in
                    (from c in CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures)
                     where !c.IsNeutralCulture && c.LCID != 0x7f
                     select c)
                select new RegionInfo(c.Name);
        }

        #endregion

        #region Comparison

        public override int GetHashCode()
        {
            return this.ThreeLetterISOCode.ToUpperInvariant().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != typeof(Currency))
                return false;

            return this.Equals((Currency)obj);
        }

        public bool Equals(Currency other)
        {
            return (this.ThreeLetterISOCode.Equals(other.ThreeLetterISOCode, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string ToString()
        {
            return "{0} ({1})".FormatCurrent(
                this.Symbol ?? this.ThreeLetterISOCode,
                this.NativeName ?? this.EnglishName ?? this.ThreeLetterISOCode);
        }

        #endregion

        #region Implicit operators

        public static implicit operator Currency(string threeLetterISOCode)
        {
            return Currency.GetCurrency(threeLetterISOCode);
        }

        public static implicit operator Currency(Money value)
        {
            return new Currency(value.Currency);
        }

        #endregion

        #region Operator overloads

        public static bool operator ==(Currency left, Currency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Temp/Cargo

        // ISOCode, NumericISOCode, Symbol, EnglishName, DecimalDigits

        //new CurrencyInfo("ARS",  32, "",         "Argentine peso",                 2,       false),
        //new CurrencyInfo("AUD",  36, "",         "Australian dollar",              2,       false),
        //new CurrencyInfo("ATS",  40, "",         "Austrian schilling",             2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("BSD",  44, "",         "Bahamian dollar",                2,       false),
        //new CurrencyInfo("BHD",  48, "",         "Bahraini dinar",                 2,       false),
        //new CurrencyInfo("BDT",  50, "",         "Bangladesh taka",                2,       false),
        //new CurrencyInfo("BEF",  56, "",         "Belgian franc",                  2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("BWP",  72, "",         "Botswanan pula",                 2,       false),
        //new CurrencyInfo("BRL", 986, "",         "Brazilian real",                 2,       false),
        //new CurrencyInfo("GBP", 826, "£",        "British pound",                  2,      true),
        //new CurrencyInfo("BND",  96, "",         "Brunei dollar",                  2,       false),
        //new CurrencyInfo("BGN", 975, "",         "Bulgarian lev",                  2,       false),
        //new CurrencyInfo("CAD", 124, "",         "Canadian dollar",                2,       false),
        //new CurrencyInfo("CLP", 152, "",         "Chilean peso",                   2,       false),
        //new CurrencyInfo("CNY", 156, "",         "Chinese yuan renminbi",          2,       false),
        //new CurrencyInfo("COP", 170, "",         "Colombian peso",                 2,       false),
        //new CurrencyInfo("HRK", 191, "",         "Croatian kuna",                  2,       false),
        //new CurrencyInfo("CYP", 196, "",         "Cyprus pound",                   2,       false),
        //new CurrencyInfo("CZK", 203, "",         "Czech koruna",                   2,       false),
        //new CurrencyInfo("DKK", 208, "DK",       "Danische Krone",                 2,       true),
        //new CurrencyInfo("DEM", 276, "DM",       "Deutsche Mark",                  2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("EGP", 818, "",         "Egyptian pound",                 2,       false),
        //new CurrencyInfo("EEK", 233, "",         "Estonian kroon",                 2,       false),
        //new CurrencyInfo("EUR", 978, "",        "Euro",                           2,      true),
        //new CurrencyInfo("XEU", 954, "",         "European Currency Unit",         2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("FJD", 242, "",         "Fiji dollar",                    2,       false),
        //new CurrencyInfo("FIM", 246, "",         "Finnish markka",                 2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("FRF", 250, "",         "French franc",                   2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("GHC", 288, "",         "Ghana cedi",                     2,       false),
        //new CurrencyInfo("GRD", 300, "",         "Greek drachma",                  2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("HNL", 340, "",         "Honduras lempira",               2,       false),
        //new CurrencyInfo("HKD", 344, "",         "Hong Kong dollar",               2,       false),
        //new CurrencyInfo("HUF", 348, "",         "Hungarian forint",               2,       false),
        //new CurrencyInfo("ISK", 352, "",         "Iceland krona",                  2,       false),
        //new CurrencyInfo("INR", 356, "",         "Indian rupee",                   2,       false),
        //new CurrencyInfo("IDR", 360, "",         "Indonesian rupiah",              2,       false),
        //new CurrencyInfo("IRR", 364, "",         "Iranian rial",                   2,       false),
        //new CurrencyInfo("IQD", 368, "",         "Iraqi dinar",                    2,       false),
        //new CurrencyInfo("IEP", 372, "",         "Irish pound",                    2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("ILS", 376, "Shekel",   "Israeli shekel",                 2,       false),
        //new CurrencyInfo("ITL", 380, "",         "Italian lira",                   2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("JMD", 388, "",         "Jamaican dollar",                2,       false),
        //new CurrencyInfo("JPY", 392, "¥",        "Japanese yen",                   2,      true),
        //new CurrencyInfo("KWD", 414, "",         "Kuwaiti dinar",                  2,       false),
        //new CurrencyInfo("LVL", 428, "",         "Latvian lats",                   2,       false),
        //new CurrencyInfo("LYD", 434, "",         "Libyan dinar",                   2,       false),
        //new CurrencyInfo("LTL", 440, "",         "Lithuanian litas",               2,       false),
        //new CurrencyInfo("LUF", 442, "",         "Luxembourg franc",               2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("MYR", 458, "",         "Malaysian ringgit",              2,       false),
        //new CurrencyInfo("MTL", 470, "",         "Maltese lira",                   2,       false),
        //new CurrencyInfo("MUR", 480, "",         "Mauritius rupee",                2,       false),
        //new CurrencyInfo("MXP", 484, "",         "Mexican peso",                   2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("MXN", 484, "",         "Mexican peso",                   2,       false),
        //new CurrencyInfo("MAD", 504, "",         "Moroccan dirham",                2,       false),
        //new CurrencyInfo("MMK", 104, "",         "Myanmar kyat",                   2,       false),
        //new CurrencyInfo("NPR", 524, "",         "Nepalese rupee",                 2,       false),
        //new CurrencyInfo("ANG", 532, "",         "Netherlands Antillian guilder",  2,       false),
        //new CurrencyInfo("NLG", 528, "",         "Netherlands guilder",            2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("NZD", 554, "",         "New Zealand dollar",             2,       false),
        //new CurrencyInfo("NOK", 578, "NK",       "Norwegische Krone",              2,       true),
        //new CurrencyInfo("OMR", 512, "",         "Omani rial",                     2,   false),
        //new CurrencyInfo("PKR", 586, "",         "Pakistan rupee",                 2,       false),
        //new CurrencyInfo("PAB", 590, "",         "Panamanian balboa",              2,       false),
        //new CurrencyInfo("PEN", 604, "",         "Peruvian sol nuevo",             2,       false),
        //new CurrencyInfo("PHP", 608, "",         "Philippine peso",                2,       false),
        //new CurrencyInfo("PLN", 985, "Zloty",    "Polish zloty",                   2,       true),
        //new CurrencyInfo("PTE", 620, "",         "Portuguese escudo",              2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("QAR", 634, "",         "Qatari riyal",                   2,       false),
        //new CurrencyInfo("ROL", 642, "",         "Romanian leu",                   2,       false),
        //new CurrencyInfo("RON",   0, "",         "Romanian new leu",               2,       false),
        //new CurrencyInfo("RUB", 643, "Rubel",    "Russian ruble",                  2,       true),
        //new CurrencyInfo("SAR", 682, "",         "Saudi riyal",                    2,       false),
        //new CurrencyInfo("SGD", 702, "",         "Singapore dollar",               2,       false),
        //new CurrencyInfo("SKK", 703, "",         "Slovak koruna",                  2,       false),
        //new CurrencyInfo("SIT", 705, "",         "Slovenia tolar",                 2,       false),
        //new CurrencyInfo("ZAR", 710, "Rand",     "South African rand",             2,       true),
        //new CurrencyInfo("ZAL", 991, "",         "South African rand (financial)", 2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("KRW", 410, "",         "South Korean won",               2,       false),
        //new CurrencyInfo("ESP", 724, "",         "Spanish peseta",                 2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("XDR", 960, "",         "Special Drawing Right",          2,       false),
        //new CurrencyInfo("LKR", 144, "",         "Sri Lankan rupee",               2,       false),
        //new CurrencyInfo("SEK", 752, "SK",       "Schwedische Krone",              2,       true),
        //new CurrencyInfo("CHF", 756, "Franken",  "Swiss franc",                    2,       true),
        //new CurrencyInfo("TWD", 901, "",         "Taiwan new dollar",              2,       false),
        //new CurrencyInfo("THB", 764, "",         "Thai baht",                      2,       false),
        //new CurrencyInfo("TTD", 780, "",         "Trinidad and Tobago dollar",     2,       false),
        //new CurrencyInfo("TND", 788, "",         "Tunisian dinar",                 2,       false),
        //new CurrencyInfo("TRL", 792, "",         "Turkish lira",                   2,       false), // Gibt es nicht mehr
        //new CurrencyInfo("TRY", 949, "",         "Turkish new lira",               2,       true),
        //new CurrencyInfo("AED", 784, "",         "U.A.E. dirham",                  2,       false),
        //new CurrencyInfo("USD", 840, "$",        "US Dollar",                      2,       true),
        //new CurrencyInfo("VEB", 862, "",         "Venezuelan bolivar",             2,       false)


        #endregion

    }

}
