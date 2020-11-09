using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Utilities;

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
        /// <param name="virtualRootPath">The virtual root path of templates to load, e.g. "~/Plugins/MyPlugins/EmailTemplates". Default is "~/App_Data/EmailTemplates".</param>
        /// <returns>Deserialized template xml</returns>
        public MessageTemplate Load(string templateName, Language language, string virtualRootPath = null)
        {
            Guard.NotEmpty(templateName, nameof(templateName));
            Guard.NotNull(language, nameof(language));

            var dir = ResolveTemplateDirectory(language, virtualRootPath);
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
        /// <param name="virtualRootPath">The virtual root path of templates to load, e.g. "~/Plugins/MyPlugins/EmailTemplates". Default is "~/App_Data/EmailTemplates".</param>
        /// <returns>List of deserialized template xml</returns>
        public IEnumerable<MessageTemplate> LoadAll(Language language, string virtualRootPath = null)
        {
            Guard.NotNull(language, nameof(language));

            var dir = ResolveTemplateDirectory(language, virtualRootPath);
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
        /// <param name="virtualRootPath">The virtual root path of templates to import, e.g. "~/Plugins/MyPlugins/EmailTemplates". Default is "~/App_Data/EmailTemplates".</param>
        public void ImportAll(Language language, string virtualRootPath = null)
        {
            var table = _ctx.Set<MessageTemplate>();

            var sourceTemplates = LoadAll(language, virtualRootPath);
            var dbTemplatesMap = table
                .ToList()
                .ToMultimap(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var source in sourceTemplates)
            {
                if (dbTemplatesMap.ContainsKey(source.Name))
                {
                    foreach (var target in dbTemplatesMap[source.Name])
                    {
                        if (source.To.HasValue()) target.To = source.To;
                        if (source.ReplyTo.HasValue()) target.ReplyTo = source.ReplyTo;
                        if (source.Subject.HasValue()) target.Subject = source.Subject;
                        if (source.ModelTypes.HasValue()) target.ModelTypes = source.ModelTypes;
                        if (source.Body.HasValue()) target.Body = source.Body;
                    }
                }
                else
                {
                    var template = new MessageTemplate
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

                    table.Add(template);
                }
            }

            _ctx.SaveChanges();
        }

        private DirectoryInfo ResolveTemplateDirectory(Language language, string virtualRootPath = null)
        {
            var rootPath = CommonHelper.MapPath(virtualRootPath.NullEmpty() ?? "~/App_Data/EmailTemplates/");
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
