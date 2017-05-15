using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Validators.News;

namespace SmartStore.Web.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : EntityModelBase
    {
        public NewsItemModel()
        {
            AddNewComment = new AddNewsCommentModel();
			Comments = new CommentListModel();
		}
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
		public DateTime CreatedOn { get; set; }

		public string Title { get; set; }
        public string Short { get; set; }
        public string Full { get; set; }

		public AddNewsCommentModel AddNewComment { get; set; }
		public CommentListModel Comments { get; set; }
    }
}