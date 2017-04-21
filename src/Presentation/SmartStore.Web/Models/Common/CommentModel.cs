using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
	public partial class CommentListModel : ModelBase
	{
		private bool? _hasAnyAvatar;

		public CommentListModel()
		{
			Comments = new List<CommentModel>();
		}

		public bool AllowComments { get; set; }
		public int NumberOfComments { get; set; }
		public IList<CommentModel> Comments { get; set; }
		public bool AllowCustomersToUploadAvatars { get; set; }
		public int AvatarPictureSize { get; set; }

		public bool HasAnyAvatar()
		{
			if (_hasAnyAvatar == null)
			{
				_hasAnyAvatar = this.AllowCustomersToUploadAvatars && Comments.Any(x => x.CustomerAvatarUrl.HasValue());
			}

			return _hasAnyAvatar.Value;
		}
	}

	public partial class CommentModel : EntityModelBase
    {
		private readonly WeakReference<CommentListModel> _parent;

		public CommentModel(CommentListModel parent)
		{
			Guard.NotNull(parent, nameof(parent));

			_parent = new WeakReference<CommentListModel>(parent);
		}

		public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAvatarUrl { get; set; }
        public string CommentTitle { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedOn { get; set; }
		public string CreatedOnPretty { get; set; }
		public bool AllowViewingProfiles { get; set; }

		public CommentListModel Parent
		{
			get
			{
				CommentListModel parent;
				_parent.TryGetTarget(out parent);
				return parent;
			}
		}
    }
}