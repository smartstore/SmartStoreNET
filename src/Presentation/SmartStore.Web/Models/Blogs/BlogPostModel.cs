using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;

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
            PictureModel = new PictureModel();
            PreviewPictureModel = new PictureModel();
        }

        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public IList<BlogPostTagModel> Tags { get; set; }

        public string Title { get; set; }

        public PictureModel PictureModel { get; set; }

        public PictureModel PreviewPictureModel { get; set; }

        public string Intro { get; set; }

        public string Body { get; set; }

        public string SectionBg { get; set; }

        public bool HasBgImage { get; set; }

        public bool DisplayAdminLink { get; set; }

        public bool DisplayTagsInPreview { get; set; }

        public bool IsPublished { get; set; }

        public PreviewDisplayType PreviewDisplayType { get; set; }

        public AddBlogCommentModel AddNewComment { get; set; }
        public CommentListModel Comments { get; set; }
    }

    public class BlogPostValidator : AbstractValidator<BlogPostModel>
    {
        public BlogPostValidator()
        {
            RuleFor(x => x.AddNewComment.CommentText)
                .NotEmpty()
                .When(x => x.AddNewComment != null);
        }
    }
}