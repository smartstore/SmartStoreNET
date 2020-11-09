using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public static class DataListExtensions
    {
        public static IHtmlString DataList<T>(this HtmlHelper helper, IEnumerable<T> items, int columns,
            Func<T, HelperResult> template, int gridColumns = 12)
            where T : class
        {
            if (items == null)
                return new HtmlString("");

            var spanClassPrefix = "col-md-";
            var rowClass = "row";

            Guard.Against<ArgumentOutOfRangeException>(gridColumns % columns != 0, "Wrong column count. Ensure that gridColumns is divisible by columns.");

            var sb = new StringBuilder();
            sb.Append("<div class='data-list data-list-grid'>");

            int cellIndex = 0;

            string spanClass = spanClassPrefix + (gridColumns / columns).ToString();

            foreach (T item in items)
            {
                if (cellIndex == 0)
                    sb.Append("<div class='data-list-row " + rowClass + "'>");

                sb.Append("<div class='{0} data-list-item equalized-column' data-equalized-deep='true'>".FormatInvariant(spanClass));
                sb.Append(template(item).ToHtmlString());
                sb.Append("</div>");

                cellIndex++;

                if (cellIndex == columns)
                {
                    cellIndex = 0;
                    sb.Append("</div>");
                }
            }

            if (cellIndex != 0)
            {
                sb.Append("</div>"); // close .row-fluid
            }

            sb.Append("</div>"); // close .data-list

            return new HtmlString(sb.ToString());
        }

        //public static IHtmlString DataList<T>(this HtmlHelper helper, IEnumerable<T> items, int columns,
        //    Func<T, HelperResult> template) 
        //    where T : class
        //{
        //    if (items == null)
        //        return new HtmlString("");

        //    var sb = new StringBuilder();
        //    sb.Append("<table>");

        //    int cellIndex = 0;

        //    foreach (T item in items)
        //    {
        //        if (cellIndex == 0)
        //            sb.Append("<tr>");

        //        sb.Append("<td");
        //        sb.Append(">");

        //        sb.Append(template(item).ToHtmlString());
        //        sb.Append("</td>");

        //        cellIndex++;

        //        if (cellIndex == columns)
        //        {
        //            cellIndex = 0;
        //            sb.Append("</tr>");
        //        }
        //    }

        //    if (cellIndex != 0)
        //    {
        //        for (; cellIndex < columns; cellIndex++)
        //        {
        //            sb.Append("<td>&nbsp;</td>");
        //        }

        //        sb.Append("</tr>");
        //    }

        //    sb.Append("</table>");

        //    return new HtmlString(sb.ToString());
        //}
    }
}
