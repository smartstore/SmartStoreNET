using System.Collections.Generic;
using SmartStore.Admin.Models.Directory;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Shipping
{
    public class ShippingMethodRestrictionModel : ModelBase
    {
        public ShippingMethodRestrictionModel()
        {
            AvailableShippingMethods = new List<ShippingMethodModel>();
            AvailableCountries = new List<CountryModel>();
            Restricted = new Dictionary<int, IDictionary<int, bool>>();
        }
        public IList<ShippingMethodModel> AvailableShippingMethods { get; set; }
        public IList<CountryModel> AvailableCountries { get; set; }

        //[country id] / [shipping method id] / [restricted]
        public IDictionary<int, IDictionary<int, bool>> Restricted { get; set; }
    }
}