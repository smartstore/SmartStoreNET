using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Data.Utilities
{
    public sealed class NewsItemConverter
    {
        private readonly SmartObjectContext _ctx;

        public NewsItemConverter(IDbContext context)
        {
            _ctx = context as SmartObjectContext;
            if (_ctx == null)
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
        }

        /// <summary>
        /// Loads all news item from disk (~/App_Data/Samples/news/)
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="virtualRootPath">The virtual root path of news to load, e.g. "~/Plugins/MyPlugins/NewsItem". Default is "~/App_Data/Samples/news".</param>
        /// <returns>List of deserialized news items xml</returns>
        public IEnumerable<NewsItem> LoadAll(Language language, string virtualRootPath = null)
        {
            Guard.NotNull(language, nameof(language));

            var dir = ResolveNewsDirectory(language, virtualRootPath);
            var files = dir.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                yield return DeserializeNews(file.FullName);
            }
        }

        /// <summary>
        /// Imports all news xml files to NewsItem table
        /// </summary>
        /// <param name="virtualRootPath">The virtual root path of blogs to import, e.g. "~/Plugins/MyPlugins/NewsItem". Default is "~/App_Data/Samples/news".</param>
        /// <returns>List of new imported news items</returns>
        public IList<NewsItem> ImportAll(Language language, string virtualRootPath = null)
        {
            var newsImported = new List<NewsItem>();
            var table = _ctx.Set<NewsItem>();
            var sourceBlogs = LoadAll(language);
            var dbNewsMap = table.ToList().ToMultimap(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var source in sourceBlogs)
            {
                if (dbNewsMap.ContainsKey(source.Title))
                {
                    foreach (var target in dbNewsMap[source.Title])
                    {
                        if (source.Title.HasValue()) target.Title = source.Title;
                        if (source.MetaTitle.HasValue()) target.MetaTitle = source.MetaTitle;
                        if (source.MetaDescription.HasValue()) target.MetaDescription = source.MetaDescription;
                        if (source.Short.HasValue()) target.Short = source.Short;
                        if (source.Full.HasValue()) target.Full = source.Full;
                        if (source.CreatedOnUtc != null) target.CreatedOnUtc = source.CreatedOnUtc;
                        if (source.MediaFile != null) target.MediaFile = source.MediaFile;
                        if (source.PreviewMediaFile != null) target.PreviewMediaFile = source.PreviewMediaFile;
                        target.AllowComments = source.AllowComments;
                    }
                }
                else
                {
                    var news = new NewsItem
                    {
                        Title = source.Title,
                        MetaTitle = source.MetaTitle,
                        MetaDescription = source.MetaDescription,
                        Short = source.Short,
                        Full = source.Full,                                                
                        CreatedOnUtc = source.CreatedOnUtc,                        
                        MediaFile = source.MediaFile,
                        PreviewMediaFile = source.PreviewMediaFile,
                        AllowComments = source.AllowComments,
                        Published = true
                    };

                    newsImported.Add(news);
                    table.Add(news);
                }
            }

            _ctx.SaveChanges();
            return newsImported;
        }

        private DirectoryInfo ResolveNewsDirectory(Language language, string virtualRootPath = null)
        {
            var testPaths = new[]
            {
                language.LanguageCulture,
                language.GetTwoLetterISOLanguageName(),
                "en"
            };

            var rootPath = CommonHelper.MapPath(virtualRootPath.NullEmpty() ?? "~/App_Data/Samples/news/");
            foreach (var path in testPaths.Select(x => Path.Combine(rootPath, x)))
            {
                if (Directory.Exists(path))
                {
                    return new DirectoryInfo(path);
                }
            }

            throw new DirectoryNotFoundException($"Could not obtain a news item path for language {language.LanguageCulture}. Fallback to 'en' failed, because directory does not exist.");
        }

        private NewsItem DeserializeNews(string fullPath)
        {
            return DeserializeDocument(XDocument.Load(fullPath));
        }

        private NewsItem DeserializeDocument(XDocument doc)
        {
            var result = new NewsItem();
            var nodes = doc.Root.Nodes().OfType<XElement>();

            foreach (var node in nodes)
            {
                var value = node.Value.Trim();

                switch (node.Name.LocalName)
                {
                    case "Title":
                        result.Title = value;
                        break;
                    case "MetaTitle":
                        result.MetaTitle = value;
                        break;
                    case "MetaDescription":
                        result.MetaDescription = value;
                        break;
                    case "Short":
                        result.Short = value;
                        break;
                    case "Full":
                        result.Full = value;
                        break;
                    case "CreatedOn":
                        result.CreatedOnUtc = value.ToDateTime(new DateTime()).Value;
                        break;
                    case "Image":
                        var seName = GetSeName(Path.GetFileNameWithoutExtension(value));
                        result.MediaFile = CreateImage(value, seName);
                        break;
                    case "ImagePreview":
                        seName = GetSeName(Path.GetFileNameWithoutExtension(value));
                        var previewImage = CreateImage(value, seName);
                        result.PreviewMediaFile = previewImage.Name == result.MediaFile.Name
                            ? result.MediaFile
                            : previewImage;
                        break;
                    case "Comments":
                        result.AllowComments = value.ToBool();
                        break;
                }
            }

            return result;
        }

        private MediaFile CreateImage(string fileName, string seoFilename = null)
        {
            try
            {
                var ext = Path.GetExtension(fileName);
                var mimeType = MimeTypes.MapNameToMimeType(ext);
                var path = Path.Combine(CommonHelper.MapPath("~/App_Data/Samples/news/"), fileName).Replace('/', '\\');
                var buffer = File.ReadAllBytes(path);
                var now = DateTime.UtcNow;

                var name = seoFilename.HasValue()
                    ? seoFilename.Truncate(100) + ext
                    : Path.GetFileName(fileName).ToLower().Replace('_', '-');

                var file = new MediaFile
                {
                    Name = name,
                    MediaType = "image",
                    MimeType = mimeType,
                    Extension = ext.EmptyNull().TrimStart('.'),
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    Size = buffer.Length,
                    MediaStorage = new MediaStorage { Data = buffer },
                    Version = 1 // so that FolderId is set later during track detection
                };

                return file;
            }
            catch (Exception ex)
            {
                // Throw ex;
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        private string GetSeName(string name)
        {
            return SeoHelper.GetSeName(name, true, false);
        }
    }
}