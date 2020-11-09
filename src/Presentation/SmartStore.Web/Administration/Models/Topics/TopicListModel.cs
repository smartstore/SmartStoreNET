using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Topics
{
    public class TopicListModel : ModelBase
    {
        public TopicListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.SystemName")]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.RenderAsWidget")]
        public bool? RenderAsWidget { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetZone")]
        public string WidgetZone { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}