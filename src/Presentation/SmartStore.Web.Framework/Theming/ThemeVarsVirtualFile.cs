using System.IO;
using System.Text;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Theming
{
    public class ThemeVarsVirtualFile : VirtualFile
    {
		private readonly string _extension;
		private readonly string _themeName;
        private readonly int _storeId;	

        public ThemeVarsVirtualFile(string virtualPath, string themeName, int storeId)
            : this(virtualPath, Path.GetExtension(virtualPath), themeName, storeId)
        {
        }

		internal ThemeVarsVirtualFile(string virtualPath, string extension, string themeName, int storeId)
			: base(virtualPath)
		{
			_extension = extension;
			_themeName = themeName;
			_storeId = storeId;
		}

		public override bool IsDirectory
        {
            get { return false; }
        }
        
        public override Stream Open()
        {
            var repo = new ThemeVarsRepository();

            if (_themeName.IsEmpty())
                return GenerateStreamFromString(string.Empty);

            var css = repo.GetPreprocessorCss(_extension, _themeName, _storeId);
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

    }
}