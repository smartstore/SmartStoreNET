using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Security
{
    public class ValidateCaptchaAttribute : ActionFilterAttribute
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public Lazy<CaptchaSettings> CaptchaSettings { get; set; }
        public Lazy<ILocalizationService> LocalizationService { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var valid = false;
            var verify = true;

            try
            {
                verify = CaptchaSettings.Value.CanDisplayCaptcha && VerifyRecaptcha(filterContext);

                if (verify)
                {
                    var verifyUrl = CommonHelper.GetAppSetting<string>("g:RecaptchaVerifyUrl");
                    var recaptchaResponse = filterContext.HttpContext.Request.Form["g-recaptcha-response"];

                    var url = "{0}?secret={1}&response={2}".FormatInvariant(
                        verifyUrl,
                        HttpUtility.UrlEncode(CaptchaSettings.Value.ReCaptchaPrivateKey),
                        HttpUtility.UrlEncode(recaptchaResponse)
                    );

                    using (var client = new WebClient())
                    {
                        var jsonResponse = client.DownloadString(url);
                        using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonResponse)))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(GoogleRecaptchaApiResponse));
                            var result = serializer.ReadObject(memoryStream) as GoogleRecaptchaApiResponse;

                            if (result == null)
                            {
                                Logger.Error(LocalizationService.Value.GetResource("Common.CaptchaUnableToVerify"));
                            }
                            else
                            {
                                if (result.ErrorCodes == null)
                                {
                                    valid = result.Success;
                                }
                                else
                                {
                                    // Do not log 'missing input'. Could be a regular case.
                                    foreach (var error in result.ErrorCodes.Where(x => x.HasValue() && x != "missing-input-response"))
                                    {
                                        Logger.Error("Error while getting Google Recaptcha response: " + error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorsAll(ex);
            }

            // Push the result values as parameters in our action method.
            filterContext.ActionParameters["captchaValid"] = valid;

            filterContext.ActionParameters["captchaError"] = !valid && verify
                ? LocalizationService.Value.GetResource(CaptchaSettings.Value.UseInvisibleReCaptcha ? "Common.WrongInvisibleCaptcha" : "Common.WrongCaptcha")
                : null;

            base.OnActionExecuting(filterContext);
        }

        private bool VerifyRecaptcha(ActionExecutingContext context)
        {
            // Poor method to avoid unnecessary requests to the Google API.
            var controller = context?.ActionDescriptor?.ControllerDescriptor?.ControllerName.EmptyNull();
            var action = context?.ActionDescriptor?.ActionName.EmptyNull();

            if (controller == "Blog" && action == "BlogCommentAdd")
            {
                return CaptchaSettings.Value.ShowOnBlogCommentPage;
            }
            else if (controller == "Boards")
            {
                switch (action)
                {
                    case "TopicCreate":
                    case "TopicEdit":
                    case "PostCreate":
                    case "PostEdit":
                        return CaptchaSettings.Value.ShowOnForumPage;
                }
            }
            else if (controller == "Customer")
            {
                switch (action)
                {
                    case "Login":
                        return CaptchaSettings.Value.ShowOnLoginPage;
                    case "Register":
                        return CaptchaSettings.Value.ShowOnRegistrationPage;
                }
            }
            else if (controller == "Home" && action == "ContactUsSend")
            {
                return CaptchaSettings.Value.ShowOnContactUsPage;
            }
            else if (controller == "News" && action == "NewsCommentAdd")
            {
                return CaptchaSettings.Value.ShowOnNewsCommentPage;
            }
            else if (controller == "Product")
            {
                switch (action)
                {
                    case "ReviewsAdd":
                        return CaptchaSettings.Value.ShowOnProductReviewPage;
                    case "AskQuestionSend":
                        return CaptchaSettings.Value.ShowOnAskQuestionPage;
                    case "EmailAFriendSend":
                        return CaptchaSettings.Value.ShowOnEmailProductToFriendPage;
                }
            }
            else if (controller == "ShoppingCart" && action == "EmailWishlistSend")
            {
                return CaptchaSettings.Value.ShowOnEmailWishlistToFriendPage;
            }

            return true;
        }
    }


    [DataContract]
    public class GoogleRecaptchaApiResponse
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }

        [DataMember(Name = "error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}
