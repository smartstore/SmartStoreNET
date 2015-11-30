using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImageResizer;
using ImageResizer.Configuration;
using ImageResizer.Plugins.PrettyGifs;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Media
{
    
    public class ImageResizerService : IImageResizerService
    {

        static ImageResizerService()
        {
            new PrettyGifs().Install(Config.Current);
        }
        
        public bool IsSupportedImage(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ext != null)
            {
                var extension = ext.Trim('.');
                return ImageBuilder.Current.GetSupportedFileExtensions().Any(x => x == extension);
            }

            return false;
        }
        
        public MemoryStream ResizeImage(Stream source, int? maxWidth = null, int? maxHeight = null, int? quality = 0, object settings = null)
        {
            Guard.ArgumentNotNull(() => source);

            ResizeSettings resizeSettings = ImageResizerUtils.CreateResizeSettings(settings);

            if (quality.HasValue)
                resizeSettings.Quality = quality.Value;
            if (maxHeight.HasValue)
                resizeSettings.MaxHeight = maxHeight.Value;
            if (maxWidth.HasValue)
                resizeSettings.MaxWidth = maxWidth.Value;

            var resultStream = new MemoryStream();
            ImageBuilder.Current.Build(source, resultStream, resizeSettings);
            return resultStream;
        }

    }

}
