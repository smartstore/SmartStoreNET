using System.IO;
using System.Linq;
using ImageResizer;
using ImageResizer.Configuration;
using ImageResizer.Plugins.PrettyGifs;

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
            Guard.NotNull(source, nameof(source));

			var resultStream = new MemoryStream();
			var resizeSettings = ImageResizerUtil.CreateResizeSettings(settings);

			if (source.Length != 0)
			{
				if (quality.HasValue)
					resizeSettings.Quality = quality.Value;
				if (maxHeight.HasValue)
					resizeSettings.MaxHeight = maxHeight.Value;
				if (maxWidth.HasValue)
					resizeSettings.MaxWidth = maxWidth.Value;

				ImageBuilder.Current.Build(source, resultStream, resizeSettings);
			}

            return resultStream;
        }

    }

}
