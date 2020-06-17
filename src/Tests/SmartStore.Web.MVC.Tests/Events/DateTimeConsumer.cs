using System;
using SmartStore.Core.Events;

namespace SmartStore.Web.MVC.Tests.Events
{
    public class DateTimeConsumer : IConsumer
    {
        public void HandleEvent(DateTime eventMessage)
        {
            DateTime = eventMessage;
        }

        // For testing
        public static DateTime DateTime { get; set; }
    }
}
