using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Forums
{
    [Validator(typeof(ForumGroupValidator))]
    public class ForumGroupModel : EntityModelBase, ILocalizedModel<ForumGroupLocalizedModel>
    {
        public ForumGroupModel()
        {
            ForumModels = new List<ForumModel>();
            Locales = new List<ForumGroupLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        // ACL.
        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public int[] SelectedCustomerRoleIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public bool SubjectToAcl { get; set; }

        public IList<ForumModel> ForumModels { get; set; }
        public IList<ForumGroupLocalizedModel> Locales { get; set; }
    }

    public class ForumGroupLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
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

    public class ForumGroupMapper :
        IMapper<ForumGroup, ForumGroupModel>
    {
        public void Map(ForumGroup from, ForumGroupModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
        }
    }
}