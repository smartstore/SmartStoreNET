using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class EstimateShippingModel : ModelBase
    {
        public EstimateShippingModel()
        {
            ShippingOptions = new List<ShippingOptionModel>();
            Warnings = new List<string>();
            
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        public bool Enabled { get; set; }

		public string ShippingInfoUrl { get; set; }

        public IList<ShippingOptionModel> ShippingOptions { get; set; }

        public IList<string> Warnings { get; set; }
        
        [SmartResourceDisplayName("ShoppingCart.EstimateShipping.Country")]
        public int? CountryId { get; set; }
        [SmartResourceDisplayName("ShoppingCart.EstimateShipping.StateProvince")]
        public int? StateProvinceId { get; set; }
        [SmartResourceDisplayName("ShoppingCart.EstimateShipping.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

		#region Nested Classes

        public partial class ShippingOptionModel : ModelBase
        {
			public int ShippingMethodId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string Price { get; set; }
        }

		#endregion
    }
}