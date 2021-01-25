using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Blogs
{
    public class BlogListModel : TabbableModel
    {
        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Title")]
        public string SearchTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Intro")]
        public string SearchIntro { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Body")]
        public string SearchBody { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags")]
        public string SearchTags { get; set; }
        public MultiSelectList SearchAvailableTags { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.StartDate")]
        public DateTime? SearchStartDate { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.EndDate")]
        public DateTime? SearchEndDate { get; set; }

        [UIHint("Stores")]
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        [SmartResourceDisplayName("Admin.Common.IsPublished")]
        public bool? SearchIsPublished { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.Fields.Language")]
        public int SearchLanguageId { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }

        public bool IsSingleStoreMode { get; set; }
        public bool IsSingleLanguageMode { get; set; }
        public int GridPageSize { get; set; }
    }
}