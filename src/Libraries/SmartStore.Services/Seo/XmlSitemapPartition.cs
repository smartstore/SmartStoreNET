using System;
using System.IO;

namespace SmartStore.Services.Seo
{
    public class XmlSitemapPartition
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int StoreId { get; set; }
        public int LanguageId { get; set; }
        public DateTime ModifiedOnUtc { get; set; }
        public Stream Stream { get; set; }
    }
}
