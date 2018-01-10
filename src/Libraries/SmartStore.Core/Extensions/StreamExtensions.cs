using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Diagnostics;

namespace SmartStore
{
	public static class StreamExtensions
	{
		public static StreamReader ToStreamReader(this Stream stream, bool leaveOpen)
		{
			return new StreamReader(stream, Encoding.UTF8, true, 0x400, leaveOpen);
		}

		public static StreamReader ToStreamReader(this Stream stream, Encoding encoding, bool detectEncoding, int bufferSize, bool leaveOpen)
		{
			return new StreamReader(stream, encoding, detectEncoding, bufferSize, leaveOpen);
		}

		public static bool ToFile(this Stream srcStream, string path) 
        {
			if (srcStream == null)
				return false;

			const int BuffSize = 32768;
			var result = true;
			Stream dstStream = null;
			var buffer = new byte[BuffSize];

			try 
            {
                using (dstStream = File.Open(path, FileMode.Create))
                {
					int len;
                    while ((len = srcStream.Read(buffer, 0, BuffSize)) > 0)
                    {
                        dstStream.Write(buffer, 0, len);
                    }
				}
            }
			catch 
            {
				result = false;
			}
			finally
			{
				if (dstStream != null)
				{
					dstStream.Close();
					dstStream.Dispose();
				}
			}

			return (result && File.Exists(path));
		}

        public static bool ContentsEqual(this Stream src, Stream other) 
        {
            Guard.NotNull(src, nameof(src));
            Guard.NotNull(other, nameof(other));

            if (src.Length != other.Length)
                return false;

            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];
            
            while (true)
            {
                int len1 = src.Read(buffer1, 0, bufferSize);
                int len2 = other.Read(buffer2, 0, bufferSize);

                if (len1 != len2)
                    return false;

                if (len1 == 0)
                    return true;

                int iterations = (int)Math.Ceiling((double)len1 / sizeof(Int64));
                for (int i = 0; i < iterations; i++)
                {
                    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                    {
                        return false;
                    }
                }
            }
        }

	}
    
}
