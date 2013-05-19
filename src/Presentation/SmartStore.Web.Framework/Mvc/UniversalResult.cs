using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace SmartStore.Web.Framework.Mvc
{
	/// <summary>An action method result wrapper that handles all required action results in a project.</summary>
	/// <remarks>codehint: sm-add</remarks>
	public class UniversalResult : ActionResult
	{
		public UniversalResult() {
		}
		public UniversalResult(string content, string contentType = MediaTypeNames.Text.Html) {
			Content = content;
			ContentType = contentType;
		}
		public UniversalResult(XmlDocument doc, string contentType = MediaTypeNames.Text.Xml) {
			Content = doc;
			ContentType = contentType;
		}
		// TODO: more constructors here...

		public object Content { get; set; }
		public string ContentType { get; set; }

		public override void ExecuteResult(ControllerContext context) {
			if (Content != null) {
				string str = null;
				XmlDocument xmlDoc = null;

				context.HttpContext.Response.ContentType = ContentType;

				if ((str = Content as string) != null) {
					context.HttpContext.Response.Write(str);
				}
				else if ((xmlDoc = Content as XmlDocument) != null) {
					var xs = new XmlSerializer(xmlDoc.GetType());
					xs.Serialize(context.HttpContext.Response.Output, xmlDoc);
				}
				// TODO: more...
			}
		}
	}	// class
}
