using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Collections;
using SmartStore.ComponentModel;
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
		public ActionResult Edit2(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
				return AccessDeniedView();

			var template = _messageTemplateService.GetMessageTemplateById(id);
			if (template == null)
				return RedirectToAction("List");

			if (!template.Name.EndsWith(".Liquid"))
			{
				var liquidTemplate = _messageTemplateService.GetMessageTemplateByName(template.Name + ".Liquid", Services.StoreContext.CurrentStore.Id);

				if (liquidTemplate == null)
				{
					liquidTemplate = new MessageTemplate();
					MiniMapper.Map(template, liquidTemplate);
					liquidTemplate.Id = 0;
					liquidTemplate.To = "{{ Customer.FullName }} <{{ Customer.Email }}>";
					liquidTemplate.Name += ".Liquid";

					_messageTemplateService.InsertMessageTemplate(liquidTemplate);
				}

				template = liquidTemplate;
			}

			var model = template.ToModel();

			if (template.LastModelTree.HasValue())
			{
				ViewBag.LastModelTree = Newtonsoft.Json.JsonConvert.DeserializeObject<TreeNode<ModelTreeMember>>(template.LastModelTree);
			}		

			return View(model);
		}

		[HttpPost, FormValueRequired("save", "save-continue")]
		public ActionResult Edit2(MessageTemplateModel model)
		{
			var template = _messageTemplateService.GetMessageTemplateById(model.Id);
			if (template == null)
				return RedirectToAction("List");

			if (ModelState.IsValid && template.Name.EndsWith(".Liquid"))
			{
				template.To = model.To;
				template.ReplyTo = model.ReplyTo;
				template.Body = model.Body;
				template.Subject = model.Subject;

				_messageTemplateService.UpdateMessageTemplate(template);

				var context = new MessageContext
				{
					MessageTemplate = template,
					Customer = Services.WorkContext.CurrentCustomer,
					TestMode = true
				};

				var factory = Services.Resolve<IMessageFactory>();
				var result = factory.CreateMessage(context, false);
				var messageModel = result.Model;
				return Content(result.Email.Body, "text/html");
			}

			return RedirectToAction("Edit2", template.Id);
		}




		public ActionResult EditTest()
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

			return View("Edit2", messageTemplate);
		}

		[HttpPost, FormValueRequired("save", "save-continue")]
		public ActionResult EditTest(MessageTemplate model)
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