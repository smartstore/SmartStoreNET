using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Blogs
{
    public partial class BlogPostTagModel : ModelBase
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public int BlogPostCount { get; set; }
    }
}