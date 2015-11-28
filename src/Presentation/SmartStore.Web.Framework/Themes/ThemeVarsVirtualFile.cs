using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Themes
{
    public class ThemeVarsVirtualFile : VirtualFile
    {
        private readonly string _themeName;
        private readonly int _storeId;

        public ThemeVarsVirtualFile(string virtualPath, string themeName, int storeId)
            : base(virtualPath)
        {
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

            var lessCss = repo.GetVariablesAsLess(_themeName, _storeId);
            return GenerateStreamFromString(lessCss);
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