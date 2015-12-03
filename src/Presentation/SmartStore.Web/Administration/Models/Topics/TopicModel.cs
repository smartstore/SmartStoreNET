using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Topics
{
	[Validator(typeof(TopicValidator))]
    public class TopicModel : TabbableModel, ILocalizedModel<TopicLocalizedModel>
    {
        #region widget zone names
        private readonly static string[] s_widgetZones = new string[] { 
            "main_column_before", 
            "main_column_after", 
            "left_side_column_before", 
            "left_side_column_before", 
            "right_side_column_before", 
            "right_side_column_before", 
            "notifications", 
            "body_start_html_tag_after",
            "content_before", 
            "content_after", 
            "body_end_html_tag_before"
        };
        #endregion
        
        public TopicModel()
        {
			WidgetWrapContent = true;
			Locales = new List<TopicLocalizedModel>();
			AvailableStores = new List<StoreModel>();
            AvailableWidgetZones = s_widgetZones;
            AvailableTitleTags = new List<SelectListItem>(); 
            AvailableTitleTags.Add(new SelectListItem { Text = "h1", Value = "h1" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h2", Value = "h2" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h3", Value = "h3" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h4", Value = "h4" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h5", Value = "h5" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h6", Value = "h6" });
            AvailableTitleTags.Add(new SelectListItem { Text = "div", Value = "div" });
            AvailableTitleTags.Add(new SelectListItem { Text = "span", Value = "span" });
        }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInSitemap")]
        public bool IncludeInSitemap { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.IsPasswordProtected")]
        public bool IsPasswordProtected { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Password")]
		[DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.URL")]
        [AllowHtml]
        public string Url { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.RenderAsWidget")]
        public bool RenderAsWidget { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetZone")]
        [UIHint("WidgetZone")]
        public string WidgetZone { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetWrapContent")]
		public bool WidgetWrapContent { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetShowTitle")]
        public bool WidgetShowTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetBordered")]
        public bool WidgetBordered { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Priority")]
        public int Priority { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.TitleTag")]
        public string TitleTag { get; set; }

        public string[] AvailableWidgetZones { get; private set; }
        public IList<SelectListItem> AvailableTitleTags { get; private set; }

        public IList<TopicLocalizedModel> Locales { get; set; }
    }

    public class TopicLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }
    }
}