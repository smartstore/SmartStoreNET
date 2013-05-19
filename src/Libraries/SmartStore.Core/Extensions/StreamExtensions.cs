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
		public static bool ToFile(this Stream srcStream, string path) {
			if (srcStream == null)
				return false;

			const int BuffSize = 32768;
			bool result = true;
			int len = 0;
			Stream DstStream = null;
			byte[] buffer = new Byte[BuffSize];

			try {
				using (DstStream = File.OpenWrite(path)) {
					while ((len = srcStream.Read(buffer, 0, BuffSize)) > 0)
						DstStream.Write(buffer, 0, len);
				}
			}
			catch (Exception) {
				result = false;
			}
			finally
			{
				if (DstStream != null)
				{
					DstStream.Close();
					DstStream.Dispose();
				}
			}
			return (result && System.IO.File.Exists(path));
		}
	}	// class
}
