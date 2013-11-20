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
        private readonly int _storeId;

        public ThemeVarsVirtualFile(string virtualPath, int storeId)
            : base(virtualPath)
        {
            _storeId = storeId;
        }

        public override bool IsDirectory
        {
            get { return false; }
        }
        
        public override Stream Open()
        {
            var repo = new ThemeVarsRepository();
            var parameters = repo.GetParameters(_storeId);
            var lessCss = TransformToLess(parameters);

            return GenerateStreamFromString(lessCss);
        }

        private Stream GenerateStreamFromString(string value)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Unicode);
            writer.Write(value);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        
        private string TransformToLess(IDictionary<string, string> parameters)
        {
            if (parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var parameter in parameters.Where(ValueIsNotNullOrEmpty))
            {
                sb.AppendFormat("@{0}: {1};\n", parameter.Key, parameter.Value);
            }

            return sb.ToString();
        }

        private static bool ValueIsNotNullOrEmpty(KeyValuePair<string, string> kvp)
        {
            return !string.IsNullOrEmpty(kvp.Value);
        }

    }
}