using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework
{
	public enum ZoneInjectMode
	{
		Append,
		Prepend,
		Replace
	}

	public static class HtmlZoneExtensions
	{
		private class DocumentZone : IDisposable
		{
			internal const string ItemsKey = "DocumentTail.Snippets";
			private const string UniqueKeysKey = "DocumentTail.UniqueKeys";

			private readonly ViewContext _viewContext;
			private readonly TextWriter _originalViewContextWriter;
			private readonly WebViewPage _page;
			private readonly string _targetZone;
			private readonly ZoneInjectMode _injectMode;

			public DocumentZone(HtmlHelper html, string targetZone, ZoneInjectMode injectMode, string key)
			{
				Guard.NotEmpty(targetZone, nameof(targetZone));

				_viewContext = html.ViewContext;
				_originalViewContextWriter = _viewContext.Writer;
				_page = (WebViewPage)html.ViewDataContainer;

				var writer = new StringWriter();
				_page.OutputStack.Push(writer);
				_viewContext.Writer = writer;

				_targetZone = targetZone;
				_injectMode = injectMode;

				if (key.HasValue())
				{
					UniqueKeys.Add(key);
				}
			}

			public static IEnumerable<string> GetZoneContent(string zone)
			{
				Guard.NotEmpty(zone, nameof(zone));

				var storage = HttpContext.Current?.GetItem<Multimap<string, string>>(ItemsKey, forceCreation: false);
				return storage?[zone];
			}

			public static bool HasUniqueKey(string key)
			{
				Guard.NotEmpty(key, nameof(key));

				var uniqueKeys = HttpContext.Current?.GetItem<HashSet<string>>(UniqueKeysKey, forceCreation: false);
				return uniqueKeys != null && uniqueKeys.Contains(key);
			}

			private static HashSet<string> UniqueKeys
			{
				get
				{
					return HttpContext.Current?.GetItem<HashSet<string>>(UniqueKeysKey, () => new HashSet<string>(StringComparer.OrdinalIgnoreCase), true);
				}
			}

			private Multimap<string, string> GetStorage()
			{
				return _page.ViewContext?.HttpContext?.GetItem<Multimap<string, string>>(ItemsKey, () => new Multimap<string, string>(StringComparer.OrdinalIgnoreCase));
			}

			public void Dispose()
			{
				var storage = GetStorage();
				if (storage == null)
					return;

				var writer = ((StringWriter)_page.OutputStack.Pop());
				var content = writer.ToString();

				_viewContext.Writer = _originalViewContextWriter;

				if (_injectMode == ZoneInjectMode.Append)
				{
					storage[_targetZone].Add(content);
				}
				else if (_injectMode == ZoneInjectMode.Prepend)
				{
					var list = new List<string>(storage[_targetZone]);
					list.Insert(0, content);
					storage.RemoveAll(_targetZone);
					storage[_targetZone].AddRange(list);
				}
				else // Replace
				{
					storage.RemoveAll(_targetZone);
					storage[_targetZone].Add(content);
				}
			}
		}

		public static IDisposable BeginZoneContent(this HtmlHelper helper, 
			string targetZone, 
			ZoneInjectMode injectMode = ZoneInjectMode.Append, 
			string key = null)
		{
			if ((key.HasValue() && DocumentZone.HasUniqueKey(key)) || helper.ViewContext.HttpContext.Request.IsAjaxRequest())
			{
				return ActionDisposable.Empty;
			}

			return new DocumentZone(helper, targetZone, injectMode, key);
		}

		public static void RenderZone(this HtmlHelper helper, string zone)
		{
			var snippets = DocumentZone.GetZoneContent(zone);
			if (snippets == null)
			{
				return;
			}

			foreach (var snippet in snippets)
			{
				helper.ViewContext.Writer.WriteLine(snippet);
			}
		}
	}
}
