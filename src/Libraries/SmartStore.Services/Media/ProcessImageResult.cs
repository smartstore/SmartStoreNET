using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
	public class ProcessImageResult : DisposableObject
	{
		public ProcessImageQuery Query { get; set; }

		public MemoryStream Result { get; set; }

		public int? SourceWidth { get; set; }
		public int? SourceHeight { get; set; }

		public string FileExtension { get; set; }
		public string MimeType { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public long ProcessTimeMs { get; set; }

		protected override void OnDispose(bool disposing)
		{
			if (disposing && Result != null)
			{
				Result.Dispose();
				Result = null;
			}
		}
	}
}
