using System;
using NUnit.Framework;
using SmartStore.Web.Controllers;

namespace SmartStore.Web.MVC.Tests.Public.Infrastructure
{
    [TestFixture]
    public class RoutesTests : RoutesTestsBase
    {
        [Test]
        public void Default_route()
        {
            "~/".ShouldMapTo<HomeController>(c => c.Index());
        }

        [Test]
        public void Blog_routes()
        {
            //TODO why does it pass null instead of "new BlogPagingFilteringModel()" as it's done in real application? The same is about issue is in the other route test methods
            "~/blog/".ShouldMapTo<BlogController>(c => c.List(null));
            "~/blog/rss/".ShouldMapTo<BlogController>(c => c.ListRss());
            //"~/blog/2/".ShouldMapTo<BlogController>(c => c.BlogPost(2));
            //"~/blog/2/test-se-name".ShouldMapTo<BlogController>(c => c.BlogPost(2));
            //TODO validate properties such as 'Tag' or 'Month' in the passed BlogPagingFilteringModel. The same is about issue is in the other route test methods
            //"~/blog/tag/sometag".ShouldMapTo<BlogController>(c => c.List(new BlogPagingFilteringModel() { Tag = "sometag" }));
            //"~/blog/month/3".ShouldMapTo<BlogController>(c => c.List(new BlogPagingFilteringModel() { Month = "4" }));
        }

        [Test]
        public void Boards_routes()
        {
            "~/boards/".ShouldMapTo<BoardsController>(c => c.Index());
            //TODO add support for optional parameters in 'ShouldMapTo' method (such as in ~/boards/activediscussions/ or ~/boards/topic/11/). The same is about issue is in the other route test methods
            //"~/boards/activediscussions/".ShouldMapTo<BoardsController>(c => c.ActiveDiscussions(0));
            //"~/boards/activediscussionsrss/".ShouldMapTo<BoardsController>(c => c.ActiveDiscussionsRss(0));
            "~/boards/postedit/1".ShouldMapTo<BoardsController>(c => c.PostEdit(1));
            "~/boards/postdelete/2".ShouldMapTo<BoardsController>(c => c.PostDelete(2));
            "~/boards/postcreate/3".ShouldMapTo<BoardsController>(c => c.PostCreate(3, null));
            "~/boards/postcreate/4/5".ShouldMapTo<BoardsController>(c => c.PostCreate(4, 5));
            "~/boards/topicedit/6".ShouldMapTo<BoardsController>(c => c.TopicEdit(6));
            "~/boards/topicdelete/7".ShouldMapTo<BoardsController>(c => c.TopicDelete(7));
            "~/boards/topiccreate/8".ShouldMapTo<BoardsController>(c => c.TopicCreate(8));
            "~/boards/topicmove/9".ShouldMapTo<BoardsController>(c => c.TopicMove(9));
            "~/boards/topicwatch/10".ShouldMapTo<BoardsController>(c => c.TopicWatch(10));

            "~/boards/topic/11/".ShouldMapTo<BoardsController>(c => c.Topic(11, 1));
            "~/boards/topic/11/test-topic-slug".ShouldMapTo<BoardsController>(c => c.Topic(11, 1));
            "~/boards/topic/11/test-topic-slug/page/2".ShouldMapTo<BoardsController>(c => c.Topic(11, 2));
            "~/boards/forumwatch/12".ShouldMapTo<BoardsController>(c => c.ForumWatch(12));
            "~/boards/forumrss/13".ShouldMapTo<BoardsController>(c => c.ForumRss(13));
            "~/boards/forum/14/".ShouldMapTo<BoardsController>(c => c.Forum(14, 1));
            "~/boards/forum/14/test-forum-slug".ShouldMapTo<BoardsController>(c => c.Forum(14, 1));
            "~/boards/forum/14/test-forum-slug/page/2".ShouldMapTo<BoardsController>(c => c.Forum(14, 2));
            "~/boards/forumgroup/15/".ShouldMapTo<BoardsController>(c => c.ForumGroup(15));
            "~/boards/forumgroup/15/test-forumgroup-slug/".ShouldMapTo<BoardsController>(c => c.ForumGroup(15));
            //"~/boards/search/".ShouldMapTo<BoardsController>(c => c.Search(null, null, null, null, null, 1));
            "~/boards/search/".ShouldMapTo<BoardsController>(c => c.Search(null));
        }

