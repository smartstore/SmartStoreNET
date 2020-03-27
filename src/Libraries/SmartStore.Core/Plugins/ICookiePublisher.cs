using System;
using System.Web.Routing;

namespace SmartStore.Core.Plugins
{
	/// <summary>
	/// Marks a module as a cookie publisher. 
	/// </summary>
	public interface ICookiePublisher
	{
		/// <summary>
		/// Gets the cookie info of the cookie publisher (e.g. plugin or other module).
		/// </summary>
		CookieInfo GetCookieInfo();
	}

	/// <summary>
	/// Plugin cookie infos.
	/// </summary>
	public class CookieInfo
	{
		/// <summary>
		/// Name of the plugin.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of the cookie (e.g. purpose of using the cookie).
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Type of the cookie.
		/// </summary>
		public CookieType CookieType { get; set; }
	}

	/// <summary>
	/// Type of the cookie.
	/// </summary>
	public enum CookieType
	{
		Required,
		Analytics,
		ThirdParty
	}
}
