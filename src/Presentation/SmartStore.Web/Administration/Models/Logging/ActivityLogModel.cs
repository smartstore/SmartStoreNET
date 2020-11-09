using System;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Logging;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Logging
{
    public partial class ActivityLogModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Comment")]
        public string Comment { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.IsSystemAccount")]
        public bool IsSystemAccount { get; set; }
        public string SystemAccountName { get; set; }
    }

    public class ActivityLogMapper :
        IMapper<ActivityLog, ActivityLogModel>
    {
        public void Map(ActivityLog from, ActivityLogModel to)
        {
            MiniMapper.Map(from, to);
            to.ActivityLogTypeName = from.ActivityLogType?.Name;
            to.CustomerEmail = from.Customer?.Email;
        }
    }
}
