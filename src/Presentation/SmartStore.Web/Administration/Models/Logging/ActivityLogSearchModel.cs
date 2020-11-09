using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Logging
{
    public class ActivityLogSearchModel : ModelBase
    {
        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public int ActivityLogTypeId { get; set; }
        public IList<SelectListItem> ActivityLogType { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnFrom")]
        public DateTime? CreatedOnFrom { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOnTo")]
        public DateTime? CreatedOnTo { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CustomerEmail")]
        [AllowHtml]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CustomerSystemAccount")]
        public bool? CustomerSystemAccount { get; set; }
    }
}