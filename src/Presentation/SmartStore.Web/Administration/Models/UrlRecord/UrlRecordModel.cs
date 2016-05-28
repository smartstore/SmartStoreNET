using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.UrlRecord
{
    public partial class UrlRecordModel : EntityModelBase
    {
        [Required, AllowHtml, SmartResourceDisplayName("Admin.System.SeNames.Name")]
        public string Slug { get; set; }

        [Required, SmartResourceDisplayName("Admin.System.SeNames.EntityName")]
        public string EntityName { get; set; }

		[Required, SmartResourceDisplayName("Admin.System.SeNames.EntityId")]
		public int EntityId { get; set; }

		[SmartResourceDisplayName("Admin.Common.Entity")]
		public string EntityUrl { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.IsActive")]
        public bool IsActive { get; set; }

		[SmartResourceDisplayName("Admin.System.SeNames.Language")]
		public int LanguageId { get; set; }
		public List<SelectListItem> AvailableLanguages { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Language")]
        public string Language { get; set; }

		[SmartResourceDisplayName("Admin.System.SeNames.SlugsPerEntity")]
		public int SlugsPerEntity { get; set; }
    }
}