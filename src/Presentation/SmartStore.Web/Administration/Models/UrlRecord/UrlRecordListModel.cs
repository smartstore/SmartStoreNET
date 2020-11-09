using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.UrlRecord
{
    public partial class UrlRecordListModel : ModelBase
    {
        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityName")]
        public string EntityName { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityId")]
        public int? EntityId { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.IsActive")]
        public bool? IsActive { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Language")]
        public int? LanguageId { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }
    }
}