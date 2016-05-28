using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Forums;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Forums
{
    [Validator(typeof(ForumValidator))]
	public class ForumModel : EntityModelBase, ILocalizedModel<ForumLocalizedModel>
    {
        public ForumModel()
        {
            ForumGroups = new List<ForumGroupModel>();
			Locales = new List<ForumLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId")]
        public int ForumGroupId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.SeName")]
		[AllowHtml]
		public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public List<ForumGroupModel> ForumGroups { get; set; }

		public IList<ForumLocalizedModel> Locales { get; set; }
    }

	public class ForumLocalizedModel : ILocalizedModelLocal
	{
		public int LanguageId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Name")]
		[AllowHtml]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.SeName")]
		[AllowHtml]
		public string SeName { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]
		[AllowHtml]
		public string Description { get; set; }
	}
}