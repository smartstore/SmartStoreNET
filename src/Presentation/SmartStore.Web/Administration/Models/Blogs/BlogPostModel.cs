﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Blogs
{
    [Validator(typeof(BlogPostValidator))]
    public class BlogPostModel : TabbableModel
	{
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.IsPublished")]
        public bool IsPublished { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        [AllowHtml]
        public string LanguageName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        [AllowHtml]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Intro")]
        [AllowHtml]
        public string Intro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.PreviewDisplayType")]
        public PreviewDisplayType PreviewDisplayType { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Picture")]
        public int? PictureId { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.PreviewPicture")]
        public int? PreviewPictureId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.SectionBg")]
        public string SectionBg { get; set; }
        
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.DisplayTagsInPreview")]
        public bool DisplayTagsInPreview { get; set; } = true;

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]
        [AllowHtml]
        public string Tags { get; set; }

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

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        // Store mapping.
        [UIHint("Stores"), AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }
    }

    public partial class BlogPostValidator : AbstractValidator<BlogPostModel>
    {
        public BlogPostValidator()
        {
            RuleFor(x => x.Title).NotNull();
            RuleFor(x => x.Body).NotNull();
            RuleFor(x => x.PictureId)
                .NotNull()
                .When(x => x.PreviewDisplayType == PreviewDisplayType.Default || x.PreviewDisplayType == PreviewDisplayType.DefaultSectionBg);
            RuleFor(x => x.PreviewPictureId)
                .NotNull()
                .When(x => x.PreviewDisplayType == PreviewDisplayType.Preview || x.PreviewDisplayType == PreviewDisplayType.PreviewSectionBg);
        }
    }

    public class BlogPostMapper :
        IMapper<BlogPost, BlogPostModel>
    {
        public void Map(BlogPost from, BlogPostModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(from.LanguageId, true, false);
        }
    }
}