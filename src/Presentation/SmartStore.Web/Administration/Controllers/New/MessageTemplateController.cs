using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Collections;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
	public partial class MessageTemplateController
	{
		public ActionResult Edit2()
		{
			var path = @"D:\_temp\Emails\email.liquid";
			string body;
			if (!System.IO.File.Exists(path))
			{
				body = @"<a href='{{ Store.Url }}'>
  <img src='{{ Store.Logo.Src }}' width='{{ Store.Logo.Width }}' height='{{ Store.Logo.Height }}' alt='{{ Store.Name }}'>
  </a>
  
  <div>
    Welcome {{ Customer.FullName }}, {{ Customer.Email }}
  </div>
  <div>
	{{ Company.CompanyName }}
    {{ Company.Firstname }}
    {{ Company.Lastname }}
    {{ Company.Street }}
  </div>";

				System.IO.File.WriteAllText(path, body);
			}
			else
			{
				body = System.IO.File.ReadAllText(path);
			}

			var messageTemplate = new MessageTemplate
			{
				Name = "MessageTemplate.Test",
				Subject = "Welcome to {{ Store.Name }}",
				Body = body
			};

			return View(messageTemplate);
		}

		[HttpPost, FormValueRequired("save", "save-continue")]
		public ActionResult Edit2(MessageTemplate model)
		{
			System.IO.File.WriteAllText(@"D:\_temp\Emails\email.liquid", model.Body);

			var factory = Services.Resolve<IMessageFactory>();

			var context = new MessageContext
			{
				MessageTemplate = model,
				Customer = Services.WorkContext.CurrentCustomer,
				TestMode = true
			};

			var result = factory.CreateMessage(context, false);
			var messageModel = result.Model;
			return Content(result.Email.Body, "text/html");
		}
	}
}