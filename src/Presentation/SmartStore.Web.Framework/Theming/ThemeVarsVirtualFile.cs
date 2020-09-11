using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Theming
{
    public class ThemeVarsVirtualFile : VirtualFile, IFileDependencyProvider
    {
        public ThemeVarsVirtualFile(string virtualPath, string themeName, int storeId)
            : this(virtualPath, Path.GetExtension(virtualPath), themeName, storeId)
        {
        }

        internal ThemeVarsVirtualFile(string virtualPath, string extension, string themeName, int storeId)
            : base(virtualPath)
        {
            Extension = extension;
            ThemeName = themeName;
            StoreId = storeId;
        }

        public override bool IsDirectory => false;

        public string Extension { get; private set; }
        public string ThemeName { get; private set; }
        public int StoreId { get; private set; }

        public override Stream Open()
        {
            var repo = new ThemeVarsRepository();

            if (ThemeName.IsEmpty())
                return GenerateStreamFromString(string.Empty);

            var css = repo.GetPreprocessorCss(Extension, ThemeName, StoreId);
            return GenerateStreamFromString(css);
        }

        private Stream GenerateStreamFromString(string value)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.Unicode, 1024, true))
            {
                writer.Write(value);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        public void AddFileDependencies(ICollection<string> mappedPaths, ICollection<string> cacheKeys)
        {
            cacheKeys.Add(FrameworkCacheConsumer.BuildThemeVarsCacheKey(ThemeName, StoreId));
        }
    }
}