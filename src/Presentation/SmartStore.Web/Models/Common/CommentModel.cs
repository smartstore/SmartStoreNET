using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Customer;

namespace SmartStore.Web.Models.Common
{
    public partial class CommentListModel : ModelBase
    {
        public CommentListModel()
        {
            Comments = new List<CommentModel>();
        }

        public bool AllowComments { get; set; }
        public int NumberOfComments { get; set; }
        public IList<CommentModel> Comments { get; set; }
        public bool AllowCustomersToUploadAvatars { get; set; }
    }

    public partial class CommentModel : EntityModelBase
    {
        private readonly WeakReference<CommentListModel> _parent;

        public CommentModel(CommentListModel parent)
        {
            Guard.NotNull(parent, nameof(parent));

            Avatar = new CustomerAvatarModel();
            _parent = new WeakReference<CommentListModel>(parent);
        }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CommentTitle { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnPretty { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public CustomerAvatarModel Avatar { get; set; }

        public CommentListModel Parent
        {
            get
            {
                _parent.TryGetTarget(out var parent);
                return parent;
            }
        }
    }
}