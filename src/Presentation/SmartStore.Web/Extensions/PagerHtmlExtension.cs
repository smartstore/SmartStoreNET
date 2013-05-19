//using System;
//using System.Text;
//using System.Web.Mvc;
//using System.Web.Mvc.Html;
//using SmartStore.Core.Infrastructure;
//using SmartStore.Services.Localization;
//using SmartStore.Web.Framework.UI.Paging;
//using SmartStore.Web.Models.Boards;
//using SmartStore.Web.Models.Common;

//namespace SmartStore.Web.Extensions
//{
//    public static class PagerHtmlExtension
//    {
//        //we have two pagers:
//        //The first one can have custom routes
//        //The second one just adds query string parameter
//        // codehint: sm-delete
//        public static MvcHtmlString ForumTopicSmallPager<TModel>(this HtmlHelper<TModel> html, ForumTopicRowModel model)
//        {
//            var localizationService= EngineContext.Current.Resolve<ILocalizationService>();

//            var forumTopicId = model.Id;
//            var forumTopicSlug = model.SeName;
//            var totalPages = model.TotalPostPages;

//            if (totalPages > 0)
//            {
//                var links = new StringBuilder();

//                if (totalPages <= 4)
//                {
//                    for (int x = 1; x <= totalPages; x++)
//                    {
//                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, page = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }));
//                        if (x < totalPages)
//                        {
//                            links.Append(", ");
//                        }
//                    }
//                }
//                else
//                {
//                    links.Append(html.RouteLink("1", "TopicSlugPaged", new { id = forumTopicId, page = (1), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), 1) }));
//                    links.Append(" ... ");

//                    for (int x = (totalPages - 2); x <= totalPages; x++)
//                    {
//                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, page = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }));

//                        if (x < totalPages)
//                        {
//                            links.Append(", ");
//                        }
//                    }
//                }

//                // Inserts the topic page links into the localized string ([Go to page: {0}])
//                return MvcHtmlString.Create(String.Format(localizationService.GetResource("Forum.Topics.GotoPostPager"), links.ToString()));
//            }
//            return MvcHtmlString.Create(string.Empty);
//        }

//        // codehint: sm-delete
//    }
//}