        [Test]
        public void Catalog_routes()
        {
            //"~/p/some-se-name/".ShouldMapTo<CatalogController>(c => c.Product("some-se-name"));
            "~/recentlyviewedproducts/".ShouldMapTo<CatalogController>(c => c.RecentlyViewedProducts(null));
            "~/newproducts/".ShouldMapTo<CatalogController>(c => c.RecentlyAddedProducts(null));
            "~/newproducts/rss/".ShouldMapTo<CatalogController>(c => c.RecentlyAddedProductsRss(null));
            "~/catalog/addproducttocompare/2".ShouldMapTo<CatalogController>(c => c.AddProductToCompareList(2));
            "~/compareproducts/".ShouldMapTo<CatalogController>(c => c.CompareProducts());
            "~/catalog/RemoveProductFromCompare/3".ShouldMapTo<CatalogController>(c => c.RemoveProductFromCompareList(3));
            "~/catalog/clearcomparelist/".ShouldMapTo<CatalogController>(c => c.ClearCompareList());
            "~/product/emailafriend/4".ShouldMapTo<ProductController>(c => c.EmailAFriend(4));

            "~/manufacturer/all/".ShouldMapTo<CatalogController>(c => c.ManufacturerAll());

            "~/product/reviews/7/".ShouldMapTo<ProductController>(c => c.Reviews(7));
            "~/product/backinstocksubscribe/8/".ShouldMapTo<ProductController>(c => c.BackInStockSubscribePopup(8));

            "~/producttag/9/".ShouldMapTo<CatalogController>(c => c.ProductsByTag(9, null));
            "~/producttag/9/se-name/".ShouldMapTo<CatalogController>(c => c.ProductsByTag(9, null));
            "~/producttag/all/".ShouldMapTo<CatalogController>(c => c.ProductTagsAll());

            "~/search/".ShouldMapTo<SearchController>(c => c.Search(null));
        }

        [Test]
        public void Customer_routes()
        {
            "~/customer/DeleteBackInStockSubscription/1/".ShouldMapTo<CustomerController>(c => c.DeleteBackInStockSubscription(1));
            //"~/login/".ShouldMapTo<CustomerController>(c => c.Login(null, null, false));
            "~/login/".ShouldMapTo<CustomerController>(c => c.Login((bool?)null));
            "~/register/".ShouldMapTo<CustomerController>(c => c.Register());
            "~/logout/".ShouldMapTo<CustomerController>(c => c.Logout());
            "~/registerresult/2".ShouldMapTo<CustomerController>(c => c.RegisterResult(2));
            "~/customer/passwordrecovery/".ShouldMapTo<CustomerController>(c => c.PasswordRecovery());
            "~/customer/passwordrecoveryconfirm".ShouldMapTo<CustomerController>(c => c.PasswordRecoveryConfirm(null, null));

            "~/customer/info/".ShouldMapTo<CustomerController>(c => c.Info());
            "~/customer/addresses/".ShouldMapTo<CustomerController>(c => c.Addresses());
            "~/customer/orders/".ShouldMapTo<CustomerController>(c => c.Orders(null, null));
            "~/customer/returnrequests/".ShouldMapTo<CustomerController>(c => c.ReturnRequests());
            "~/customer/downloadableproducts/".ShouldMapTo<CustomerController>(c => c.DownloadableProducts());
            "~/customer/backinstocksubscriptions/".ShouldMapTo<CustomerController>(c => c.BackInStockSubscriptions(null));
            "~/customer/backinstocksubscriptions/?page=3".ShouldMapTo<CustomerController>(c => c.BackInStockSubscriptions(3));

            "~/customer/rewardpoints/".ShouldMapTo<CustomerController>(c => c.RewardPoints());
            "~/customer/changepassword/".ShouldMapTo<CustomerController>(c => c.ChangePassword());
            "~/customer/avatar/".ShouldMapTo<CustomerController>(c => c.Avatar());
            "~/customer/activation/?token=cc74c80f-1edd-43f7-85df-a3cccc1b47b9&email=test@test.com".ShouldMapTo<CustomerController>(c => c.AccountActivation("cc74c80f-1edd-43f7-85df-a3cccc1b47b9", "test@test.com"));
            "~/customer/forumsubscriptions".ShouldMapTo<CustomerController>(c => c.ForumSubscriptions(null));
            "~/customer/forumsubscriptions/?page=4".ShouldMapTo<CustomerController>(c => c.ForumSubscriptions(4));
            "~/customer/deleteforumsubscription/5".ShouldMapTo<CustomerController>(c => c.DeleteForumSubscription(5));
            "~/customer/addressdelete/6".ShouldMapTo<CustomerController>(c => c.AddressDelete(6));
            "~/customer/addressedit/7".ShouldMapTo<CustomerController>(c => c.AddressEdit(7));
            "~/customer/addressadd".ShouldMapTo<CustomerController>(c => c.AddressAdd());
        }

