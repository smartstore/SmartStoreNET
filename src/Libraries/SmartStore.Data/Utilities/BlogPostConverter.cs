using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Data.Utilities
{
    public sealed class BlogPostConverter
    {
        private readonly SmartObjectContext _ctx;

        public BlogPostConverter(IDbContext context)
        {
            _ctx = context as SmartObjectContext;
            if (_ctx == null)
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
        }

        /// <summary>
        /// Loads all blog post from disk (~/App_Data/Samples/blog/)
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="virtualRootPath">The virtual root path of blogs to load, e.g. "~/Plugins/MyPlugins/BlogPosts". Default is "~/App_Data/Samples/blog".</param>
        /// <returns>List of deserialized blog posts xml</returns>
        public IEnumerable<BlogPost> LoadAll(Language language, string virtualRootPath = null)
        {
            Guard.NotNull(language, nameof(language));

            var dir = ResolveBlogDirectory(language, virtualRootPath);
            var files = dir.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                yield return DeserializeBlog(file.FullName);
            }
        }

        /// <summary>
        /// Imports all blog xml files to BlogPost table
        /// </summary>
        /// <param name="virtualRootPath">The virtual root path of blogs to import, e.g. "~/Plugins/MyPlugins/BlogPost". Default is "~/App_Data/Samples/blog".</param>
        /// <returns>List of new imported blog posts</returns>
        public IList<BlogPost> ImportAll(Language language, string virtualRootPath = null)
        {
            var blogsImported = new List<BlogPost>();
            var table = _ctx.Set<BlogPost>();
            var sourceBlogs = LoadAll(language);
            var dbBlogMap = table.ToList().ToMultimap(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var source in sourceBlogs)
            {
                if (dbBlogMap.ContainsKey(source.Title))
                {
                    foreach (var target in dbBlogMap[source.Title])
                    {
                        if (source.Title.HasValue()) target.Title = source.Title;
                        if (source.MetaTitle.HasValue()) target.MetaTitle = source.MetaTitle;
                        if (source.MetaDescription.HasValue()) target.MetaDescription = source.MetaDescription;
                        if (source.Intro.HasValue()) target.Intro = source.Intro;
                        if (source.Body.HasValue()) target.Body = source.Body;
                        if (source.Tags.HasValue()) target.Tags = source.Tags;
                        if (source.CreatedOnUtc != null) target.CreatedOnUtc = source.CreatedOnUtc;
                        if (source.MediaFile != null) target.MediaFile = source.MediaFile;
                        if (source.PreviewMediaFile != null) target.PreviewMediaFile = source.PreviewMediaFile;
                        if (source.SectionBg.HasValue()) target.SectionBg = source.SectionBg;
                        target.DisplayTagsInPreview = source.DisplayTagsInPreview;
                        target.PreviewDisplayType = source.PreviewDisplayType;
                        target.AllowComments = source.AllowComments;
                    }
                }
                else
                {
                    var blog = new BlogPost
                    {
                        Title = source.Title,
                        MetaTitle = source.MetaTitle,
                        MetaDescription = source.MetaDescription,
                        Intro = source.Intro,
                        Body = source.Body,                        
                        Tags = source.Tags,
                        DisplayTagsInPreview = source.DisplayTagsInPreview,
                        CreatedOnUtc = source.CreatedOnUtc,
                        PreviewDisplayType = source.PreviewDisplayType,
                        MediaFile = source.MediaFile,
                        PreviewMediaFile = source.PreviewMediaFile,
                        AllowComments = source.AllowComments,
                        SectionBg = source.SectionBg,
                        IsPublished = true
                    };

                    blogsImported.Add(blog);
                    table.Add(blog);
                }
            }

            _ctx.SaveChanges();
            return blogsImported;
        }

        private DirectoryInfo ResolveBlogDirectory(Language language, string virtualRootPath = null)
        {
            var testPaths = new[]
            {
                language.LanguageCulture,
                language.GetTwoLetterISOLanguageName(),
                "en"
            };

            var rootPath = CommonHelper.MapPath(virtualRootPath.NullEmpty() ?? "~/App_Data/Samples/blog/");
            foreach (var path in testPaths.Select(x => Path.Combine(rootPath, x)))
            {
                if (Directory.Exists(path))
                {
                    return new DirectoryInfo(path);
                }
            }

            throw new DirectoryNotFoundException($"Could not obtain an blog post path for language {language.LanguageCulture}. Fallback to 'en' failed, because directory does not exist.");
        }

        private BlogPost DeserializeBlog(string fullPath)
        {
            return DeserializeDocument(XDocument.Load(fullPath));
        }

        private BlogPost DeserializeDocument(XDocument doc)
        {
            var result = new BlogPost();
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
                    case "Intro":
                        result.Intro = value;
                        break;
                    case "Body":
                        result.Body = value;
                        break;
                    case "Tags":
                        result.Tags = value;
                        break;
                    case "DisplayTags":
                        result.DisplayTagsInPreview = value.ToBool();
                        break;
                    case "CreatedOn":
                        result.CreatedOnUtc = value.ToDateTime(new DateTime()).Value;
                        break;
                    case "DisplayType":
                        result.PreviewDisplayType = (PreviewDisplayType)int.Parse(value);
                        break;
                    case "Image":
                        var seName = GetSeName(Path.GetFileNameWithoutExtension(value));
                        result.MediaFile = CreateImage(value, seName);
                        break;
                    case "ImagePreview":
                        seName = GetSeName(Path.GetFileNameWithoutExtension(value));
                        result.PreviewMediaFile = CreateImage(value, seName);
                        break;
                    case "Comments":
                        result.AllowComments = value.ToBool();
                        break;
                    case "SectionBg":
                        result.SectionBg = value;
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
                var path = Path.Combine(CommonHelper.MapPath("~/App_Data/Samples/blog/"), fileName).Replace('/', '\\');
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