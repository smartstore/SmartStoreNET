using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Blogs;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Blogs
{
    [Validator(typeof(BlogPostValidator))]
    public class BlogPostModel : EntityModelBase
    {
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

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.AllowComments")]
        public bool AllowComments { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]
        [AllowHtml]
        public string Tags { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Comments")]
        public int Comments { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOnUtc")]
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

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }
    }
}