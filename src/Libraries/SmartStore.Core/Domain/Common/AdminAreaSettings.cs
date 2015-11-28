
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class AdminAreaSettings : ISettings
    {
		public AdminAreaSettings()
		{
			GridPageSize = 25;
			DisplayProductPictures = true;
			RichEditorFlavor = "RichEditor";
		}
		
		public int GridPageSize { get; set; }

        public bool DisplayProductPictures { get; set; }

        public string RichEditorFlavor { get; set; }
    }
}