using System;
using ImageResizer;
using ImageResizer.Configuration;

namespace SmartStore.Services.Media
{
    internal static class ImageResizerUtils
    {
        public static ResizeSettings CreateResizeSettings(object settings)
        {
            ResizeSettings resizeSettings;

            if (settings is string)
            {
                resizeSettings = new ResizeSettings((string)settings);
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
