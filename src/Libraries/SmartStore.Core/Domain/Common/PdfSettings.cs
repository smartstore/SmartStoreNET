
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class PdfSettings : ISettings
    {
		public PdfSettings()
		{
			Enabled = true;
			RenderOrderNotes = true;
		}
		
		/// <summary>
        /// Gets or sets a value indicating PDF is supported
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// PDF logo picture identifier
        /// </summary>
        public int LogoPictureId { get; set; }

        /// <summary>
        /// Gets or sets whether letter page size is enabled
        /// </summary>
        public bool LetterPageSizeEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render order notes in PDf reports
        /// </summary>
        public bool RenderOrderNotes { get; set; }
    }
}