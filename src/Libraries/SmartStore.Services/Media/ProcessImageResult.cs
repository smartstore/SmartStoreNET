using System;
using System.IO;

namespace SmartStore.Services.Media
{
	public class ProcessImageResult : DisposableObject
	{
		public ProcessImageQuery Query { get; set; }

		public MemoryStream OutputStream { get; set; }

		public int? SourceWidth { get; set; }
		public int? SourceHeight { get; set; }
		public string SourceMimeType { get; set; }

		public string FileExtension { get; set; }
		public string MimeType { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		/// <summary>
		/// Is <c>true</c> if any effect has been applied that changed the image visually (like background color, contrast, sharpness etc.).
		/// Resize and compression quality does NOT count as FX.
		/// </summary>
		public bool HasAppliedVisualEffects { get; set; }

		public long ProcessTimeMs { get; set; }

		protected override void OnDispose(bool disposing)
		{
			if (disposing && OutputStream != null)
			{
				OutputStream.Dispose();
				OutputStream = null;
			}
		}
	}
}
