using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public static class HtmlHelperExtensions
    {
        public static ComponentFactory<TModel> SmartStore<TModel>(this HtmlHelper<TModel> helper)
        {
            return new ComponentFactory<TModel>(helper);
        }

		public static IHtmlString Attrs(this HtmlHelper html, IDictionary<string, object> attrs)
		{
			var sb = new StringBuilder();

			using (var writer = new StringWriter(sb))
			{
				RenderAttrs(html, attrs, writer);
			}

			return MvcHtmlString.Create(sb.ToString());	
		}

		public static void RenderAttrs(this HtmlHelper html, IDictionary<string, object> attrs)
		{
			RenderAttrs(html, attrs, html.ViewContext.Writer);
		}

		private static void RenderAttrs(HtmlHelper html, IDictionary<string, object> attrs, TextWriter output)
		{
			Guard.NotNull(attrs, nameof(attrs));

			if (attrs.Count == 0)
			{
				return;
			}

			var first = true;

			foreach (var key in attrs.Keys)
			{
				var value = attrs[key];

				if (!first)
				{
					output.Write(" ");
				}

				first = false;

                output.Write(key);

                if (value != null)
                {
                    output.Write("=\"");
                    HttpUtility.HtmlAttributeEncode(value.ToString(), output);
                    output.Write("\"");
                }
			}
		}
	}
}
