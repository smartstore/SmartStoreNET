using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Web.Framework.UI
{
    public static class HtmlHelperExtensions
    {
        public static ComponentFactory<TModel> SmartStore<TModel>(this HtmlHelper<TModel> helper)
        {
            return new ComponentFactory<TModel>(helper);
        }

        public static IHtmlString Attrs(this HtmlHelper html, IDictionary<string, object> attrs, params string[] exclude)
        {
            var sb = PooledStringBuilder.Rent();

            using (var writer = new StringWriter(sb))
            {
                RenderAttrs(attrs, writer, exclude);
            }

            return MvcHtmlString.Create(sb.ToStringAndReturn());
        }

        public static void RenderAttrs(this HtmlHelper html, IDictionary<string, object> attrs, params string[] exclude)
        {
            RenderAttrs(attrs, html.ViewContext.Writer, exclude);
        }

        private static void RenderAttrs(IDictionary<string, object> attrs, TextWriter output, params string[] exclude)
        {
            Guard.NotNull(attrs, nameof(attrs));

            if (attrs.Count == 0)
            {
                return;
            }

            var first = true;

            foreach (var kvp in attrs)
            {
                if (exclude.Contains(kvp.Key))
                    continue;

                if (!first)
                {
                    output.Write(" ");
                }

                first = false;

                output.Write(kvp.Key);

                if (kvp.Value != null)
                {
                    output.Write("=\"");
                    HttpUtility.HtmlAttributeEncode(kvp.Value.ToString(), output);
                    output.Write("\"");
                }
            }
        }
    }
}
