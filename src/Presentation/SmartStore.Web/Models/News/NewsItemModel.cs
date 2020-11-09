using System;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.News
{
    [Validator(typeof(NewsItemValidator))]
    public partial class NewsItemModel : EntityModelBase
    {
        public NewsItemModel()
        {
            AddNewComment = new AddNewsCommentModel();
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

        public string Title { get; set; }
        public string Short { get; set; }
        public string Full { get; set; }

        public PictureModel PictureModel { get; set; }
        public PictureModel PreviewPictureModel { get; set; }

        public bool DisplayAdminLink { get; set; }

        public bool Published { get; set; }

        public AddNewsCommentModel AddNewComment { get; set; }
        public CommentListModel Comments { get; set; }
    }

    public class NewsItemValidator : AbstractValidator<NewsItemModel>
    {
        public NewsItemValidator()
        {
            RuleFor(x => x.AddNewComment.CommentTitle)
                .NotEmpty()
                .When(x => x.AddNewComment != null);

            RuleFor(x => x.AddNewComment.CommentTitle)
                .Length(1, 200)
                .When(x => x.AddNewComment != null && !string.IsNullOrEmpty(x.AddNewComment.CommentTitle));

            RuleFor(x => x.AddNewComment.CommentText)
                .NotEmpty()
                .When(x => x.AddNewComment != null);
        }
    }
}