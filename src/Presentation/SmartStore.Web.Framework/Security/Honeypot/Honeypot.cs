using System;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Security
{
    public class HoneypotField
    {
        public string Name { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    internal static class Honeypot
    {
        internal const string TokenFieldName = "__hpToken";

        private static readonly string[] _fieldNames = new[] { "Phone", "Fax", "Email", "Age", "Name", "FirstName", "LastName", "Type", "Custom", "Reason", "Pet", "Question", "Region" };
        private static readonly string _fieldSuffix = CommonHelper.GenerateRandomDigitCode(5);

        public static HoneypotField CreateToken()
        {
            var r = new Random();

            // Create a rondom field name with pattern "[random1]-[random2][suffix]"
            var len = _fieldNames.Length;
            var fieldName = string.Concat(_fieldNames[r.Next(0, len)], "-", _fieldNames[r.Next(0, len)], _fieldSuffix);

            return new HoneypotField
            {
                Name = fieldName,
                CreatedOnUtc = DateTime.UtcNow
            };
        }

        public static string SerializeToken(HoneypotField token)
        {
            Guard.NotNull(token, nameof(token));

            var json = JsonConvert.SerializeObject(token);
            var encoded = MachineKey.Protect(Encoding.UTF8.GetBytes(json));

            var result = Convert.ToBase64String(encoded);
            return result;
        }

        public static HoneypotField DeserializeToken(string token)
        {
            Guard.NotEmpty(token, nameof(token));

            var encoded = Convert.FromBase64String(token);
            var decoded = MachineKey.Unprotect(encoded);
            var json = Encoding.UTF8.GetString(decoded);

            var result = JsonConvert.DeserializeObject<HoneypotField>(json);
            return result;
        }

        public static bool IsBot(HttpContextBase httpContext)
        {
            var tokenString = httpContext.Request.Form[TokenFieldName];
            if (tokenString.IsEmpty())
            {
                throw new InvalidOperationException("The required honeypot form field is missing. Please render the field with 'Html.HoneypotField()'.");
            }

            var token = DeserializeToken(tokenString);
            var trap = httpContext.Request.Form[token.Name];
            var isBot = trap == null || trap.Length > 0 || (DateTime.UtcNow - token.CreatedOnUtc).TotalMilliseconds < 2000;

            return isBot;
        }
    }
}
