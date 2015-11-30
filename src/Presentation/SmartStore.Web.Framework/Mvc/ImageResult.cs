using System.IO;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Media;

namespace SmartStore.Web.Framework.Mvc
{
    public class PictureResult : FileStreamResult
    {
        public PictureResult(Picture picture, int targetSize)
            : base(
         GetMemoryStream(picture, targetSize), picture.MimeType)
        {

        }

        static MemoryStream GetMemoryStream(Picture picture, int targetSize)
        {
            var localUrl = EngineContext.Current.Resolve<IPictureService>().GetThumbLocalPath(picture, targetSize);
            var fileStream = new FileStream(localUrl, FileMode.Open, FileAccess.Read, FileShare.Read);
            var memoryStream = StreamToMemory(fileStream);
            fileStream.Close();
            return memoryStream;
        }

        static MemoryStream StreamToMemory(Stream input)
        {

            byte[] buffer = new byte[1024];
            int count = 1024;
            MemoryStream output;

            // build a new stream
            if (input.CanSeek)
            {
                output = new MemoryStream((int)input.Length);
            }
            else
            {
                output = new MemoryStream();
            }

            // iterate stream and transfer to memory stream
            do
            {
                count = input.Read(buffer, 0, count);
                if (count == 0)
                    break; // TODO: might not be correct. Was : Exit Do
                output.Write(buffer, 0, count);
            } while (true);

            // rewind stream
            output.Position = 0;

            // pass back
            return output;

        }

    }
}
