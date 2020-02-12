using System;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Seo
{
    public class GenericPath : IComparable<GenericPath>, IEquatable<GenericPath>
    {
        public string EntityName { get; set; }
        public Route Route { get; set; }
        public string IdParamName { get; set; }
        public int Order { get; set; }

        public int CompareTo(GenericPath other)
        {
            return this.Order.CompareTo(other.Order);
        }

        public bool Equals(GenericPath other)
        {
            return other != null && this.EntityName.IsCaseInsensitiveEqual(other.EntityName);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GenericPath);
        }

        public override int GetHashCode()
        {
            return this.EntityName.ToLowerInvariant().GetHashCode();
        }
    }
}
