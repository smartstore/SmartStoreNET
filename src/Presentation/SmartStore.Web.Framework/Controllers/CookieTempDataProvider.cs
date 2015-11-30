using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Controllers
{
	internal class CookieTempDataProvider : ITempDataProvider
	{
		internal const string TempDataCookieKey = "__ControllerTempData";
		HttpContextBase _httpContext;

		public CookieTempDataProvider(HttpContextBase httpContext)
		{
			if (httpContext == null)
			{
				throw new ArgumentNullException("httpContext");
			}
			_httpContext = httpContext;
		}

		public System.Web.HttpContextBase HttpContext
		{
			get
			{
				return _httpContext;
			}
		}

		protected virtual IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
		{
			System.Web.HttpCookie cookie = _httpContext.Request.Cookies[TempDataCookieKey];
			if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
			{
				IDictionary<string, object> deserializedTempData = DeserializeTempData(cookie.Value);
				return deserializedTempData;
			}

			return new Dictionary<string, object>();
		}

		protected virtual void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
		{
			bool isDirty = (values != null && values.Count > 0);

			string cookieValue = SerializeToBase64EncodedString(values);
			var cookie = new System.Web.HttpCookie(TempDataCookieKey);
			cookie.HttpOnly = true;

			// Remove cookie
			if (!isDirty)
			{
				cookie.Expires = DateTime.Now.AddDays(-4.0);
				cookie.Value = string.Empty;

				_httpContext.Response.Cookies.Set(cookie);

				return;

			}
			cookie.Value = cookieValue;

			_httpContext.Response.Cookies.Add(cookie);
		}

		public static IDictionary<string, object> DeserializeTempData(string base64EncodedSerializedTempData)
		{
			byte[] bytes = Convert.FromBase64String(base64EncodedSerializedTempData);
			var memStream = new MemoryStream(bytes);
			var binFormatter = new BinaryFormatter();
			return binFormatter.Deserialize(memStream, null) as IDictionary<string, object> /*TempDataDictionary : This returns NULL*/;
		}

		public static string SerializeToBase64EncodedString(IDictionary<string, object> values)
		{
			MemoryStream memStream = new MemoryStream();
			memStream.Seek(0, SeekOrigin.Begin);
			var binFormatter = new BinaryFormatter();
			binFormatter.Serialize(memStream, values);
			memStream.Seek(0, SeekOrigin.Begin);
			byte[] bytes = memStream.ToArray();
			return Convert.ToBase64String(bytes);
		}

		IDictionary<string, object> ITempDataProvider.LoadTempData(ControllerContext controllerContext)
		{
			return LoadTempData(controllerContext);
		}

		void ITempDataProvider.SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
		{
			SaveTempData(controllerContext, values);
		}
	}
}
