using System;

namespace SmartStore.Services.Media
{
	public sealed class ProcessImageException : Exception
	{
		public ProcessImageException()
			: this((string)null, null)
		{
		}

		public ProcessImageException(ProcessImageQuery query)
			: this(query, null)
		{
		}

		public ProcessImageException(string message, ProcessImageQuery query)
			: base(message)
		{
			Query = query;
		}

		public ProcessImageException(ProcessImageQuery query, Exception innerException)
			: base(CreateMessage(query), innerException)
		{
			Query = query;
		}

		private static string CreateMessage(ProcessImageQuery query)
		{
			var fileName = query?.FileName;

			if (fileName.HasValue())
			{
				return "Error while processing image '{0}'.".FormatCurrent(fileName);
			}
			else
			{
				return "Error while processing image.";
			}
		}

		public ProcessImageQuery Query { get; private set; }
	}
}
