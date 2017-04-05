using System;
using System.Collections.Specialized;
using SmartStore.Services.Catalog;

namespace SmartStore
{
	public static class NameValueCollectionExtensions
	{
		public static string AddGiftCardAttribute(this NameValueCollection collection, string attributes, int productId, IProductAttributeParser productAttributeParser, int bundleItemId = 0)
		{
			string recipientName = "";
			string recipientEmail = "";
			string senderName = "";
			string senderEmail = "";
			string giftCardMessage = "";

			string strProductId = "";
			if (productId != 0)
				strProductId = "_{0}_{1}".FormatWith(productId, bundleItemId);

			foreach (string formKey in collection.AllKeys)
			{
				if (formKey.Equals(string.Format("giftcard{0}.RecipientName", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					recipientName = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.RecipientEmail", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					recipientEmail = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.SenderName", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					senderName = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.SenderEmail", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					senderEmail = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.Message", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					giftCardMessage = collection[formKey];
					continue;
				}
			}

			return productAttributeParser.AddGiftCardAttribute(attributes, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
		}
	}
}
