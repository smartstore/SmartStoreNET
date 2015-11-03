using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.UrlRecord
{
    public partial class UrlRecordModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
        public string Slug { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityId")]
        public int EntityId { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.EntityName")]
        public string EntityName { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.IsActive")]
        public bool IsActive { get; set; }

		[SmartResourceDisplayName("Admin.System.SeNames.Language")]
		public int LanguageId { get; set; }
		public List<SelectListItem> AvailableLanguages { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Language")]
        public string Language { get; set; }
    }
}