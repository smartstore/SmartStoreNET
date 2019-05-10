using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Forums
{
    [Validator(typeof(ForumGroupValidator))]
	public class ForumGroupModel : EntityModelBase, ILocalizedModel<ForumGroupLocalizedModel>, IStoreSelector, IAclSelector
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

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        public bool SubjectToAcl { get; set; }
        public IEnumerable<SelectListItem> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }

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

    public partial class ForumGroupValidator : AbstractValidator<ForumGroupModel>
    {
        public ForumGroupValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}