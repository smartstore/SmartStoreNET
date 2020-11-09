using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.News
{
    public partial class HomePageNewsItemsModel : ModelBase, ICloneable
    {
        public HomePageNewsItemsModel()
        {
            NewsItems = new List<NewsItemModel>();
        }

        public IList<NewsItemModel> NewsItems { get; set; }

        public object Clone()
        {
            // We use a shallow copy (deep clone is not required here)
            return this.MemberwiseClone();
        }
    }
}