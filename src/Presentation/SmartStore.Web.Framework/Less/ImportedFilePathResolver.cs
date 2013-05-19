using System.IO;
using System.Web;
using System.Web.Hosting;
using dotless.Core.Input;

namespace SmartStore.Web.Framework.Less
{
    public class ImportedFilePathResolver : IPathResolver
    {
        private string _currentFileDirectory;
        private string _currentFilePath;

        public ImportedFilePathResolver(string currentFilePath)
        {
            CurrentFilePath = currentFilePath;
        }

        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        public string CurrentFilePath
        {
            get { return _currentFilePath; }
            set
            {
                _currentFilePath = value;
                _currentFileDirectory = Path.GetDirectoryName(value);
            }
        }

        /// <summary>
        /// Returns the absolute path for the specified improted file path.
        /// </summary>
        /// <param name="filePath">The imported file path.</param>
        public string GetFullPath(string filePath)
        {
            filePath = filePath.Replace('\\', '/').Trim();

            if (filePath.StartsWith("~"))
            {
                filePath = VirtualPathUtility.ToAbsolute(filePath);
            }

            if (filePath.StartsWith("/"))
            {
                filePath = HostingEnvironment.MapPath(filePath);
            }
            else if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(_currentFileDirectory, filePath);
            }

            return filePath;
        }
    }
}
