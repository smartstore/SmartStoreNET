using FluentValidation;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Blogs
{
    public partial class BlogPostValidator : AbstractValidator<BlogPostModel>
    {
        public BlogPostValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Fields.Title.Required"));

            RuleFor(x => x.Body)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Fields.Body.Required"));
        }
    }
}