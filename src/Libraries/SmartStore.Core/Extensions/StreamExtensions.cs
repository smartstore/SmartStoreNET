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
		
        public static bool ToFile(this Stream srcStream, string path) 
        {
			if (srcStream == null)
				return false;

			const int BuffSize = 32768;
			bool result = true;
			int len = 0;
			Stream dstStream = null;
			byte[] buffer = new Byte[BuffSize];

			try 
            {
				using (dstStream = File.OpenWrite(path)) 
                {
					while ((len = srcStream.Read(buffer, 0, BuffSize)) > 0)
						dstStream.Write(buffer, 0, len);
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

			return (result && System.IO.File.Exists(path));
		}

        public static bool ContentsEqual(this Stream src, Stream other) 
        {
            Guard.ArgumentNotNull(() => src);
            Guard.ArgumentNotNull(() => other);

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
