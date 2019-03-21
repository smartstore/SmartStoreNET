using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Menus
{
    public class MenuRecordListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int StoreId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.SystemName")]
        public string SystemName { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public int GridPageSize { get; set; }
    }
}