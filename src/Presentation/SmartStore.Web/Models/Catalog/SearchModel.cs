using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class SearchModel : ModelBase
    {
        public SearchModel()
        {
            PagingFilteringContext = new SearchPagingFilteringModel();
            Products = new List<ProductOverviewModel>();

            this.AvailableCategories = new List<SelectListItem>();
            this.AvailableManufacturers = new List<SelectListItem>();
        }

        public string Warning { get; set; }

        public bool NoResults { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        [SmartResourceDisplayName("Search.SearchTerm")]
        [AllowHtml]
        public string Q { get; set; }
        /// <summary>
        /// Category ID
        /// </summary>
        [SmartResourceDisplayName("Search.Category")]
        public int Cid { get; set; }
        [SmartResourceDisplayName("Search.IncludeSubCategories")]
        public bool Isc { get; set; }
        /// <summary>
        /// Manufacturer ID
        /// </summary>
        [SmartResourceDisplayName("Search.Manufacturer")]
        public int Mid { get; set; }
        /// <summary>
        /// Price - From 
        /// </summary>
        [AllowHtml]
        public string Pf { get; set; }
        /// <summary>
        /// Price - To
        /// </summary>
        [AllowHtml]
        public string Pt { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [SmartResourceDisplayName("Search.SearchInDescriptions")]
        public bool Sid { get; set; }
        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        [SmartResourceDisplayName("Search.AdvancedSearch")]
        public bool As { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }


        public SearchPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
    }
}