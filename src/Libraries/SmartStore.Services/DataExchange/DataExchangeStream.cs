using System.IO;

namespace SmartStore.Services.DataExchange
{
	public class DataExchangeStream
	{
		private FileStream _stream;

		/// <summary>
		/// The extension of the file
		/// </summary>
		public string FileExtension { get; internal set; }

		/// <summary>
		/// Number of items written to file
		/// </summary>
		public int ItemCount { get; internal set; }

		/// <summary>
		/// <c>True</c> if no data has been written to the file yet
		/// </summary>
		public bool IsBeginOfFile { get; internal set; }

		/// <summary>
		/// <c>True</c> if no more data will be written to the file
		/// </summary>
		public bool IsEndOfFile { get; internal set; }

		/// <summary>
		/// <c>True</c> if file is open for writing
		/// </summary>
		public bool IsOpen { get { return _stream != null; } }

		internal void Open(string newPath)
		{
			Close();

			if (newPath.HasValue())
			{
				ItemCount = 0;
				IsBeginOfFile = true;
				IsEndOfFile = false;
				FileExtension = Path.GetExtension(newPath);

				_stream = new FileStream(newPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			}
		}

		internal void Close()
		{
			if (_stream != null)
			{
				FileExtension = null;
				ItemCount = 0;
				IsBeginOfFile = IsEndOfFile = false;

				_stream.Close();
				_stream.Dispose();
				_stream = null;
			}
		}
	}
}