        [Test]
        public void Profile_routes()
        {
            "~/profile/1".ShouldMapTo<ProfileController>(c => c.Index(1, null));
            "~/profile/1/?page=2".ShouldMapTo<ProfileController>(c => c.Index(1, 2));
        }

        [Test]
        public void Cart_routes()
        {
            "~/cart/".ShouldMapTo<ShoppingCartController>(c => c.Cart(null));
            "~/wishlist".ShouldMapTo<ShoppingCartController>(c => c.Wishlist(null));
            "~/wishlist/aa74c80f-1edd-43f7-85df-a3cccc1b47b9".ShouldMapTo<ShoppingCartController>(c => c.Wishlist(new Guid("aa74c80f-1edd-43f7-85df-a3cccc1b47b9")));
            "~/shoppingcart/emailwishlist".ShouldMapTo<ShoppingCartController>(c => c.EmailWishlist());
        }

        [Test]
        public void Checkout_routes()
        {
            "~/checkout".ShouldMapTo<CheckoutController>(c => c.Index());
            "~/checkout/shippingaddress".ShouldMapTo<CheckoutController>(c => c.ShippingAddress());
            "~/checkout/billingaddress".ShouldMapTo<CheckoutController>(c => c.BillingAddress());
            "~/checkout/shippingmethod".ShouldMapTo<CheckoutController>(c => c.ShippingMethod());
            "~/checkout/paymentmethod".ShouldMapTo<CheckoutController>(c => c.PaymentMethod());
            "~/checkout/confirm".ShouldMapTo<CheckoutController>(c => c.Confirm());
            "~/checkout/completed".ShouldMapTo<CheckoutController>(c => c.Completed());
        }

        [Test]
        public void Order_routes()
        {
            "~/order/details/1".ShouldMapTo<OrderController>(c => c.Details(1));
            "~/order/reorder/3".ShouldMapTo<OrderController>(c => c.ReOrder(3));
            //"~/order/pdf/4".ShouldMapTo<OrderController>(c => c.GetPdfInvoice(4));
            "~/order/print/5".ShouldMapTo<OrderController>(c => c.Print(5, false));
        }

        [Test]
        public void ReturnRequest_routes()
        {
            "~/returnrequest/returnrequest/2".ShouldMapTo<ReturnRequestController>(c => c.ReturnRequest(2));
        }

        [Test]
        public void Common_routes()
        {
            "~/contactus".ShouldMapTo<HomeController>(c => c.ContactUs());
            "~/storeclosed".ShouldMapTo<HomeController>(c => c.StoreClosed());
        }

        [Test]
        public void Newsletter_routes()
        {
            //TODO cannot validate true parameter
            //"~/newsletter/subscriptionactivation/bb74c80f-1edd-43f7-85df-a3cccc1b47b9/true".ShouldMapTo<NewsletterController>(c => c.SubscriptionActivation(new Guid("bb74c80f-1edd-43f7-85df-a3cccc1b47b9"), true));
        }

        [Test]
        public void PrivateMessages_routes()
        {
            "~/privatemessages/".ShouldMapTo<PrivateMessagesController>(c => c.Index(null, null));
            "~/privatemessages/sent".ShouldMapTo<PrivateMessagesController>(c => c.Index(null, "sent"));
            "~/privatemessages/inbox".ShouldMapTo<PrivateMessagesController>(c => c.Index(null, "inbox"));
            "~/privatemessages/sent/?page=2".ShouldMapTo<PrivateMessagesController>(c => c.Index(2, "sent"));
            "~/privatemessages/send/3".ShouldMapTo<PrivateMessagesController>(c => c.Send(3, null));
            "~/privatemessages/send/4/?replyToMessageId=5".ShouldMapTo<PrivateMessagesController>(c => c.Send(4, 5));
            "~/privatemessages/view/6".ShouldMapTo<PrivateMessagesController>(c => c.View(6));
            "~/privatemessages/delete/7".ShouldMapTo<PrivateMessagesController>(c => c.Delete(7));
        }

        [Test]
        public void News_routes()
        {
            "~/news".ShouldMapTo<NewsController>(c => c.List(null));
            "~/news/rss/".ShouldMapTo<NewsController>(c => c.ListRss());
            //"~/news/2/".ShouldMapTo<NewsController>(c => c.NewsItem(2));
            //"~/news/2/se-name".ShouldMapTo<NewsController>(c => c.NewsItem(2));
        }
    }
}
