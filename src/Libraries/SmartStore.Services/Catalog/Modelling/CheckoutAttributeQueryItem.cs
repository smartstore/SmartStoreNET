using System;
using System.Collections.Generic;
using System.Web;

namespace SmartStore.Services.Catalog.Modelling
{
	public class CheckoutAttributeQueryItem
	{
		public CheckoutAttributeQueryItem(int attributeId, string value)
		{
			Value = value.EmptyNull();
			AttributeId = attributeId;
		}

		public static string CreateKey(int attributeId)
		{
			return $"cattr{attributeId}";
		}

		public string Value { get; private set; }
		public int AttributeId { get; private set; }

		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
		public DateTime? Date
		{
			get
			{
				if (Year > 0 && Month > 0 && Day > 0)
				{
					try
					{
						return new DateTime(Year, Month, Day);
					}
					catch { }
				}

				return null;
			}
		}

		public string ToQueryString()
		{
			var result = new List<string>();
			var key = ToString();

			if (Year > 0 && Month > 0 && Day > 0)
			{
				var day = string.Concat(key, "-day=", Day);
				var month = string.Concat(key, "-month=", Month);
				var year = string.Concat(key, "-year=", Year);

				return string.Join("&", day, month, year);
			}

			return string.Concat(key, "=", HttpUtility.UrlEncode(Value));
		}

		public override string ToString()
		{
			return CreateKey(AttributeId);
		}
	}
}
