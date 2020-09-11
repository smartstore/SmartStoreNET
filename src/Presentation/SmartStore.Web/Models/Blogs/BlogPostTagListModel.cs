using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Blogs
{
    public partial class BlogPostTagListModel : ModelBase
    {
        public BlogPostTagListModel()
        {
            Tags = new List<BlogPostTagModel>();
        }

        public IList<BlogPostTagModel> Tags { get; set; }
    }
}