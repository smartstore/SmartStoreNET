using System;
using System.IO;

namespace SmartStore.Services.Media.Imaging
{
    public class ProcessImageResult : DisposableObject
    {
        public ProcessImageQuery Query { get; set; }

        public IImageFormat SourceFormat { get; set; }
        public IImage Image { get; set; }
        internal bool DisposeImage { get; set; }

        /// <summary>
        /// Is <c>true</c> if any effect has been applied that changed the image visually (like background color, contrast, sharpness etc.).
        /// Resize and compression quality does NOT count as FX.
        /// </summary>
        public bool HasAppliedVisualEffects { get; set; }

        public long ProcessTimeMs { get; set; }

        protected override void OnDispose(bool disposing)
        {
            if (disposing && DisposeImage && Image != null)
            {
                Image.Dispose();
                Image = null;
            }
        }
    }
}
