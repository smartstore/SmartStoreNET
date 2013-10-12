using System;
using System.ComponentModel;
using System.Xml;

namespace SmartStore
{
	/// <remarks>codehint: sm-add</remarks>
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
		public static void WriteNode(this XmlWriter writer, string name, Action content) 
        {
			if (name.HasValue() && content != null) 
            {
				writer.WriteStartElement(name);
				content();
				writer.WriteEndElement();
			}
		}

	}	// class
}
