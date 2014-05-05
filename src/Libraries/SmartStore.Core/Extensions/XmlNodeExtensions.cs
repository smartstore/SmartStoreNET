using System;
using System.ComponentModel;
using System.Xml;

namespace SmartStore
{

	public static class XmlNodeExtensions
	{
		/// <summary>Safe way to get inner text of an attribute.</summary>
		public static T GetAttributeText<T>(this XmlNode node, string attributeName, T defaultValue = default(T)) {
			try 
            {
				if (node != null && attributeName.HasValue()) 
                {
					XmlAttribute attr = node.Attributes[attributeName];
					if (attr != null) 
                    {
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(attr.InnerText);
					}
				}
			}
			catch (Exception exc) 
            {
				exc.Dump();
			}

			return defaultValue;
		}
		/// <summary>Safe way to get inner text of an attribute.</summary>
		public static string GetAttributeText(this XmlNode node, string attributeName) 
        {
			return node.GetAttributeText<string>(attributeName, null);
		}

		/// <summary>Safe way to get inner text of a node.</summary>
		public static T GetText<T>(this XmlNode node, string xpath = null, T defaultValue = default(T)) 
        {
			try 
            {
				if (node != null) 
                {
					if (xpath.IsNullOrEmpty())
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(node.InnerText);

					XmlNode n = node.SelectSingleNode(xpath);
					if (n != null)
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(n.InnerText);
				}
			}
			catch (Exception exc) 
            {
				exc.Dump();
			}

			return defaultValue;
		}
		/// <summary>Safe way to get inner text of a node.</summary>
		public static string GetText(this XmlNode node, string xpath = null, string defaultValue = default(string)) 
        {
			return node.GetText<string>(xpath, defaultValue);
		}

	}	// class
}
