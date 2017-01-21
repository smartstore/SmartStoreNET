using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework
{
	public static class HtmlTailExtensions
	{
		private class DocumentTail : IDisposable
		{
			internal const string ItemsKey = "DocumentTail.Snippets";
			private const string UniqueKeysKey = "DocumentTail.UniqueKeys";

			private readonly WebViewPage _page;

			public DocumentTail(WebViewPage page, string key)
			{
				_page = page;
				_page.OutputStack.Push(new StringWriter());

				if (key.HasValue())
				{
					UniqueKeys.Add(key);
				}
			}

			public static List<string> TailSnippets
			{
				get
				{
					var items = HttpContext.Current?.Items;
					if (items == null)
					{
						return new List<string>();
					}					

					if (!items.Contains(ItemsKey))
					{
						items[ItemsKey] = new List<string>();
					}					

					return (List<string>)items[ItemsKey];
				}
			}

			public static bool HasUniqueKey(string key)
			{
				Guard.NotEmpty(key, nameof(key));

				return UniqueKeys.Contains(key);
			}

			private static HashSet<string> UniqueKeys
			{
				get
				{
					var items = HttpContext.Current?.Items;
					if (items == null)
					{
						return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					}

					if (!items.Contains(UniqueKeysKey))
					{
						items[UniqueKeysKey] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					}

					return (HashSet<string>)items[UniqueKeysKey];
				}
			}

			public void Dispose()
			{
				TailSnippets.Add(((StringWriter)_page.OutputStack.Pop()).ToString());
			}
		}

		public static IDisposable BeginTailSnippet(this HtmlHelper helper, string key = null)
		{
			if (key.HasValue() && DocumentTail.HasUniqueKey(key))
			{
				return new ActionDisposable();
			}

			return new DocumentTail((WebViewPage)helper.ViewDataContainer, key);
		}

		public static void RenderTail(this HtmlHelper helper)
		{
			var snippets = DocumentTail.TailSnippets;
			if (snippets == null || snippets.Count == 0)
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
