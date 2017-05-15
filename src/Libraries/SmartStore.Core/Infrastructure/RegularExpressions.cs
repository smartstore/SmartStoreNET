using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartStore
{
    public static class RegularExpressions
    {

        internal static readonly string ValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
        internal static readonly string ValidIntegerPattern = "^([-]|[0-9])[0-9]*$";

        internal static readonly Regex HasTwoDot = new Regex("[0-9]*[.][0-9]*[.][0-9]*", RegexOptions.Compiled);
        internal static readonly Regex HasTwoMinus = new Regex("[0-9]*[-][0-9]*[-][0-9]*", RegexOptions.Compiled);

        public static readonly Regex IsAlpha = new Regex("[^a-zA-Z]", RegexOptions.Compiled);
        public static readonly Regex IsAlphaNumeric = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
        public static readonly Regex IsNotNumber = new Regex("[^0-9.-]", RegexOptions.Compiled);
        public static readonly Regex IsPositiveInteger = new Regex(@"\d{1,10}", RegexOptions.Compiled);
        public static readonly Regex IsNumeric = new Regex("(" + ValidRealPattern + ")|(" + ValidIntegerPattern + ")", RegexOptions.Compiled);

  //      //public static readonly Regex IsWebUrl = new Regex(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.Compiled);
		///// <remarks>see https://msdn.microsoft.com/en-us/library/ms998267.aspx</remarks>
		//public static readonly Regex IsWebUrl = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_\=~]*)?$", RegexOptions.Singleline | RegexOptions.Compiled);

		public static readonly Regex IsEmail = new Regex("^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex IsGuid = new Regex(@"\{?[a-fA-F0-9]{8}(?:-(?:[a-fA-F0-9]){4}){3}-[a-fA-F0-9]{12}\}?", RegexOptions.Compiled);
        public static readonly Regex IsBase64Guid = new Regex(@"[a-zA-Z0-9+/=]{22,24}", RegexOptions.Compiled);

        public static readonly Regex IsCultureCode = new Regex(@"^[a-z]{2}(-[A-Z]{2})?$", RegexOptions.Singleline | RegexOptions.Compiled);

		public static readonly Regex IsYearRange = new Regex(@"^(\d{4})-(\d{4})$", RegexOptions.Compiled);

		public static readonly Regex IsIban = new Regex(@"[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}", RegexOptions.Singleline | RegexOptions.Compiled);
		public static readonly Regex IsBic = new Regex(@"([a-zA-Z]{4}[a-zA-Z]{2}[a-zA-Z0-9]{2}([a-zA-Z0-9]{3})?)", RegexOptions.Singleline | RegexOptions.Compiled);
    }

}
