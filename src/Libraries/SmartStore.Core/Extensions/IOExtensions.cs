using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore
{
	public static class IOExtensions
	{		
		public static bool IsFileLocked(this FileInfo file)
		{
			if (file == null)
				return false;

			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				// the file is unavailable because it is:
				// still being written to
				// or being processed by another thread
				// or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
		}
	}
}
