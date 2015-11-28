using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Logging;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Controllers
{
    [SetWorkingCulture]
    [CustomerLastActivity]
    [StoreIpAddress]
    [StoreLastVisitedPage]
    [CheckAffiliate]
    [StoreClosedAttribute]
    [PublicStoreAllowNavigation]
    [LanguageSeoCodeAttribute]
    [RequireHttpsByConfigAttribute(SslRequirement.Retain)]
    public abstract partial class SmartAsyncController : AsyncController
    {

        protected override void Initialize(RequestContext requestContext)
        {   
            base.Initialize(requestContext);
        }
        
        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exc">Exception</param>
        private void LogException(Exception exc)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var logger = EngineContext.Current.Resolve<ILogger>();

            var customer = workContext.CurrentCustomer;
            logger.Error(exc.Message, exc, customer);
        }
        /// <summary>
        /// Display success notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void ErrorNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Error, message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true, bool logException = true)
        {
            if (logException)
                LogException(exception);
            AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display notification
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            string dataKey = string.Format("sm.notifications.{0}", type);
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }

    }
}
