using System;
using System.Xml;
using SmartStore.Core.Domain.Localization;

namespace SmartStore
{
	public static class XmlWriterExtensions
	{
		public static void WriteCData(this XmlWriter writer, string name, string value, string prefix = null, string ns = null) 
        {
			if (name.HasValue() && value != null) 
            {
				if (prefix == null && ns == null)
					writer.WriteStartElement(name);
				else
					writer.WriteStartElement(prefix, name, ns);

				writer.WriteCData(value.RemoveInvalidXmlChars());

				writer.WriteEndElement();
			}
		}

		/// <summary>
		/// Created a simple or CData node element
		/// </summary>
		/// <param name="writer">The writer</param>
		/// <param name="name">Node name</param>
		/// <param name="value">Node value</param>
		/// <param name="language">The language. Its culture is always converted to lowercase!</param>
		/// <param name="asCData">Whether to create simple or CData node</param>
		public static void Write(this XmlWriter writer, string name, string value, Language language = null, bool asCData = false)
		{
			if (name.HasValue() && value != null)
			{
				if (language != null && value.IsEmpty())
					return;		// do not create too many empty nodes for empty localized values

				writer.WriteStartElement(name);

				if (language != null)
					writer.WriteAttributeString("culture", language.LanguageCulture.EmptyNull().ToLower());

				if (asCData)
					writer.WriteCData(value.RemoveInvalidXmlChars());
				else
					writer.WriteString(value.RemoveInvalidXmlChars());

				writer.WriteEndElement();
			}
		}
	}
}
