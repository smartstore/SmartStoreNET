using SmartStore.Core.Configuration;

namespace SmartStore.Core.IO
{
    public class FileSystemSettings : ISettings
    {
        public string DirectoryName { get; set; }
    }
}