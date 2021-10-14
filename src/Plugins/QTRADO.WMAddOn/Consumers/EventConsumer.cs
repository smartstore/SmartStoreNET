using System;
using System.Web.Mvc.Html;

using QTRADO.WMAddOn.Models;
using QTRADO.WMAddOn.Settings;

using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace QTRADO.WMAddOn
{
    public class EventConsumer : IConsumer
    {
        private readonly ICommonServices _services;
        private readonly IEventPublisher _eventPublisher;

        public EventConsumer(ICommonServices services,
            IEventPublisher eventPublisher)
        {
            _services = services;
            _eventPublisher = eventPublisher;
        }

        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            // Do something when an order is placed

            // Don't let the next line mislead you! This was only added for explanation purposes not to be actually implemented in this event.
            // If you want to know which other events there are, just make a right click on .Publish( and search for references in this solution.
            //_eventPublisher.Publish(new OrderPaidEvent(null));
        }
    }
}