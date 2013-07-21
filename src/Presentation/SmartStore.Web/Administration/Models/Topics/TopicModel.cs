using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Topics
{
    [Validator(typeof(TopicValidator))]
    public class TopicModel : EntityModelBase, ILocalizedModel<TopicLocalizedModel>
    {
        public TopicModel()
        {
            Locales = new List<TopicLocalizedModel>();
			AvailableStores = new List<StoreModel>();
        }

        //Store mapping
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