using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Orders
{
    public class GiftCardListModel : ModelBase
    {
        public GiftCardListModel()
        {
            ActivatedList = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.GiftCards.List.CouponCode")]
        [AllowHtml]
        public string CouponCode { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.List.Activated")]
        public int ActivatedId { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.List.Activated")]
        public IList<SelectListItem> ActivatedList { get; set; }
    }
}