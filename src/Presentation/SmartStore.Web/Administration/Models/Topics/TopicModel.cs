using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Topics
{
    [Validator(typeof(TopicValidator))]
    public class TopicModel : TabbableModel, ILocalizedModel<TopicLocalizedModel>
    {
        public TopicModel()
        {
            WidgetWrapContent = true;
            Locales = new List<TopicLocalizedModel>();
            AvailableTitleTags = new List<SelectListItem>();
            AvailableCookieTypes = new List<SelectListItem>();
            MenuLinks = new Dictionary<string, string>();
            AvailableTitleTags.Add(new SelectListItem { Text = "h1", Value = "h1" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h2", Value = "h2" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h3", Value = "h3" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h4", Value = "h4" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h5", Value = "h5" });
            AvailableTitleTags.Add(new SelectListItem { Text = "h6", Value = "h6" });
            AvailableTitleTags.Add(new SelectListItem { Text = "div", Value = "div" });
            AvailableTitleTags.Add(new SelectListItem { Text = "span", Value = "span" });
        }

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

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.CookieType")]
        public int? CookieType { get; set; }
        public IList<SelectListItem> AvailableCookieTypes { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.HtmlId")]
        public string HtmlId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.BodyCssClass")]
        public string BodyCssClass { get; set; }

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

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.ShortTitle")]
        [AllowHtml]
        public string ShortTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Intro")]
        [AllowHtml]
        public string Intro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.RenderAsWidget")]
        public bool RenderAsWidget { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.WidgetZone")]
        [UIHint("WidgetZone")]
        public string[] WidgetZone { get; set; }

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

        [SmartResourceDisplayName("Common.Published")]
        public bool IsPublished { get; set; }

        public IList<SelectListItem> AvailableTitleTags { get; private set; }

        public IList<TopicLocalizedModel> Locales { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MenuLinks")]
        public Dictionary<string, string> MenuLinks { get; set; }
    }

    public class TopicLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.ShortTitle")]
        [AllowHtml]
        public string ShortTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Intro")]
        [AllowHtml]
        public string Intro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        public string SeName { get; set; }
    }

    public partial class TopicValidator : AbstractValidator<TopicModel>
    {
        public TopicValidator(Localizer T)
        {
            RuleFor(x => x.SystemName).NotEmpty();
            RuleFor(x => x.HtmlId)
                .Must(u => u.IsEmpty() || !u.Any(x => char.IsWhiteSpace(x)))
                .WithMessage(T("Admin.ContentManagement.Topics.Validation.NoWhiteSpace"));

            RuleFor(x => x.IsPasswordProtected)
                .Equal(false)
                .When(x => x.RenderAsWidget)
                .WithMessage(T("Admin.ContentManagement.Topics.Validation.NoPasswordAllowed"));

            RuleFor(x => x.Password)
                .NotEmpty()
                .When(x => x.IsPasswordProtected && !x.RenderAsWidget)
                .WithMessage(T("Admin.ContentManagement.Topics.Validation.NoEmptyPassword"));
        }
    }

    public class TopicMapper : IMapper<Topic, TopicModel>
    {
        public void Map(Topic from, TopicModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
            to.WidgetWrapContent = from.WidgetWrapContent ?? true;
        }
    }
}