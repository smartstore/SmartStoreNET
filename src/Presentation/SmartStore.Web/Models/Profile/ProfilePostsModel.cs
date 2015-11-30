using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Profile
{
    public partial class ProfilePostsModel : PagedListBase
    {
        public ProfilePostsModel(IPageable pageable) : base(pageable)
        {
        }
        
        public IList<PostsModel> Posts { get; set; }
    }
}