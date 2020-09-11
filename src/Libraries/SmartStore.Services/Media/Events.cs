using System;
using System.Drawing;
using System.IO;
using System.Web;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Published for every uploaded image which does NOT exceed maximum
    /// allowed size. This gives subscribers the chance to still process the image,
    /// e.g. to achive better compression before saving image data to storage. 
    /// This event does NOT get published when the uploaded image is about to be processed anyway.
    /// </summary>
    /// <remarks>
    /// A subscriber should NOT resize the image. But if you do - and you shouldn't :-) - , don't forget to set <see cref="ResultSize"/>.
    /// </remarks>
    public class ImageUploadedEvent
    {
        public ImageUploadedEvent(ProcessImageQuery query, Size size)
        {
            Query = query;
            Size = size;
        }

        /// <summary>
        /// Contains the source (as byte[], STream or path string), max size, format and default image quality instructions.
        /// </summary>
        public ProcessImageQuery Query { get; private set; }

        /// <summary>
        /// The original size of the uploaded image. May be empty.
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// The processing result. If null, the original data
        /// from <c>Query.Source</c> will be put to storage.
        /// </summary>
        public IImage ResultImage { get; set; }
    }

    /// <summary>
    /// Published after image query has been created and initialized
    /// by the media middleware controller with data from HttpContent.Request.QueryString.
    /// This event implies that a thumbnail is about to be created.
    /// </summary>
    public class ImageQueryCreatedEvent
    {
        public ImageQueryCreatedEvent(ProcessImageQuery query, HttpContextBase httpContext, string mimeType, string extension)
        {
            Query = query;
            HttpContext = httpContext;
            MimeType = mimeType;
            Extension = extension;
        }

        public ProcessImageQuery Query { get; private set; }
        public HttpContextBase HttpContext { get; private set; }
        public string MimeType { get; private set; }
        public string Extension { get; private set; }
    }

    /// <summary>
    /// Published before processing begins, but after the source has been loaded.
    /// </summary>
    public class ImageProcessingEvent
    {
        public ImageProcessingEvent(ProcessImageQuery query, IImage image)
        {
            Query = query;
            Image = image;
        }

        public ProcessImageQuery Query { get; private set; }
        public IImage Image { get; private set; }
    }

    /// <summary>
    /// Published after processing finishes.
    /// </summary>
    public class ImageProcessedEvent
    {
        public ImageProcessedEvent(ProcessImageQuery query, ProcessImageResult result)
        {
            Query = query;
            Result = result;
        }

        public ProcessImageQuery Query { get; private set; }
        public ProcessImageResult Result { get; private set; }
    }
}
