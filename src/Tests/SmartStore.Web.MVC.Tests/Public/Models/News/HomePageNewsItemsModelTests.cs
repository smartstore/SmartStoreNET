using System;
using NUnit.Framework;
using SmartStore.Tests;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.News;

namespace SmartStore.Web.MVC.Tests.Public.Models.News
{
    [TestFixture]
    public class HomePageNewsItemsModelTests
    {
        [Test]
        public void Can_clone()
        {
            var model1 = new HomePageNewsItemsModel();

            var newsItemModel1 = new NewsItemModel
            {
                Id = 1,
                SeName = "SeName 1",
                Title = "Title 1",
                Short = "Short 1",
                Full = "Full 1",
                CreatedOn = new DateTime(2010, 01, 01),
                AddNewComment = new AddNewsCommentModel
                {
                    CommentTitle = "CommentTitle 1",
                    CommentText = "CommentText 1",
                    DisplayCaptcha = true
                }
            };

            newsItemModel1.Comments.AllowComments = true;
            newsItemModel1.Comments.NumberOfComments = 2;

            newsItemModel1.Comments.Comments.Add(new CommentModel(newsItemModel1.Comments)
            {
                Id = 3,
                CustomerId = 4,
                CustomerName = "CustomerName 1",
                CommentTitle = "CommentTitle 1",
                CommentText = "CommentText 1",
                CreatedOn = new DateTime(2010, 01, 02),
                AllowViewingProfiles = true
            });
            model1.NewsItems.Add(newsItemModel1);

            var model2 = (HomePageNewsItemsModel)model1.Clone();
            model2.NewsItems.ShouldNotBeNull();
            model2.NewsItems.Count.ShouldEqual(1);

            var newsItemModel2 = model2.NewsItems[0];
            newsItemModel2.Id.ShouldEqual(1);
            newsItemModel2.SeName.ShouldEqual("SeName 1");
            newsItemModel2.Title.ShouldEqual("Title 1");
            newsItemModel2.Short.ShouldEqual("Short 1");
            newsItemModel2.Full.ShouldEqual("Full 1");
            newsItemModel2.Comments.AllowComments.ShouldEqual(true);
            newsItemModel2.Comments.NumberOfComments.ShouldEqual(2);
            newsItemModel2.CreatedOn.ShouldEqual(new DateTime(2010, 01, 01));
            newsItemModel2.Comments.Comments.ShouldNotBeNull();
            newsItemModel2.Comments.Comments.Count.ShouldEqual(1);
            newsItemModel2.Comments.Comments[0].Id.ShouldEqual(3);
            newsItemModel2.Comments.Comments[0].CustomerId.ShouldEqual(4);
            newsItemModel2.Comments.Comments[0].CustomerName.ShouldEqual("CustomerName 1");
            newsItemModel2.Comments.Comments[0].CommentTitle.ShouldEqual("CommentTitle 1");
            newsItemModel2.Comments.Comments[0].CommentText.ShouldEqual("CommentText 1");
            newsItemModel2.Comments.Comments[0].CreatedOn.ShouldEqual(new DateTime(2010, 01, 02));
            newsItemModel2.Comments.Comments[0].AllowViewingProfiles.ShouldEqual(true);
            newsItemModel2.AddNewComment.ShouldNotBeNull();
            newsItemModel2.AddNewComment.CommentTitle.ShouldEqual("CommentTitle 1");
            newsItemModel2.AddNewComment.CommentText.ShouldEqual("CommentText 1");
            newsItemModel2.AddNewComment.DisplayCaptcha.ShouldEqual(true);
        }
    }
}
