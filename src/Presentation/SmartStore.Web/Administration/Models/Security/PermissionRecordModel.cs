using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Security
{
    public class PermissionRecordModel : ModelBase
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}