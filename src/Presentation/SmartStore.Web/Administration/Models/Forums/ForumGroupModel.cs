using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Forums;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Forums
{
    [Validator(typeof(ForumGroupValidator))]
	public class ForumGroupModel : EntityModelBase, ILocalizedModel<ForumGroupLocalizedModel>, IStoreSelector
    {
        public ForumGroupModel()
        {
            ForumModels = new List<ForumModel>();
			Locales = new List<ForumGroupLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.SeName")]
		[AllowHtml]
		public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

		public IList<ForumModel> ForumModels { get; set; }

		public IList<ForumGroupLocalizedModel> Locales { get; set; }
    }

	public class ForumGroupLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
		[AllowHtml]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.SeName")]
		[AllowHtml]
		public string SeName { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Description")]
		[AllowHtml]
		public string Description { get; set; }
	}
}