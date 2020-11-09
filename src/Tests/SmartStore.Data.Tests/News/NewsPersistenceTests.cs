using System;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.News;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.News
{
    [TestFixture]
    public class NewsPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_newsItem()
        {
            var news = new NewsItem
            {
                Title = "Title 1",
                Short = "Short 1",
                Full = "Full 1",
                Published = true,
                StartDateUtc = new DateTime(2010, 01, 01),
                EndDateUtc = new DateTime(2010, 01, 02),
                AllowComments = true,
                ApprovedCommentCount = 1,
                NotApprovedCommentCount = 2,
                LimitedToStores = true,
                CreatedOnUtc = new DateTime(2010, 01, 03),
                MetaTitle = "MetaTitle 1",
                MetaDescription = "MetaDescription 1",
                MetaKeywords = "MetaKeywords 1"
            };

            var fromDb = SaveAndLoadEntity(news);
            fromDb.ShouldNotBeNull();
            fromDb.Title.ShouldEqual("Title 1");
            fromDb.Short.ShouldEqual("Short 1");
            fromDb.Full.ShouldEqual("Full 1");
            fromDb.Published.ShouldEqual(true);
            fromDb.StartDateUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.EndDateUtc.ShouldEqual(new DateTime(2010, 01, 02));
            fromDb.AllowComments.ShouldEqual(true);
            fromDb.ApprovedCommentCount.ShouldEqual(1);
            fromDb.NotApprovedCommentCount.ShouldEqual(2);
            fromDb.LimitedToStores.ShouldEqual(true);
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 03));
            fromDb.MetaTitle.ShouldEqual("MetaTitle 1");
            fromDb.MetaDescription.ShouldEqual("MetaDescription 1");
            fromDb.MetaKeywords.ShouldEqual("MetaKeywords 1");
        }

        [Test]
        public void Can_save_and_load_newsItem_with_comments()
        {
            var news = new NewsItem
            {
                Title = "Title 1",
                Short = "Short 1",
                Full = "Full 1",
                AllowComments = true,
                Published = true,
                CreatedOnUtc = new DateTime(2010, 01, 01)
            };

            news.NewsComments.Add(new NewsComment
            {
                CommentText = "Comment text 1",
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 03),
                UpdatedOnUtc = new DateTime(2010, 01, 04),
                Customer = GetTestCustomer()
            });

            var fromDb = SaveAndLoadEntity(news);
            fromDb.ShouldNotBeNull();

            fromDb.NewsComments.ShouldNotBeNull();
            (fromDb.NewsComments.Count == 1).ShouldBeTrue();
            fromDb.NewsComments.First().CommentText.ShouldEqual("Comment text 1");
        }

        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }
    }
}
