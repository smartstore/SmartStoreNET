using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Profile
{
    public partial class ProfileIndexModel : EntityModelBase
    {
        public string ProfileTitle { get; set; }
        public int PostsPage { get; set; }
        public bool PagingPosts { get; set; }
        public bool ForumsEnabled { get; set; }
    }
}