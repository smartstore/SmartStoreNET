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

        /// <summary>
        /// Key used for form names.
        /// </summary>
        /// <param name="attributeId">Checkout attribute identifier</param>
        /// <returns>Key</returns>
        public static string CreateKey(int attributeId)
        {
            return $"cattr{attributeId}";
        }

        public string Value { get; private set; }
        public int AttributeId { get; private set; }
        public DateTime? Date { get; set; }
        public bool IsFile { get; set; }
        public bool IsText { get; set; }

        public override string ToString()
        {
            var key = CreateKey(AttributeId);

            if (Date.HasValue)
            {
                return key + "-date";
            }
            else if (IsFile)
            {
                return key + "-file";
            }
            else if (IsText)
            {
                return key + "-text";
            }

            return key;
        }
    }
}
