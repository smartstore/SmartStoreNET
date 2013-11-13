using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using SmartStore.Services.Seo;

namespace SmartStore.Web.Framework.UI
{

    public enum BundleType
    {
        Script,
        Stylesheet
    }

    public interface IBundleBuilder
    {
        string Build(BundleType type, IEnumerable<string> files);
    }

    public class BundleBuilder : IBundleBuilder
    {

        private static readonly object s_lock = new object();

        public BundleBuilder()
        {
        }

        public string Build(BundleType type, IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                return string.Empty;

            string bundleVirtualPath = this.GetBundleVirtualPath(type, files);
            var bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
            if (bundleFor == null)
            {
                lock (s_lock)
                {
                    bundleFor = BundleTable.Bundles.GetBundleFor(bundleVirtualPath);
                    if (bundleFor == null)
                    {
                        Bundle bundle = (type == BundleType.Script) ? 
                            new ScriptBundle(bundleVirtualPath) as Bundle: 
                            new StyleBundle(bundleVirtualPath) as Bundle;
                        bundle.Orderer = new NullBundleOrderer();
                        //bundle.EnableFileExtensionReplacements = false;

                        if (type == BundleType.Script)
                        {
                            bundle.Include(files.ToArray());
                        }
                        else
                        {
                            files.Each(x => bundle.Include(x, new CssRewriteUrlTransform()));
                            //bundle.Transforms.Add(new CssUrlTransform());
                        }

                        BundleTable.Bundles.Add(bundle);
                        BundleTable.Bundles.IgnoreList.Clear();
                    }
                }
            }

            if (type == BundleType.Script)
                return Scripts.Render(bundleVirtualPath).ToString();

            return Styles.Render(bundleVirtualPath).ToString();
        }

        protected virtual string GetBundleVirtualPath(BundleType type, IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("parts");

            string prefix = "~/bundles/js/";
            string postfix = ".js";
            if (type == BundleType.Stylesheet)
            {
                prefix = "~/bundles/css/";
                postfix = ".css";
            }

            // compute hash
            var hash = "";
            using (SHA256 sha = new SHA256Managed())
            {
                var hashInput = "";
                foreach (var file in files.OrderBy(x => x))
                {
                    hashInput += file;
                    hashInput += ",";
                }

                byte[] input = sha.ComputeHash(Encoding.Unicode.GetBytes(hashInput));
                hash = HttpServerUtility.UrlTokenEncode(input);
            }
            // ensure only valid chars
            hash = SeoExtensions.GetSeName(hash);

            var sb = new StringBuilder(prefix);
            sb.Append(hash);
            sb.Append(postfix);
            return sb.ToString();
        }

        public class NullBundleOrderer : IBundleOrderer
        {
            public virtual IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
            {
                return files;
            }
        }

        public class CssUrlTransform : IBundleTransform
        {
            public void Process(BundleContext context, BundleResponse response)
            {
                Regex pattern = new Regex(@"url\s*\(\s*([""']?)([^:)]+)\1\s*\)", RegexOptions.IgnoreCase);

                response.Content = string.Empty;

                // open each of the files
                foreach (var file in response.Files)
                {
                    using (var reader = new StreamReader(file.VirtualFile.Open()))
                    {
                        var contents = reader.ReadToEnd();

                        // apply the RegEx to the file (to change relative paths)
                        var matches = pattern.Matches(contents);

                        if (matches.Count > 0)
                        {
                            var directoryPath = VirtualPathUtility.GetDirectory(file.IncludedVirtualPath);

                            foreach (Match match in matches)
                            {
                                // this is a path that is relative to the CSS file
                                var imageRelativePath = match.Groups[2].Value;

                                // get the image virtual path
                                var imageVirtualPath = VirtualPathUtility.Combine(directoryPath, imageRelativePath);

                                // convert the image virtual path to absolute
                                var quote = match.Groups[1].Value;
                                var replace = String.Format("url({0}{1}{0})", quote, VirtualPathUtility.ToAbsolute(imageVirtualPath));
                                contents = contents.Replace(match.Groups[0].Value, replace);
                            }

                        }
                        // copy the result into the response.
                        response.Content = String.Format("{0}\r\n{1}", response.Content, contents);
                    }
                }
            }

        }
    }

}
