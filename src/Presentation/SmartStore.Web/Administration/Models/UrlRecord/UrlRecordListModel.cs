using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.UrlRecord
{
    public partial class UrlRecordListModel : ModelBase
    {
		public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
        public string SeName { get; set; }

		[SmartResourceDisplayName("Admin.Common.Entity")]
		public string EntityName { get; set; }

		[SmartResourceDisplayName("Common.IsActive")]
		public bool? IsActive { get; set; }
    }
}