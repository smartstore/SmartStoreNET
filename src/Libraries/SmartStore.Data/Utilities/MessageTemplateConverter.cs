using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace SmartStore.Data.Utilities
{
	public sealed class MessageTemplateConverter
	{
		private readonly SmartObjectContext _ctx;
		private readonly EmailAccount _defaultEmailAccount;

		public MessageTemplateConverter(IDbContext context)
		{
			_ctx = context as SmartObjectContext;
			if (_ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			_defaultEmailAccount = _ctx.Set<EmailAccount>().FirstOrDefault(x => x.Email != null);
		}

		/// <summary>
		/// Loads a single message template from file (~/App_Data/EmailTemplates/)
		/// and deserializes its xml content.
		/// </summary>
		/// <param name="templateName">Name of template without extension, e.g. 'GiftCard.Notification'</param>
		/// <param name="language">Language</param>
		/// <returns>Deserialized template xml</returns>
		public MessageTemplate Load(string templateName, Language language)
		{
			Guard.NotEmpty(templateName, nameof(templateName));
			Guard.NotNull(language, nameof(language));

			var dir = ResolveTemplateDirectory(language);
			var fullPath = Path.Combine(dir.FullName, templateName + ".xml");

			if (!File.Exists(fullPath))
			{
				throw new FileNotFoundException($"File '{fullPath}' does not exist.");
			}

			return DeserializeTemplate(fullPath);
		}

		/// <summary>
		/// Loads all message templates from disk (~/App_Data/EmailTemplates/)
		/// </summary>
		/// <param name="language">Language</param>
		/// <returns>List of deserialized template xml</returns>
		public IEnumerable<MessageTemplate> LoadAll(Language language)
		{
			Guard.NotNull(language, nameof(language));

			var dir = ResolveTemplateDirectory(language);
			var files = dir.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly);

			foreach (var file in files)
			{
				var template = DeserializeTemplate(file.FullName);
				template.Name = Path.GetFileNameWithoutExtension(file.Name);
				yield return template;
			}
		}

		public MessageTemplate Deserialize(string xml, string templateName)
		{
			Guard.NotEmpty(xml, nameof(xml));
			Guard.NotEmpty(templateName, nameof(templateName));

			var template = DeserializeDocument(XDocument.Parse(xml));
			template.Name = templateName;
			return template;
		}

		public XmlDocument Save(MessageTemplate template, Language language)
		{
			Guard.NotNull(template, nameof(template));
			Guard.NotNull(language, nameof(language));

			var doc = new XmlDocument();
			doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><MessageTemplate></MessageTemplate>");

			var root = doc.DocumentElement;
			root.AppendChild(doc.CreateElement("To")).InnerText = template.To;
			if (template.ReplyTo.HasValue())
				root.AppendChild(doc.CreateElement("ReplyTo")).InnerText = template.ReplyTo;
			root.AppendChild(doc.CreateElement("Subject")).InnerText = template.Subject;
			root.AppendChild(doc.CreateElement("ModelTypes")).InnerText = template.ModelTypes;
			root.AppendChild(doc.CreateElement("Body")).AppendChild(doc.CreateCDataSection(template.Body));

			var path = Path.Combine(CommonHelper.MapPath("~/App_Data/EmailTemplates"), language.GetTwoLetterISOLanguageName());
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			// File path
			path = Path.Combine(path, template.Name + ".xml");

			var xml = Prettifier.PrettifyXML(doc.OuterXml);
			File.WriteAllText(path, xml);

			return doc;
		}

		/// <summary>
		/// Imports all template xml files to MessageTemplate table
		/// </summary>
		public void ImportAll(Language language)
		{
			var table = _ctx.Set<MessageTemplate>();

			var sourceTemplates = LoadAll(language);
			var dbTemplatesMap = table
				.ToList()
				.ToDictionarySafe(x => x.Name, StringComparer.OrdinalIgnoreCase);

			foreach (var source in sourceTemplates)
			{
				if (dbTemplatesMap.TryGetValue(source.Name, out var target))
				{
					if (source.To.HasValue()) target.To = source.To;
					if (source.ReplyTo.HasValue()) target.ReplyTo = source.ReplyTo;
					if (source.Subject.HasValue()) target.Subject = source.Subject;
					if (source.ModelTypes.HasValue()) target.ModelTypes = source.ModelTypes;
					if (source.Body.HasValue()) target.Body = source.Body;
				}
				else
				{
					target = new MessageTemplate
					{
						Name = source.Name,
						To = source.To,
						ReplyTo = source.ReplyTo,
						Subject = source.Subject,
						ModelTypes = source.ModelTypes,
						Body = source.Body,
						IsActive = true,
						EmailAccountId = (_defaultEmailAccount?.Id).GetValueOrDefault(),
					};

					table.Add(target);
				}
			}

			_ctx.SaveChanges();
		}

		private DirectoryInfo ResolveTemplateDirectory(Language language)
		{
			var rootPath = CommonHelper.MapPath("~/App_Data/EmailTemplates/");
			var testPaths = new[] 
			{
				language.LanguageCulture,
				language.GetTwoLetterISOLanguageName(),
				"en"
			};

			foreach (var path in testPaths.Select(x => Path.Combine(rootPath, x)))
			{
				if (Directory.Exists(path))
				{
					return new DirectoryInfo(path);
				}
			}
			
			throw new DirectoryNotFoundException($"Could not obtain an email templates path for language {language.LanguageCulture}. Fallback to 'en' failed, because directory does not exist.");
		}

		private MessageTemplate DeserializeTemplate(string fullPath)
		{
			return DeserializeDocument(XDocument.Load(fullPath));
		}

		private MessageTemplate DeserializeDocument(XDocument doc)
		{
			var root = doc.Root;
			var result = new MessageTemplate();
			
			foreach (var node in root.Nodes().OfType<XElement>())
			{
				var value = node.Value.Trim();

				switch (node.Name.LocalName)
				{
					case "To":
						result.To = value;
						break;
					case "ReplyTo":
						result.ReplyTo = value;
						break;
					case "Subject":
						result.Subject = value;
						break;
					case "ModelTypes":
						result.ModelTypes = value;
						break;
					case "Body":
						result.Body = value;
						break;
				}
			}

			return result;
		}
	}
}
