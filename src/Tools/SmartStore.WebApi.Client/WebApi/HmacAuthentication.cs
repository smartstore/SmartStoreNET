using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SmartStore.Net.WebApi
{
	public class HmacAuthentication
	{
        protected static readonly string[] _dateFormats = new string[] { "o", "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ" };
        protected static readonly string _delimiterRepresentation = "\n";
        protected static readonly string _scheme = "SmNetHmac";

		public static string Scheme1 { get { return _scheme + "1"; } }
		public static string SignatureMethod { get { return "HMAC-SHA256"; } }

		/// <summary>Creates two random unequal keys.</summary>
		public bool CreateKeys(out string key1, out string key2, int length = 32)
		{
			key1 = key2 = null;

			using (var rng = RandomNumberGenerator.Create())
			{
				for (int i = 0; i < 9999; ++i)
				{
					byte[] data1 = new byte[length];
					byte[] data2 = new byte[length];

					rng.GetNonZeroBytes(data1);
					rng.GetNonZeroBytes(data2);

					key1 = data1.ToHexString(length).ToLower();
					key2 = data2.ToHexString(length).ToLower();

					if (key1 != key2)
						break;
				}
			}
			return !string.IsNullOrWhiteSpace(key1) && !string.IsNullOrWhiteSpace(key2) && key1 != key2;
		}

		/// <summary>Creates a base64 encoded hash for a content.</summary>
		public string CreateContentMd5Hash(byte[] content)
		{
			string result = "";
			if (content != null && content.Length > 0)
			{
				using (var md5 = MD5.Create())
				{
					byte[] hash = md5.ComputeHash(content);
					result = Convert.ToBase64String(hash);
				}
			}
			return result;
		}

		/// <summary>Creates a base64 encoded HMAC-SHA256 signature.</summary>
		public string CreateSignature(string secretKey, string messageRepresentation)
		{
			if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(messageRepresentation))
				return "";

			string signature;
			var secretBytes = Encoding.UTF8.GetBytes(secretKey);
			var valueBytes = Encoding.UTF8.GetBytes(messageRepresentation);

			using (var hmac = new HMACSHA256(secretBytes))
			{
				var hash = hmac.ComputeHash(valueBytes);
				signature = Convert.ToBase64String(hash);
			}
			return signature;
		}

		/// <summary>Creates a message representation as follows:
		/// HTTP method\n +
		/// Content-MD5\n + 
		/// Response content type (accept header)\n + 
		/// Canonicalized URI\n
		/// ISO-8601 UTC timestamp including milliseconds (e.g. 2013-09-23T09:24:43.5395441Z)\n
		/// Public-Key
		/// </summary>
		public string CreateMessageRepresentation(WebApiRequestContext context, string contentMd5Hash, string timestamp, bool queryStringDecode = false)
		{
			if (context == null || !context.IsValid)
				return null;

			var url = context.Url;

			if (queryStringDecode)
			{
				var uri = new Uri(url);

				if (uri.Query != null && uri.Query.Length > 0)
				{
					url = string.Concat(uri.GetLeftPart(UriPartial.Path), HttpUtility.UrlDecode(uri.Query));
				}
			}

			var result = string.Join(_delimiterRepresentation,
				context.HttpMethod.ToLower(),
				contentMd5Hash ?? "",
				context.HttpAcceptType.ToLower(),
				url.ToLower(),
				timestamp,
				context.PublicKey.ToLower()
			);

			return result;
		}

		/// <summary>Creates the value for the authorization header entry.</summary>
		public string CreateAuthorizationHeader(string signature)
		{
			if (string.IsNullOrWhiteSpace(signature))
				return "";

			return Scheme1 + " " + signature;
		}

		/// <summary>Checks whether the authorization header is valid.</summary>
		public bool IsAuthorizationHeaderValid(string scheme, string signature)
		{
			return (!string.IsNullOrWhiteSpace(scheme) && scheme.StartsWith(_scheme) && !string.IsNullOrWhiteSpace(signature));
		}

		/// <summary>Returns a validated, versioned scheme value.</summary>
		public string GetWwwAuthenticateScheme(string schemeConsumer)
		{
			if (!string.IsNullOrWhiteSpace(schemeConsumer) && schemeConsumer == Scheme1)
			{
				return schemeConsumer;
			}
			return Scheme1;	// fallback to first version
		}

        /// <summary>
        /// Parse ISO-8601 UTC timestamp including optional milliseconds.
        /// Examples: 2013-11-09T11:37:21.1918793Z, 2013-11-09T11:37:21.191Z, 2013-11-09T11:37:21Z.
        /// </summary>
        public bool ParseTimestamp(string timestamp, out DateTime time)
        {
            foreach (var format in _dateFormats)
            {
                if (DateTime.TryParseExact(timestamp, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out time))
                    return true;
            }

            time = new DateTime();
            return false;
        }
    }


	public enum HmacResult : int
	{
		Success = 0,
		FailedForUnknownReason,
		ApiUnavailable,
		InvalidAuthorizationHeader,
		InvalidSignature,
		InvalidTimestamp,
		TimestampOutOfPeriod,
		TimestampOlderThanLastRequest,
		MissingMessageRepresentationParameter,
		ContentMd5NotMatching,
		UserUnknown,
		UserDisabled,
		UserInvalid,
		UserHasNoPermission
	}
}
