﻿using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class HeaderLinksModel : ModelBase
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerEmailUsername { get; set; }
        public bool IsCustomerImpersonated { get; set; }


        public bool DisplayAdminLink { get; set; }

        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
        
        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }

        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }
    }
}