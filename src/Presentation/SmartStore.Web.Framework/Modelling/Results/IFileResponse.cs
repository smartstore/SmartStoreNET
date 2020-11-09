using System;

namespace SmartStore.Web.Framework.Modelling
{
    public interface IFileResponse
    {
        string ContentType { get; }
        long? FileLength { get; }
        //Func<Size?> LazyDimensions { get; }
        DateTime LastModifiedUtc { get; }
        TimeSpan MaxAge { get; }
        string ETag { get; }
        FileTransmitter Transmitter { get; }
    }
}
