using System;
using System.Drawing;

namespace SmartStore.Web.Framework.Modelling
{
	public interface IFileResponse
	{
		string ContentType { get; }
		long? FileLength { get; }
		Size? Dimensions { get; }
		DateTime LastModifiedUtc { get; }
		TimeSpan MaxAge { get; }
		string ETag { get; }
		FileTransmitter Transmitter { get; }
	}
}
