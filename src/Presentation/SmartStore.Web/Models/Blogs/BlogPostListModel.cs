using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Blogs
{
    public partial class BlogPostListModel : ModelBase
    {
        public BlogPostListModel()
        {
            PagingFilteringContext = new BlogPagingFilteringModel();
            BlogPosts = new List<BlogPostModel>();
        }

        public int WorkingLanguageId { get; set; }
        public BlogPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<BlogPostModel> BlogPosts { get; set; }
    }
}