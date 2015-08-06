using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.DataExchange
{
	public class ExportPreviewModel : EntityModelBase
	{
		public string Name { get; set; }
		public string ThumbnailUrl { get; set; }
	}
}