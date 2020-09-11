using System;
using System.Collections.Generic;
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

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
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

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }

    public partial class ForumValidator : AbstractValidator<ForumModel>
    {
        public ForumValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ForumGroupId).NotEmpty();
        }
    }

    public class ForumMapper :
        IMapper<Forum, ForumModel>
    {
        public void Map(Forum from, ForumModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
        }
    }
}