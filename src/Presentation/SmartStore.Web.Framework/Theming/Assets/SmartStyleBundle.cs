using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Optimization;
using BundleTransformer.Core;
using BundleTransformer.Core.Builders;

namespace SmartStore.Web.Framework.Theming.Assets
{
    internal class SmartStyleBundle : Bundle
    {
        private bool _wasCached = false;
        private string[] _originalBundleFilePathes;
        private IEnumerable<BundleFile> _transformedBundleFiles;

        public SmartStyleBundle(string virtualPath)
            : this(virtualPath, null)
        { }

        [SuppressMessage("ReSharper", "VirtualMemberCallInContructor")]
        public SmartStyleBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath, new IBundleTransform[] { BundleTransformerContext.Current.Styles.GetDefaultTransformInstance() })
        {
            Builder = new NullBuilder();
        }

        public override BundleResponse GenerateBundleResponse(BundleContext context)
        {
            // This is overridden, because BudleTransformer adds LESS/SASS @imports
            // to the Bundle.Files collection. This is bad as we allow switching
            // Optimization mode per UI. Switching from true to false would also include
            // ALL LESS/SASS imports in the generated output ('link' tags)

            // get all ORIGINAL bundle parts (including LESS/SASS parents, no @imports)
            var files = this.EnumerateFiles(context);

            // replace file pattern like {version} and let Bundler resolve min/debug extensions.
            files = context.BundleCollection.FileExtensionReplacementList.ReplaceFileExtensions(context, files);
            // save originals for later use
            _originalBundleFilePathes = files.Select(x => x.IncludedVirtualPath.TrimStart('~')).ToArray();

            var response = base.GenerateBundleResponse(context);
            // at this stage, BundleTransformer pushed ALL LESS/SASS @imports to Bundle.Files, which is bad...

            _transformedBundleFiles = response.Files;

            if (!CacheIsEnabled(context))
            {
                // ...so we must clean the file list immediately when caching is disabled ('cause UpdateCache() will not run)
                CleanBundleFiles(response);
            }

            return response;
        }

        public override void UpdateCache(BundleContext context, BundleResponse response)
        {
            if (_wasCached && _transformedBundleFiles != null && _transformedBundleFiles.Any())
            {
                response.Files = _transformedBundleFiles;
            }

            // update cache WITH Sass/Less @imports, because they need to be monitored for cache invalidation
            base.UpdateCache(context, response);

            // now clean. @imports are not needed anymore
            CleanBundleFiles(response);

            _wasCached = true;
        }

        private void CleanBundleFiles(BundleResponse response)
        {
            if (_originalBundleFilePathes == null || _originalBundleFilePathes.Length == 0)
                return;

            var files = response.Files;
            if (_originalBundleFilePathes.Length == files.Count())
                return;

            var cleanFiles = from file in files
                             let virtualPath = file.IncludedVirtualPath
                             where _originalBundleFilePathes.Any(x => virtualPath.EndsWith(x, StringComparison.OrdinalIgnoreCase))
                             select file;

            response.Files = cleanFiles;
        }

        private static bool CacheIsEnabled(BundleContext context)
        {
            Guard.NotNull(context, nameof(context));

            return context.HttpContext?.Cache != null && !context.EnableInstrumentation;

        }
    }

}
