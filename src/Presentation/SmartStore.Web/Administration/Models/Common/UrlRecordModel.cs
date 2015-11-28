using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Common
{
    public partial class UrlRecordModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityId")]
        public int EntityId { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityName")]
        public string EntityName { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.IsActive")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Language")]
        public string Language { get; set; }
    }
}