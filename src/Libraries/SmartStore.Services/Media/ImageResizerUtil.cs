using System;
using System.Collections.Specialized;
using ImageResizer;
using ImageResizer.Configuration;

namespace SmartStore.Services.Media
{
    internal static class ImageResizerUtil
    {
        public static ResizeSettings CreateResizeSettings(object settings)
        {
            ResizeSettings resizeSettings;

            if (settings is string)
            {
                resizeSettings = new ResizeSettings((string)settings);
            }
			else if (settings is NameValueCollection)
			{
				resizeSettings = new ResizeSettings((NameValueCollection)settings);
			}
			else if (settings is ResizeSettings)
            {
                resizeSettings = (ResizeSettings)settings;
            }
            else
            {
                resizeSettings = new ResizeSettings();
            }

            return resizeSettings;
        }
    }
}
