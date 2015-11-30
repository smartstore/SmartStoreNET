using System;
using System.Linq;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Blogs
{
    [TestFixture]
    public class BlogPostPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_blogPost()
        {
            var blogPost = new BlogPost
            {
                Title = "Title 1",
                Body = "Body 1",
                AllowComments = true,
                ApprovedCommentCount = 1,
                NotApprovedCommentCount = 2,
                Tags = "Tags 1",
                StartDateUtc = new DateTime(2010, 01, 01),
                EndDateUtc = new DateTime(2010, 01, 02),
                CreatedOnUtc = new DateTime(2010, 01, 03),
                MetaTitle = "MetaTitle 1",
                MetaDescription = "MetaDescription 1",
                MetaKeywords = "MetaKeywords 1",
				LimitedToStores = true,
                Language = new Language()
                {
                    Name = "English",
                    LanguageCulture = "en-Us",
                }
            };

            var fromDb = SaveAndLoadEntity(blogPost);
            fromDb.ShouldNotBeNull();
            fromDb.Title.ShouldEqual("Title 1");
            fromDb.Body.ShouldEqual("Body 1");
            fromDb.AllowComments.ShouldEqual(true);
            fromDb.ApprovedCommentCount.ShouldEqual(1);
            fromDb.NotApprovedCommentCount.ShouldEqual(2);
            fromDb.Tags.ShouldEqual("Tags 1");
            fromDb.StartDateUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.EndDateUtc.ShouldEqual(new DateTime(2010, 01, 02));
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 03));
            fromDb.MetaTitle.ShouldEqual("MetaTitle 1");
            fromDb.MetaDescription.ShouldEqual("MetaDescription 1");
            fromDb.MetaKeywords.ShouldEqual("MetaKeywords 1");
			fromDb.LimitedToStores.ShouldEqual(true);

            fromDb.Language.ShouldNotBeNull();
            fromDb.Language.Name.ShouldEqual("English");
        }

        [Test]
        public void Can_save_and_load_blogPost_with_blogComments()
        {
            var blogPost = new BlogPost
            {
                Title = "Title 1",
                Body = "Body 1",
                AllowComments = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                Language = new Language()
                {
                    Name = "English",
                    LanguageCulture = "en-Us",
                }
            };
            blogPost.BlogComments.Add
                (
                    new BlogComment
                    {
                        IpAddress = "192.168.1.1",
                        IsApproved = true,
                        CreatedOnUtc = new DateTime(2010, 01, 03),
                        UpdatedOnUtc = new DateTime(2010, 01, 04),
                        Customer = GetTestCustomer()
                    }
                );
            var fromDb = SaveAndLoadEntity(blogPost);
            fromDb.ShouldNotBeNull();


            fromDb.BlogComments.ShouldNotBeNull();
            (fromDb.BlogComments.Count == 1).ShouldBeTrue();
            fromDb.BlogComments.First().IpAddress.ShouldEqual("192.168.1.1");
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
