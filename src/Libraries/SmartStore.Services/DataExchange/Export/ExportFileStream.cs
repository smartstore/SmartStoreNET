using System.IO;

namespace SmartStore.Services.DataExchange.Export
{
    public class ExportFileStream : Stream
    {
        private const long FLUSH_WRITE_BYTES = 1024 * 1024 * 4;  // Flush to disk each 4 MB.

        private readonly string _filePath;
        private Stream _stream;
        private FileStream _fsStream;
        private long _count;

        public ExportFileStream(string filePath)
        {
            _filePath = filePath;
        }

        protected Stream Stream
        {
            get
            {
                if (_stream == null)
                {
                    if (_filePath.HasValue())
                    {
                        _stream = _fsStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                    }
                    else
                    {
                        _stream = new MemoryStream();
                    }
                }

                return _stream;
            }
        }

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        public override void Flush()
        {
            Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);

            _count += count;

            if (_count > FLUSH_WRITE_BYTES)
            {
                _count = 0;

                if (_fsStream != null)
                {
                    _fsStream.Flush(true);
                }
                else
                {
                    Stream.Flush();
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
