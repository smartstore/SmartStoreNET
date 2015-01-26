using System;
using System.ComponentModel;
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
		/// <returns>The value</returns>
		public static T GetValue<T>(this XPathNavigator node, string xpath = null, T defaultValue = default(T))
		{
			try
			{
				if (node != null)
				{
					if (xpath.IsNullOrEmpty())
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(node.Value);

					var n = node.SelectSingleNode(xpath);
					if (n != null)
						return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(n.Value);
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
