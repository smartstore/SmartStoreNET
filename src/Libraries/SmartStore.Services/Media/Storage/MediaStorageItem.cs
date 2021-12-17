using System;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media.Storage
{
    public abstract class MediaStorageItem : IDisposable
    {
        private Stream _sourceStream;

        public Stream SourceStream
        {
            get
            {
                if (_sourceStream == null)
                {
                    _sourceStream = GetSourceStream();
                }

                if (_sourceStream.CanSeek)
                {
                    _sourceStream.Position = 0;
                }

                return _sourceStream;
            }
        }

        protected abstract Stream GetSourceStream();
        public abstract void SaveTo(Stream stream, MediaFile mediaFile);
        public abstract Task SaveToAsync(Stream stream, MediaFile mediaFile);

        protected static int GetLength(Stream stream)
        {
            if (stream.CanSeek)
            {
                return (int)stream.Length;
            }
            else if (stream.Position > 0) 
            {
                return (int)stream.Position;
            }

            return 0;
        }

        public virtual void Dispose()
        {
            if (_sourceStream != null)
            {
                _sourceStream.Dispose();
                _sourceStream = null;
            }
        }

        #region Factories

        public static MediaStorageItem FromImage(IImage image)
        {
            return new ImageStorageItem(image);
        }

        public static MediaStorageItem FromStream(Stream stream)
        {
            return new StreamStorageItem(stream);
        }

        public static MediaStorageItem FromFile(IFile file)
        {
            return new StreamStorageItem(file.OpenRead());
        }

        #endregion

        #region Impls

        public class ImageStorageItem : MediaStorageItem
        {
            private readonly IImage _image;

            public ImageStorageItem(IImage image)
            {
                _image = image;
            }

            protected override Stream GetSourceStream()
            {
                var memStream = new MemoryStream();
                _image.Save(memStream);
                return memStream;
            }

            public override void SaveTo(Stream stream, MediaFile mediaFile)
            {
                _image.Save(stream);
                mediaFile.Size = GetLength(stream);
            }

            public override Task SaveToAsync(Stream stream, MediaFile mediaFile)
            {
                _image.Save(stream);
                mediaFile.Size = GetLength(stream);
                return Task.CompletedTask;
            }
        }

        public class StreamStorageItem : MediaStorageItem
        {
            private readonly Stream _stream;

            public StreamStorageItem(Stream stream)
            {
                _stream = stream;
            }

            protected override Stream GetSourceStream()
            {
                return _stream;
            }

            public override void SaveTo(Stream stream, MediaFile mediaFile)
            {
                if (stream.CanSeek)
                {
                    stream.SetLength(0);
                }

                SourceStream.CopyTo(stream);

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                mediaFile.Size = GetLength(stream);
            }

            public override async Task SaveToAsync(Stream stream, MediaFile mediaFile)
            {
                if (stream.CanSeek)
                {
                    stream.SetLength(0);
                }

                await SourceStream.CopyToAsync(stream);

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                mediaFile.Size = GetLength(stream);
            }
        }

        #endregion
    }
}
