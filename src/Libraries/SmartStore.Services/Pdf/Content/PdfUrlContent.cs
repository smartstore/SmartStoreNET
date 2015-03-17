﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using SmartStore.Core;

namespace SmartStore.Services.Pdf
{
	public class PdfUrlContent : IPdfContent
	{
		private readonly string _url;

		public PdfUrlContent(string url)
		{
			Guard.ArgumentNotEmpty(() => url);
			_url = url;

			this.SendAuthCookie = false;
			this.Post = new Dictionary<string, string>();
			this.Cookies = new Dictionary<string, string>();
		}

		#region Options

		/// <summary>
		/// Send FormsAuthentication cookie to authorize
		/// </summary>
		public bool SendAuthCookie { get; set; }

		/// <summary>
		/// HTTP Authentication username.
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// HTTP Authentication password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Sets cookies.
		/// </summary>
		public IDictionary<string, string> Post { get; set; }

		/// <summary>
		/// Sets post values.
		/// </summary>
		public IDictionary<string, string> Cookies { get; set; }

		#endregion

		public PdfContentKind Kind
		{
			get { return PdfContentKind.Url; }
		}

		protected virtual string GetAbsoluteUrl()
		{
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				return WebHelper.GetAbsoluteUrl(_url, new HttpRequestWrapper(HttpContext.Current.Request));
			}

			return _url;
		}

		public string Process(string flag)
		{
			return GetAbsoluteUrl();
		}

		public void WriteArguments(string flag, StringBuilder builder)
		{
			if (UserName.HasValue())
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, " --username {0}", UserName);
			}

			if (Password.HasValue())
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, " --password {0}", Password);
			}

			if (Post != null && Post.Count > 0)
			{
				CreateRepeatableFlags("--post", Post, builder);
			}

			if (Cookies != null && Cookies.Count > 0)
			{
				CreateRepeatableFlags("--cookie", Cookies, builder);
			}

			// Send FormsAuthentication Cookie
			var ctx = HttpContext.Current;
			if (SendAuthCookie && ctx != null && ctx.Request != null && ctx.Request.Cookies != null)
			{
				var authCookieName = FormsAuthentication.FormsCookieName;
				if (Cookies == null || !Cookies.ContainsKey(authCookieName))
				{
					HttpCookie authCookie = null;
					if (ctx.Request.Cookies.AllKeys.Contains(authCookieName))
					{
						authCookie = ctx.Request.Cookies[authCookieName];
					}
					if (authCookie != null)
					{
						var authCookieValue = authCookie.Value;
						builder.AppendFormat(CultureInfo.InvariantCulture, " {0} {1} {2}", "--cookie", authCookieName, authCookieValue);
					}
				}
			}
		}

		private void CreateRepeatableFlags(string flagName, IDictionary<string, string> dict, StringBuilder sb)
		{
			foreach (var kvp in dict)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " {0} {1} {2}", flagName, kvp.Key, kvp.Value.EmptyNull());
			}
		}

		public void Teardown()
		{
			// noop
		}
	}
}
