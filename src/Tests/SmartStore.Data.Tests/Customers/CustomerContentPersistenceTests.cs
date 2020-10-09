using System;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Data.Tests.Blogs;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Customers
{
    [TestFixture]
    public class CustomerContentPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_customerContent()
        {
            var customerContent = new CustomerContent
            {
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02),
                Customer = GetTestCustomer()
            };

            var fromDb = SaveAndLoadEntity(customerContent);
            fromDb.ShouldNotBeNull();
            fromDb.IpAddress.ShouldEqual("192.168.1.1");
            fromDb.IsApproved.ShouldEqual(true);
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.UpdatedOnUtc.ShouldEqual(new DateTime(2010, 01, 02));

            fromDb.Customer.ShouldNotBeNull();
        }

        [Test]
        public void Can_get_customer_content_by_type()
        {
            var customer = SaveAndLoadEntity<Customer>(GetTestCustomer(), false);
            var product = SaveAndLoadEntity<Product>(GetTestProduct(), false);

            var productReview = new ProductReview
            {
                Customer = customer,
                Product = product,
                Title = "Test",
                ReviewText = "A review",
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02),
            };

            var productReviewHelpfulness = new ProductReviewHelpfulness
            {
                Customer = customer,
                ProductReview = productReview,
                WasHelpful = true,
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 03),
                UpdatedOnUtc = new DateTime(2010, 01, 04)
            };

            var blogComment = new BlogComment
            {
                Customer = customer,
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 03),
                UpdatedOnUtc = new DateTime(2010, 01, 04),
                BlogPost = BlogPostPersistenceTests.GetTestBlogPost()
            };

            var table = context.Set<CustomerContent>();
            table.RemoveRange(table.ToList());

            table.Add(productReview);
            table.Add(productReviewHelpfulness);
            table.Add(blogComment);

            context.SaveChanges();

            context.Dispose();
            context = new SmartObjectContext(GetTestDbName());

            var query = context.Set<CustomerContent>();
            query.ToList().Count.ShouldEqual(3);

            var dbReviews = query.OfType<ProductReview>().ToList();
            dbReviews.Count().ShouldEqual(1);
            dbReviews.First().ReviewText.ShouldEqual("A review");

            var dbHelpfulnessRecords = query.OfType<ProductReviewHelpfulness>().ToList();
            dbHelpfulnessRecords.Count().ShouldEqual(1);
            dbHelpfulnessRecords.First().WasHelpful.ShouldEqual(true);

            var dbBlogCommentRecords = query.OfType<BlogComment>().ToList();
            dbBlogCommentRecords.Count().ShouldEqual(1);
        }

        [Test]
        public void Can_save_productReview_with_helpfulness()
        {
            var customer = SaveAndLoadEntity<Customer>(GetTestCustomer(), false);
            var product = SaveAndLoadEntity<Product>(GetTestProduct(), false);

            var productReview = new ProductReview
            {
                Customer = customer,
                Product = product,
                Title = "Test",
                ReviewText = "A review",
                IpAddress = "192.168.1.1",
                IsApproved = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02)
            };
            productReview.ProductReviewHelpfulnessEntries.Add
                (
                    new ProductReviewHelpfulness
                    {
                        Customer = customer,
                        WasHelpful = true,
                        IpAddress = "192.168.1.1",
                        IsApproved = true,
                        CreatedOnUtc = new DateTime(2010, 01, 03),
                        UpdatedOnUtc = new DateTime(2010, 01, 04)
                    }
                );
            var fromDb = SaveAndLoadEntity(productReview);
            fromDb.ShouldNotBeNull();
            fromDb.ReviewText.ShouldEqual("A review");


            fromDb.ProductReviewHelpfulnessEntries.ShouldNotBeNull();
            (fromDb.ProductReviewHelpfulnessEntries.Count == 1).ShouldBeTrue();
            fromDb.ProductReviewHelpfulnessEntries.First().WasHelpful.ShouldEqual(true);
        }

        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "some comment here",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }

        protected Product GetTestProduct()
        {
            return new Product
            {
                Name = "Name 1",
                Published = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02),
            };
        }
    }
}