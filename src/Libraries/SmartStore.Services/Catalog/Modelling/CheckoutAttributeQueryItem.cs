using System;

namespace SmartStore.Services.Catalog.Modelling
{
	public class CheckoutAttributeQueryItem
	{
		public CheckoutAttributeQueryItem(int attributeId, string value)
		{
			Value = value ?? string.Empty;
			AttributeId = attributeId;
		}

		public static string CreateKey(int attributeId)
		{
			return $"cattr{attributeId}";
		}

		public string Value { get; private set; }
		public int AttributeId { get; private set; }
		public DateTime? Date { get; set; }

		public override string ToString()
		{
			return CreateKey(AttributeId);
		}
	}
}
