using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.News
{
    public partial class NewsItemListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Title")]
        public string SearchTitle { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Short")]
        public string SearchShort { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Full")]
        public string SearchFull { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.StartDate")]
        public DateTime? SearchStartDate { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.EndDate")]
        public DateTime? SearchEndDate { get; set; }

        [UIHint("Stores")]
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        [SmartResourceDisplayName("Admin.Common.IsPublished")]
        public bool? SearchIsPublished { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.News.NewsItems.Fields.Language")]
        public int SearchLanguageId { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }

        public bool IsSingleStoreMode { get; set; }
        public bool IsSingleLanguageMode { get; set; }
        public int GridPageSize { get; set; }
    }
}