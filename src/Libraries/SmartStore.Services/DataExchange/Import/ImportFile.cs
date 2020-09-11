using System;
using System.Linq;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Import
{
    public partial class ImportFile
    {
        public ImportFile(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            Path = path;

            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (fileName.HasValue())
            {
                foreach (RelatedEntityType type in Enum.GetValues(typeof(RelatedEntityType)))
                {
                    if (fileName.EndsWith(type.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        RelatedType = type;
                    }
                }
            }
        }

        /// <summary>
        /// Path of the import file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// File name of <see cref="Path"/>.
        /// </summary>
        public string Name => System.IO.Path.GetFileName(Path);

        /// <summary>
        /// File extension of <see cref="Path"/>.
        /// </summary>
        public string Extension => System.IO.Path.GetExtension(Path);

        /// <summary>
        /// Indicates whether the file has an CSV file extension.
        /// </summary>
        internal bool IsCsv => (new string[] { ".csv", ".txt", ".tab" }).Contains(Extension, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Related entity type.
        /// </summary>
        public RelatedEntityType? RelatedType { get; private set; }

        /// <summary>
        /// File label text.
        /// </summary>
        public string Label { get; set; }
    }
}
