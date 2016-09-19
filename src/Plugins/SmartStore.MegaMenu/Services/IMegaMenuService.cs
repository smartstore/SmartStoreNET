using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Payments;
using SmartStore.MegaMenu.Domain;

namespace SmartStore.MegaMenu.Services
{
	public partial interface IMegaMenuService
    {
        MegaMenuRecord GetMegaMenuRecord(int categoryId);
        void InsertMegaMenuRecord(MegaMenuRecord record);
        void UpdateMegaMenuRecord(MegaMenuRecord record);
        void DeleteMegaMenuRecord(MegaMenuRecord record);
    }
}
