using System;

namespace SmartStore.Services.Catalog.Modelling
{
	public class CheckoutAttributeQueryItem
	{
		public CheckoutAttributeQueryItem(int attributeId, string value)
		{
			Value = value.EmptyNull();
			AttributeId = attributeId;
		}

		public static string Prefix
		{
			get
			{
				return "cattr";
			}
		}

		public static string CreateKey(int attributeId)
		{
			return $"{Prefix}-{attributeId}";
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

		public override string ToString()
		{
			return CreateKey(AttributeId);
		}
	}
}
