using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Bundles;
using SmartStore.Core;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Theming.Assets;
using SmartStore.Core.Themes;

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
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
		private readonly IThemeContext _themeContext;

        private static readonly object s_lock = new object();

        public BundleBuilder(IStoreContext storeContext, IWorkContext workContext, IThemeContext themeContext)
        {
            this._storeContext = storeContext;
            this._workContext = workContext;
			this._themeContext = themeContext;
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
						var nullOrderer = new NullOrderer();

						Bundle bundle = (type == BundleType.Script) ?
                            new CustomScriptBundle(bundleVirtualPath) as Bundle :
                            new SmartStyleBundle(bundleVirtualPath) as Bundle;
						bundle.Orderer = nullOrderer;

						bundle.Include(files.ToArray());

                        BundleTable.Bundles.Add(bundle);
                    }
                }
            }

            if (type == BundleType.Script)
			{
				return Scripts.Render(bundleVirtualPath).ToString();

				//// Uncomment this if you want to bypass asset caching on mobile browsers
				//return Scripts.RenderFormat("<script src='{0}?" + CommonHelper.GenerateRandomDigitCode(5) + "'></script>", 
				//	files.Select(x => VirtualPathUtility.ToAppRelative(x)).ToArray()).ToString();
			}   

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

			// TBD: routing fix
			postfix = "";

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

                // append StoreId & ThemeName to hash in order to vary cache by store/theme combination
                if (type == BundleType.Stylesheet && !_workContext.IsAdmin && files.Any(x => x.EndsWith(".scss", StringComparison.OrdinalIgnoreCase)))
                {
                    hash += "-s" + _storeContext.CurrentStore.Id;
					hash += "-t" + _themeContext.CurrentTheme.ThemeName;
                }
            }

            // ensure only valid chars
            hash = SeoExtensions.GetSeName(hash);

            var sb = new StringBuilder(prefix);
            sb.Append(hash);
			sb.Append(postfix); 
            return sb.ToString();
        }
    }
}
