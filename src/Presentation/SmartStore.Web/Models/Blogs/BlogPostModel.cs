using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Validators.Blogs;

namespace SmartStore.Web.Models.Blogs
{
    [Validator(typeof(BlogPostValidator))]
    public partial class BlogPostModel : EntityModelBase
    {
        public BlogPostModel()
        {
            Tags = new List<BlogPostTagModel>();
			AddNewComment = new AddBlogCommentModel();
			Comments = new CommentListModel();
		}

        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
		public DateTime CreatedOn { get; set; }
		public IList<BlogPostTagModel> Tags { get; set; }

		public string Title { get; set; }
        public string Body { get; set; }

		public AddBlogCommentModel AddNewComment { get; set; }
		public CommentListModel Comments { get; set; }
	}
}