using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.XPath;

namespace SmartStore
{
	public static class XPathExtensions
	{
		/// <summary>
		/// Safe way to get the value of a xpath navigator item
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="node">The node</param>
		/// <param name="xpath">Optional xpath</param>
		/// <param name="defaultValue">Optional default value</param>
		/// <param name="culture">If null is passed, CultureInfo.InvariantCulture is used.</param>
		/// <returns>The value</returns>
		public static T GetValue<T>(this XPathNavigator node, string xpath = null, T defaultValue = default(T), CultureInfo culture = null)
		{
			try
			{
				if (node != null)
				{
					if (xpath.IsNullOrEmpty() && node.Value.HasValue())
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, culture == null ? CultureInfo.InvariantCulture : culture, node.Value);

					var n = node.SelectSingleNode(xpath);

					if (n != null && n.Value.HasValue())
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, culture == null ? CultureInfo.InvariantCulture : culture, n.Value);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return defaultValue;
		}

		/// <summary>
		/// Safe way to get the value of a xpath navigator item
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="node">The node</param>
		/// <param name="xpath">Optional xpath</param>
		/// <param name="defaultValue">Optional default value</param>
		/// <returns>The value</returns>
		public static string GetValue(this XPathNavigator node, string xpath = null, string defaultValue = null)
		{
			try
			{
				if (node != null)
				{
					if (xpath.IsNullOrEmpty())
						return node.Value;

					var n = node.SelectSingleNode(xpath);
					if (n != null)
						return n.Value;
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return defaultValue;
		}
	}
}
