using System;
using System.IO;
using System.Web;

namespace SmartStore.Web.Framework.Filters
{
    /// <summary>
    /// A semi-generic Stream implementation for Response.Filter with
    /// an event interface for handling Content transformations via
    /// Stream or String.    
    /// <remarks>
    /// Use with care for large output as this implementation copies
    /// the output into a memory stream and so increases memory usage.
    /// </remarks>
    /// </summary>    
    public class ResponseFilterStream : Stream
    {
        private readonly HttpContextBase _httpContext;

        /// <summary>
        /// The original stream
        /// </summary>
        private readonly Stream _innerStream;

        /// <summary>
        /// Current position in the original (inner) stream
        /// </summary>
        private long _position;

        /// <summary>
        /// Stream that original content is read into
        /// and then passed to TransformStream function
        /// </summary>
        private MemoryStream _captureStream;

        /// <summary>
        /// Internal pointer that that keeps track of the size
        /// of the captureStream
        /// </summary>
        private int _capturePointer = 0;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="innerStream">Original inner stream</param>
        /// <param name="capacity">Initial capacity of the capture stream</param>
        public ResponseFilterStream(Stream innerStream, HttpContextBase httpContext, int capacity = 5000)
        {
            Guard.NotNull(innerStream, nameof(innerStream));
            Guard.NotNull(httpContext, nameof(httpContext));

            _httpContext = httpContext;
            _innerStream = innerStream;
            _captureStream = new MemoryStream(capacity);
        }


        /// <summary>
        /// Determines whether the stream is captured
        /// </summary>
        private bool IsCaptured => CaptureStream != null || CaptureString != null || TransformStream != null || TransformString != null;

        /// <summary>
        /// Determines whether the Write method is outputting data immediately
        /// or delaying output until Flush() is fired.
        /// </summary>
        private bool IsOutputDelayed => TransformStream != null || TransformString != null;


        /// <summary>
        /// Event that captures Response output and makes it available
        /// as a MemoryStream instance. Output is captured but won't 
        /// affect Response output.
        /// </summary>
        public event Action<MemoryStream> CaptureStream;

        /// <summary>
        /// Event that captures Response output and makes it available
        /// as a string. Output is captured but won't affect Response output.
        /// </summary>
        public event Action<string> CaptureString;

        /// <summary>
        /// Event that allows you to transform the stream as each chunk of
        /// the output is written in the Write() operation of the stream.
        /// This means that it's possible/likely that the input 
        /// buffer will not contain the full response output but only
        /// one of potentially many chunks.
        /// 
        /// This event is called as part of the filter stream's Write() 
        /// operation.
        /// </summary>
        public event Func<byte[], byte[]> TransformWrite;

        /// <summary>
        /// Event that allows you to transform the response stream as
        /// each chunk of bytep[] output is written during the stream's write
        /// operation. This means it's possibly/likely that the string
        /// passed to the handler only contains a portion of the full
        /// output. Typical buffer chunks are around 16k a piece.
        /// 
        /// This event is called as part of the stream's Write operation.
        /// </summary>
        public event Func<string, string> TransformWriteString;

        /// <summary>
        /// This event allows capturing and transformation of the entire 
        /// output stream by caching all write operations and delaying final
        /// response output until Flush() is called on the stream.
        /// </summary>
        public event Func<MemoryStream, MemoryStream> TransformStream;

        /// <summary>
        /// Event that can be hooked up to handle Response.Filter
        /// Transformation. Passed a string that you can modify and
        /// return back as a return value. The modified content
        /// will become the final output.
        /// </summary>
        public event Func<string, string> TransformString;


        protected virtual void OnCaptureStream(MemoryStream ms)
        {
            CaptureStream?.Invoke(ms);
        }


        private void OnCaptureStringInternal(MemoryStream ms)
        {
            if (CaptureString != null)
            {
                var content = _httpContext.Response.ContentEncoding.GetString(ms.ToArray());
                OnCaptureString(content);
            }
        }

        protected virtual void OnCaptureString(string output)
        {
            CaptureString?.Invoke(output);
        }

        protected virtual byte[] OnTransformWrite(byte[] buffer)
        {
            if (TransformWrite != null)
                return TransformWrite(buffer);

            return buffer;
        }

        private byte[] OnTransformWriteStringInternal(byte[] buffer)
        {
            var encoding = _httpContext.Response.ContentEncoding;
            string output = OnTransformWriteString(encoding.GetString(buffer));
            return encoding.GetBytes(output);
        }

        private string OnTransformWriteString(string value)
        {
            if (TransformWriteString != null)
                return TransformWriteString(value);

            return value;
        }


        protected virtual MemoryStream OnTransformCompleteStream(MemoryStream ms)
        {
            if (TransformStream != null)
                return TransformStream(ms);

            return ms;
        }


        /// <summary>
        /// Wrapper method form OnTransformString that handles
        /// stream to string and vice versa conversions
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        internal MemoryStream OnTransformCompleteStringInternal(MemoryStream ms)
        {
            if (TransformString == null)
                return ms;

            var content = _httpContext.Response.ContentEncoding.GetString(ms.ToArray());
            content = TransformString(content);

            var buffer = _httpContext.Response.ContentEncoding.GetBytes(content);
            ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);

            return ms;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        public override long Seek(long offset, SeekOrigin direction)
        {
            return _innerStream.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            _innerStream.SetLength(length);
        }

        public override void Close()
        {
            _innerStream.Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Overriden to capture output written by ASP.NET and captured
        /// into a cached stream that is written out later when Flush() is called.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (IsCaptured)
            {
                // Copy to holding buffer only - we'll write out later
                _captureStream.Write(buffer, 0, count);
                _capturePointer += count;
            }

            // Just transform this buffer
            if (TransformWrite != null)
                buffer = OnTransformWrite(buffer);

            if (TransformWriteString != null)
                buffer = OnTransformWriteStringInternal(buffer);

            if (!IsOutputDelayed)
                _innerStream.Write(buffer, offset, buffer.Length);
        }

        /// <summary>
        /// Override flush by writing out the captured stream data
        /// </summary>
        public override void Flush()
        {

            if (IsCaptured && _captureStream.Length > 0)
            {
                // Check for transform implementations
                _captureStream = OnTransformCompleteStream(_captureStream);
                _captureStream = OnTransformCompleteStringInternal(_captureStream);

                OnCaptureStream(_captureStream);
                OnCaptureStringInternal(_captureStream);

                // Write the stream back out if output was delayed
                if (IsOutputDelayed)
                    _innerStream.Write(_captureStream.ToArray(), 0, (int)_captureStream.Length);

                // Clear the cache once we've written it out
                _captureStream.SetLength(0);
            }

            // default flush behavior
            _innerStream.Flush();
        }
    }
}
