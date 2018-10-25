using System;
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
                foreach (ExportEntityType type in Enum.GetValues(typeof(ExportEntityType)))
                {
                    if (type >= ExportEntityType.TierPrice && fileName.EndsWith(type.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        IsRelatedData = true;
                        EntityType = type;
                        break;
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
        /// Indicates whether the file contains related data.
        /// </summary>
        public bool IsRelatedData { get; private set; }

        /// <summary>
        /// Entity type. Only known for related data.
        /// </summary>
        public ExportEntityType? EntityType { get; private set; }

        /// <summary>
        /// File label text.
        /// </summary>
        public string Label { get; set; }
    }
}
