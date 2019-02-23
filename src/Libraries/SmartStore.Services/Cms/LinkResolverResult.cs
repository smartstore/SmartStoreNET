using System;

namespace SmartStore.Services.Cms
{
    public enum LinkType
    {
        Product = 0,
        Category,
        Manufacturer,
        Topic,
        Url = 20,
        File = 30
    }

    public enum LinkStatus
    {
        Ok,
		Forbidden,
		NotFound,
        Hidden
    }


    public partial class LinkResolverResult
    {
        public LinkType Type { get; set; }
        public object Value { get; set; }
        public LinkStatus Status { get; set; }

        public string Link { get; set; }
        public string Label { get; set; }
    }

    [Serializable]
    public partial class LinkResolverData
    {
        public LinkType Type { get; set; }
        public object Value { get; set; }
        public LinkStatus Status { get; set; }

        public string Link { get; set; }
        public string Label { get; set; }

        public int Id { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
    }
}
