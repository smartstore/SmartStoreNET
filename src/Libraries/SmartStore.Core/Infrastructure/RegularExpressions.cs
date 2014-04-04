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
        public static readonly Regex IsWebUrl = new Regex(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex IsEmail = new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.Singleline | RegexOptions.Compiled);
        public static readonly Regex RemoveHTML = new Regex(@"<[/]{0,1}\s*(?<tag>\w*)\s*(?<attr>.*?=['""].*?[""'])*?\s*[/]{0,1}>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static readonly Regex IsGuid = new Regex(@"\{?[a-fA-F0-9]{8}(?:-(?:[a-fA-F0-9]){4}){3}-[a-fA-F0-9]{12}\}?", RegexOptions.Compiled);
        public static readonly Regex IsBase64Guid = new Regex(@"[a-zA-Z0-9+/=]{22,24}", RegexOptions.Compiled);

        public static readonly Regex IsCultureCode = new Regex(@"^([a-z]{2})|([a-z]{2}-[A-Z]{2})$", RegexOptions.Singleline | RegexOptions.Compiled);

		public static readonly Regex IsYearRange = new Regex(@"^(\d{4})-(\d{4})$", RegexOptions.Compiled);

    }

}
