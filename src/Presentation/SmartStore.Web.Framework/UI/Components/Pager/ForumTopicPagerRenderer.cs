using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Web.Framework.UI
{
    
    public class ForumTopicPagerRenderer : PagerRenderer
    {

        protected override void AddPageItemsList(List<PagerItem> items)
        {
            var pager = base.Component;
            var maxPages = pager.MaxPagesToDisplay;
            var pageCount = pager.Model.TotalPages;

            int start = 1;
            if (pageCount > maxPages + 1)
            {
                start = (pageCount + 1) - maxPages;
            }
            int end = pager.Model.TotalPages;

            if (start > 2)
            {
                items.Add(new PagerItem("1", GenerateUrl(1)));
                items.Add(new PagerItem("...", "", PagerItemType.Text));
            }

            for (int i = start; i <= end; i++)
            {
                var item = new PagerItem(i.ToString(), GenerateUrl(i));
                items.Add(item);
            }
        }

    }

}
