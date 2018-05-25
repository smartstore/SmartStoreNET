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
    public class TopicModel : TabbableModel, ILocalizedModel<TopicLocalizedModel>, IStoreSelector
    {       
        public TopicModel()
        {
			WidgetWrapContent = true;
			Locales = new List<TopicLocalizedModel>();
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

		// Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
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

		[SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]
		public string SeName { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.RenderAsWidget")]
        public bool RenderAsWidget { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetZone")]
		[UIHint("WidgetZone")]
		public string[] WidgetZone { get; set; }
		public MultiSelectList AvailableWidgetZones { get; set; }

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

        public bool IsSystemTopic { get; set; }

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

		[SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]
		public string SeName { get; set; }
	}
}