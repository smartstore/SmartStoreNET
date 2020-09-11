using System;
using System.IO;
using System.Web;

namespace SmartStore.Web.Framework.Modelling
{
    public abstract class FileTransmitter
    {
        public abstract long GetFileLength();
        public abstract void TransmitFile(long offset, long length, long fileLength, HttpContextBase context);
    }

    public sealed class FileBufferTransmitter : FileTransmitter
    {
        private readonly Func<byte[]> _bufferFactory;
        private byte[] _buffer;

        public FileBufferTransmitter(Func<byte[]> bufferFactory)
        {
            Guard.NotNull(bufferFactory, nameof(bufferFactory));
            _bufferFactory = bufferFactory;
        }

        public override long GetFileLength()
        {
            return GetBuffer().LongLength;
        }

        public override void TransmitFile(long offset, long length, long fileLength, HttpContextBase context)
        {
            context.Response.OutputStream.Write(GetBuffer(), (int)offset, (int)length);
        }

        private byte[] GetBuffer()
        {
            if (_buffer == null)
            {
                _buffer = _bufferFactory();
                if (_buffer == null)
                {
                    throw new NullReferenceException("File buffer cannot be NULL.");
                }
            }

            return _buffer;
        }
    }

    public sealed class FileStreamTransmitter : FileTransmitter
    {
        // Default buffer size as defined in BufferedStream type
        const int DefaultBufferSize = 81920;

        private readonly Func<Stream> _streamFactory;
        private Stream _stream;

        public FileStreamTransmitter(Func<Stream> streamFactory)
        {
            Guard.NotNull(streamFactory, nameof(streamFactory));
            _streamFactory = streamFactory;
        }

        public override long GetFileLength()
        {
            return GetStream().Length;
        }

        public override void TransmitFile(long offset, long length, long fileLength, HttpContextBase context)
        {
            var response = context.Response;
            var stream = GetStream();

            response.BufferOutput = false;
            //response.Buffer = true; // Buffering leads to high RAM usage

            using (stream)
            {
                int bufferSize = (int)Math.Min(DefaultBufferSize, length);
                byte[] buffer = new byte[bufferSize];

                int read;
                long remaining = length;

                if (stream.CanSeek)
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                }
                while ((remaining > 0) && (read = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (response.IsClientConnected)
                    {
                        try
                        {
                            response.OutputStream.Write(buffer, 0, read);
                            remaining -= read;
                        }
                        catch (Exception ex)
                        {
                            var isConClose = ex is HttpException httpEx && httpEx.ErrorCode == -2147023901;
                            if (!isConClose)
                            {
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        remaining = 0;
                        break;
                    }
                }
            }
        }

        private Stream GetStream()
        {
            if (_stream == null)
            {
                _stream = _streamFactory();
                if (_stream == null)
                {
                    throw new NullReferenceException("File stream cannot be NULL.");
                }
            }

            return _stream;
        }
    }
}
