using System.IO;

namespace SmartStore.Services.DataExchange.Export
{
    public class ExportFileStream : Stream
    {
        private readonly Stream _stream;
        private readonly long _flushBytesNumber;
        private long _bytesCounter;

        /// <param name="stream">Stream instance to write to.</param>
        /// <param name="flushBytesNumber">Number of bytes when to write to the hard disk. Default is each 4 MB.</param>
        public ExportFileStream(Stream stream, long flushBytesNumber = 4194304)
        {
            Guard.NotNull(stream, nameof(stream));

            _stream = stream;
            _flushBytesNumber = flushBytesNumber;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);

            if (_flushBytesNumber > 0)
            {
                _bytesCounter += count;

                if (_bytesCounter > _flushBytesNumber)
                {
                    _bytesCounter = 0;

                    if (_stream is FileStream fStream)
                    {
                        fStream.Flush(true);
                    }
                    else
                    {
                        _stream.Flush();
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
