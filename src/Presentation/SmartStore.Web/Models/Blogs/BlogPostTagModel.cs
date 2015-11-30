using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Blogs
{
    public partial class BlogPostTagModel : ModelBase
    {
        public string Name { get; set; }

        public int BlogPostCount { get; set; }
    }
}