using System;

namespace SmartStore.Services.Cms
{
    [Serializable]
    public partial class LinkResolverResult
    {
        public LinkType Type { get; set; }
        public object Value { get; set; }

        public string Link { get; set; }
        public string Label { get; set; }
    }


    public enum LinkType
    {
        Product = 0,
        Category,
        Manufacturer,
        Topic,
        Url = 20,
        File = 30
    }
}
