using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Xml;
using SmartStore.Admin.Models.Messages;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Filters;

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

					var fileTemplate = GetTemplateFromFile(template.Name);
					if (fileTemplate != null)
					{
						liquidTemplate.To = fileTemplate.To.NullEmpty() ?? liquidTemplate.To;
						liquidTemplate.ReplyTo = fileTemplate.ReplyTo.NullEmpty() ?? liquidTemplate.ReplyTo;
						liquidTemplate.Subject = fileTemplate.Subject.NullEmpty() ?? liquidTemplate.Subject;
						liquidTemplate.ModelTypes = fileTemplate.ModelTypes.NullEmpty() ?? liquidTemplate.ModelTypes;
						liquidTemplate.Body = fileTemplate.Body.NullEmpty() ?? liquidTemplate.Body;
					}

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
					//Customer = Services.WorkContext.CurrentCustomer,
					TestMode = true
				};

				var factory = Services.Resolve<IMessageFactory>();
				var result = factory.CreateMessage(context, false);
				var messageModel = result.Model;
				return Content(result.Email.Body, "text/html");
			}

			return RedirectToAction("Edit2", template.Id);
		}

		[HttpPost, FormValueRequired("save-in-file"), ActionName("Edit2")]
		public ActionResult SaveInFile2(MessageTemplateModel model)
		{
			var tpl = _messageTemplateService.GetMessageTemplateById(model.Id);
			tpl.To = model.To;
			tpl.ReplyTo = model.ReplyTo;
			tpl.Subject = model.Subject;
			tpl.ModelTypes = model.ModelTypes;
			tpl.Body = model.Body;
			Services.DbContext.SaveChanges();

			var doc = new XmlDocument();
			doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><MessageTemplate></MessageTemplate>");

			var root = doc.DocumentElement;
			root.AppendChild(doc.CreateElement("To")).InnerText = model.To;
			if (model.ReplyTo.HasValue())
				root.AppendChild(doc.CreateElement("ReplyTo")).InnerText = model.ReplyTo;
			root.AppendChild(doc.CreateElement("Subject")).InnerText = model.Subject;
			root.AppendChild(doc.CreateElement("ModelTypes")).InnerText = model.ModelTypes;
			root.AppendChild(doc.CreateElement("Body")).AppendChild(doc.CreateCDataSection(model.Body));

			string path = Path.Combine(CommonHelper.MapPath("~/App_Data/EmailTemplates/de"), model.Name.RemoveEncloser("", ".Liquid") + ".xml");
			var xml = Prettifier.PrettifyXML(doc.OuterXml);
			System.IO.File.WriteAllText(path, xml);

			return RedirectToAction("Edit2", model.Id);
		}

		private MessageTemplate GetTemplateFromFile(string messageTemplateName)
		{
			string path = Path.Combine(CommonHelper.MapPath("~/App_Data/EmailTemplates/de"), messageTemplateName + ".xml");
			if (!System.IO.File.Exists(path))
				return null;

			var doc = new XmlDocument();
			doc.Load(path);
			var root = doc.DocumentElement;

			var tpl = new MessageTemplate
			{
				To = root["To"]?.InnerText?.Trim(),
				ReplyTo = root["ReplyTo"]?.InnerText?.Trim(),
				Subject = root["Subject"]?.InnerText?.Trim(),
				ModelTypes = root["ModelTypes"]?.InnerText?.Trim(),
				Body = root["Body"]?.InnerText?.Trim()
			};

			return tpl;
		}

		//[FormValueRequired("send-test-mail"), ActionName("Edit2")]
		//public ActionResult SendTestMail()
		//{
		//	var svc = Services.Resolve<IWorkflowMessageService>();

		//	var order = GetRandomEntity<Order>(x => !x.Deleted) as Order;

		//	var id = svc.SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId);
		//	var qe = Services.Resolve<IQueuedEmailService>().GetQueuedEmailById(id);

		//	return Content(qe.Body, "text/html");
		//}

		//private object GetRandomEntity<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity, new()
		//{
		//	var dbSet = Services.DbContext.Set<T>().AsNoTracking();

		//	var query = dbSet.Where(predicate);

		//	// Determine how many entities match the given predicate
		//	var count = query.Count();

		//	object result;

		//	if (count > 0)
		//	{
		//		// Fetch a random one
		//		var skip = new Random().Next(count - 1);
		//		result = query.OrderBy(x => x.Id).Skip(skip).FirstOrDefault();
		//	}
		//	else
		//	{
		//		// No entity macthes the predicate. Provide a fallback test entity
		//		result = Activator.CreateInstance<T>();
		//	}

		//	return result;
		//}


	//	public ActionResult EditTest()
	//	{
	//		var path = @"D:\_temp\Emails\email.liquid";
	//		string body;
	//		if (!System.IO.File.Exists(path))
	//		{
	//			body = @"<a href='{{ Store.Url }}'>
 // <img src='{{ Store.Logo.Src }}' width='{{ Store.Logo.Width }}' height='{{ Store.Logo.Height }}' alt='{{ Store.Name }}'>
 // </a>
  
 // <div>
 //   Welcome {{ Customer.FullName }}, {{ Customer.Email }}
 // </div>
 // <div>
	//{{ Company.CompanyName }}
 //   {{ Company.Firstname }}
 //   {{ Company.Lastname }}
 //   {{ Company.Street }}
 // </div>";

	//			System.IO.File.WriteAllText(path, body);
	//		}
	//		else
	//		{
	//			body = System.IO.File.ReadAllText(path);
	//		}

	//		var messageTemplate = new MessageTemplate
	//		{
	//			Name = "MessageTemplate.Test",
	//			Subject = "Welcome to {{ Store.Name }}",
	//			Body = body
	//		};

	//		return View("Edit2", messageTemplate);
	//	}

		//[HttpPost, FormValueRequired("save", "save-continue")]
		//public ActionResult EditTest(MessageTemplate model)
		//{
		//	System.IO.File.WriteAllText(@"D:\_temp\Emails\email.liquid", model.Body);

		//	var factory = Services.Resolve<IMessageFactory>();

		//	var context = new MessageContext
		//	{
		//		MessageTemplate = model,
		//		Customer = Services.WorkContext.CurrentCustomer,
		//		TestMode = true
		//	};

		//	var result = factory.CreateMessage(context, false);
		//	var messageModel = result.Model;
		//	return Content(result.Email.Body, "text/html");
		//}
	}
}