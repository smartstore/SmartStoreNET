using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Validators.News;

namespace SmartStore.Web.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : EntityModelBase
    {
        public NewsItemModel()
        {
            Comments = new List<NewsCommentModel>();
            AddNewComment = new AddNewsCommentModel();
        }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public string Title { get; set; }

        public string Short { get; set; }

        public string Full { get; set; }

        public bool AllowComments { get; set; }

        public int NumberOfComments { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<NewsCommentModel> Comments { get; set; }
        public AddNewsCommentModel AddNewComment { get; set; }

        public int AvatarPictureSize { get; set; }
		public bool AllowCustomersToUploadAvatars { get; set; }
    }
}