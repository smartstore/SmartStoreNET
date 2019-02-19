using System;

namespace SmartStore.Services.Cms
{
    [Serializable]
    public partial class LinkResolverResult
    {
        public LinkResolverResult(LinkType type, object value)
        {
            Type = type;
            Value = value;
        }

        public static implicit operator string (LinkResolverResult obj)
        {
            if (obj != null)
            {
                return obj.Result.EmptyNull();
            }

            return string.Empty;
        }

        public LinkType Type { get; private set; }

        public object Value { get; private set; }

        public string Result { get; set; }
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
