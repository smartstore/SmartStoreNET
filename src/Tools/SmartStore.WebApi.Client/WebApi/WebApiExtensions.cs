using System;
using System.Text;

namespace SmartStore.Net.WebApi
{
	public static class WebApiExtensions
	{
		private static readonly DateTime BeginOfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>Epoch time. Number of seconds since midnight (UTC) on 1st January 1970.</summary>
		public static long ToUnixTime(this DateTime date)
		{
			return Convert.ToInt64((date.ToUniversalTime() - BeginOfEpoch).TotalSeconds);
		}

		/// <summary>UTC date based on number of seconds since midnight (UTC) on 1st January 1970.</summary>
		public static DateTime FromUnixTime(this long unixTime)
		{
			return BeginOfEpoch.AddSeconds(unixTime);
		}

		/// <summary>Converts bytes into a hex string.</summary>
		public static string ToHexString(this byte[] bytes, int length = 0)
		{
			if (bytes == null || bytes.Length <= 0)
				return "";

			var sb = new StringBuilder();

			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("x2"));

				if (length > 0 && sb.Length >= length)
					break;
			}
			return sb.ToString();
		}
	}
}
