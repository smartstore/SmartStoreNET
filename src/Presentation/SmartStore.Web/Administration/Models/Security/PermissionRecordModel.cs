using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Security
{
    public class PermissionRecordModel : ModelBase
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
		public string Category { get; set; }

		public string CategoryLabel
		{
			get
			{
				switch (Category)
				{
					case "PublicStore":
					case "Standard":
						return "label-success";

					case "Plugin":
						return "";

					default:
						return "label-info";
				}
			}
		}
	}
}