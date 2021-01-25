using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Blogs
{
    [Validator(typeof(BlogPostValidator))]
    public class BlogPostModel : TabbableModel, ILocalizedModel<BlogPostLocalizedModel>
    {
        public BlogPostModel()
        {
            Locales = new List<BlogPostLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Common.IsPublished")]
        public bool IsPublished { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Intro")]
        [AllowHtml]
        public string Intro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.PreviewDisplayType")]
        public PreviewDisplayType PreviewDisplayType { get; set; }

        [UIHint("Media"), AdditionalMetadata("album", "content")]
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Picture")]
        public int? PictureId { get; set; }

        [UIHint("Media"), AdditionalMetadata("album", "content")]
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.PreviewPicture")]
        public int? PreviewPictureId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SectionBg")]
        public string SectionBg { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.DisplayTagsInPreview")]
        public bool DisplayTagsInPreview { get; set; } = true;

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]
        public string[] Tags { get; set; }
        public MultiSelectList AvailableTags { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Comments")]
        public int Comments { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOnUtc { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        public int? LanguageId { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        public string LanguageName { get; set; }

        public bool IsSingleLanguageMode { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        public IList<BlogPostLocalizedModel> Locales { get; set; }
    }

    public class BlogPostLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Intro")]
        [AllowHtml]
        public string Intro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }
    }


    public partial class BlogPostValidator : AbstractValidator<BlogPostModel>
    {
        public BlogPostValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Body).NotEmpty();
            RuleFor(x => x.PictureId)
                .NotNull()
                .When(x => x.PreviewDisplayType == PreviewDisplayType.Default || x.PreviewDisplayType == PreviewDisplayType.DefaultSectionBg);
            RuleFor(x => x.PreviewPictureId)
                .NotNull()
                .When(x => x.PreviewDisplayType == PreviewDisplayType.Preview || x.PreviewDisplayType == PreviewDisplayType.PreviewSectionBg);
        }
    }

    public class BlogPostMapper :
        IMapper<BlogPost, BlogPostModel>,
        IMapper<BlogPostModel, BlogPost>
    {
        public void Map(BlogPost from, BlogPostModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
            to.PictureId = from.MediaFileId;
            to.PreviewPictureId = from.PreviewMediaFileId;
        }

        public void Map(BlogPostModel from, BlogPost to)
        {
            MiniMapper.Map(from, to);
            to.MediaFileId = from.PictureId.ZeroToNull();
            to.PreviewMediaFileId = from.PreviewPictureId.ZeroToNull();
        }
    }
}