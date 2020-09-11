using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Data;
using SmartStore.Core.Themes;
using SmartStore.Core.Caching;
using SmartStore.Core.Events;
using System.Xml;
using SmartStore.Core;
using System.Net;
using System.Web;
using System.IO;

namespace SmartStore.Services.Themes
{
    public class ThemeVariablesService : IThemeVariablesService
    {
        private const string THEMEVARS_BY_THEME_KEY = "SmartStore.themevars.theme-{0}-{1}";
        private const string THEMEVARS_PATTERN_KEY = "SmartStore.themevars.";

        private readonly IRepository<ThemeVariable> _rsVariables;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IRequestCache _requestCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly Lazy<IThemeFileResolver> _themeFileResolver;
        private readonly HttpContextBase _httpContext;

        public ThemeVariablesService(
            IRepository<ThemeVariable> rsVariables,
            IThemeRegistry themeRegistry,
            IRequestCache requestCache,
            IEventPublisher eventPublisher,
            Lazy<IThemeFileResolver> themeFileResolver,
            HttpContextBase httpContext)
        {
            _rsVariables = rsVariables;
            _themeRegistry = themeRegistry;
            _requestCache = requestCache;
            _eventPublisher = eventPublisher;
            _themeFileResolver = themeFileResolver;
            _httpContext = httpContext;
        }

        public virtual ExpandoObject GetThemeVariables(string themeName, int storeId)
        {
            if (themeName.IsEmpty())
                return null;

            if (!_themeRegistry.ThemeManifestExists(themeName))
                return null;

            string key = string.Format(THEMEVARS_BY_THEME_KEY, themeName, storeId);
            return _requestCache.Get(key, () =>
            {
                var result = new ExpandoObject();
                var dict = result as IDictionary<string, object>;

                // first get all default (static) var values from manifest...
                var manifest = _themeRegistry.GetThemeManifest(themeName);
                manifest.Variables.Values.Each(v =>
                {
                    dict.Add(v.Name, v.DefaultValue);
                });

                // ...then merge with persisted runtime records
                var query = from v in _rsVariables.TableUntracked
                            where v.StoreId == storeId && v.Theme.Equals(themeName, StringComparison.OrdinalIgnoreCase)
                            select v;

                query.ToList().Each(v =>
                {
                    if (v.Value.HasValue() && dict.ContainsKey(v.Name))
                    {
                        dict[v.Name] = v.Value;
                    }
                });

                return result;
            });
        }

        public virtual void DeleteThemeVariables(string themeName, int storeId)
        {
            DeleteThemeVariablesInternal(themeName, storeId);
        }

        private void DeleteThemeVariablesInternal(string themeName, int storeId)
        {
            Guard.NotEmpty(themeName, nameof(themeName));

            var query = from v in _rsVariables.Table
                        where v.StoreId == storeId && v.Theme == themeName
                        select v;

            if (query.Any())
            {
                using (var scope = new DbContextScope(ctx: _rsVariables.Context, autoCommit: false))
                {
                    query.Each(v =>
                    {
                        _rsVariables.Delete(v);
                    });

                    _requestCache.Remove(THEMEVARS_BY_THEME_KEY.FormatInvariant(themeName, storeId));

                    _rsVariables.Context.SaveChanges();
                }
            }
        }

        public virtual int SaveThemeVariables(string themeName, int storeId, IDictionary<string, object> variables)
        {
            Guard.NotEmpty(themeName, nameof(themeName));
            Guard.Against<ArgumentException>(!_themeRegistry.ThemeManifestExists(themeName), "The theme '{0}' does not exist in the registry.".FormatInvariant(themeName));
            Guard.NotNull(variables, nameof(variables));

            if (!variables.Any())
                return 0;

            var manifest = _themeRegistry.GetThemeManifest(themeName);
            if (manifest == null)
            {
                throw new ArgumentException("Theme '{0}' does not exist".FormatInvariant(themeName), nameof(themeName));
            }

            // Get current for later restore on parse error
            var currentVars = GetThemeVariables(themeName, storeId);

            // Save
            var result = SaveThemeVariablesInternal(manifest, storeId, variables);

            if (result.TouchedVariablesCount > 0)
            {
                // Check for parsing error
                string error = ValidateSass(manifest, storeId);
                if (error.HasValue())
                {
                    // Restore previous vars
                    try
                    {
                        DeleteThemeVariablesInternal(themeName, storeId);
                    }
                    finally
                    {
                        // We do it here to absolutely ensure that this gets called
                        SaveThemeVariablesInternal(manifest, storeId, currentVars);
                    }

                    throw new ThemeValidationException(error, variables);
                }
            }

            return result.TouchedVariablesCount;
        }

        private SaveThemeVariablesResult SaveThemeVariablesInternal(ThemeManifest manifest, int storeId, IDictionary<string, object> variables)
        {
            var result = new SaveThemeVariablesResult();
            var infos = manifest.Variables;

            using (var scope = new DbContextScope(ctx: _rsVariables.Context, autoCommit: false))
            {
                var unsavedVars = new List<string>();
                var savedThemeVars = _rsVariables.Table
                    .Where(v => v.StoreId == storeId && v.Theme == manifest.ThemeName)
                    .ToDictionary(x => x.Name);

                bool touched = false;

                foreach (var v in variables)
                {
                    ThemeVariableInfo info;
                    if (!infos.TryGetValue(v.Key, out info))
                    {
                        // var not specified in metadata so don't save
                        // TODO: (MC) delete from db also if it exists
                        continue;
                    }

                    var value = v.Value == null ? string.Empty : v.Value.ToString();

                    var savedThemeVar = savedThemeVars.Get(v.Key);
                    if (savedThemeVar != null)
                    {
                        if (value.IsEmpty() || String.Equals(info.DefaultValue, value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // it's either null or the default value, so delete
                            _rsVariables.Delete(savedThemeVar);
                            result.Deleted.Add(savedThemeVar);
                            touched = true;
                        }
                        else
                        {
                            // update entity
                            if (!savedThemeVar.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
                            {
                                savedThemeVar.Value = value;
                                result.Updated.Add(savedThemeVar);
                                touched = true;
                            }
                        }
                    }
                    else
                    {
                        if (value.HasValue() && !String.Equals(info.DefaultValue, value, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // insert entity (only when not default value)
                            unsavedVars.Add(v.Key);
                            savedThemeVar = new ThemeVariable
                            {
                                Theme = manifest.ThemeName,
                                Name = v.Key,
                                Value = value,
                                StoreId = storeId
                            };
                            _rsVariables.Insert(savedThemeVar);
                            result.Inserted.Add(savedThemeVar);
                            touched = true;
                        }
                    }
                }

                if (touched)
                {
                    _rsVariables.Context.SaveChanges();
                }
            }

            return result;
        }

        public int ImportVariables(string themeName, int storeId, string configurationXml)
        {
            Guard.NotEmpty(themeName, nameof(themeName));
            Guard.NotEmpty(configurationXml, nameof(configurationXml));

            var dict = new Dictionary<string, object>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(configurationXml);

            string forTheme = xmlDoc.DocumentElement.GetAttribute("for");
            if (!forTheme.Equals(themeName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new SmartException("The theme reference in the import file ('{0}') does not match the current theme '{1}'.".FormatCurrent(forTheme.ToSafe(), themeName));
            }

            var xndVars = xmlDoc.DocumentElement.SelectNodes("Var").Cast<XmlElement>();
            foreach (var xel in xndVars)
            {
                string name = xel.GetAttribute("name");
                string value = xel.InnerText;

                if (name.IsEmpty() || value.IsEmpty())
                {
                    continue;
                }

                dict.Add(name, value);
            }

            return this.SaveThemeVariables(themeName, storeId, dict);
        }

        public string ExportVariables(string themeName, int storeId)
        {
            Guard.NotEmpty(themeName, nameof(themeName));

            var vars = this.GetThemeVariables(themeName, storeId) as IDictionary<string, object>;

            if (vars == null || !vars.Any())
                return null;

            var infos = _themeRegistry.GetThemeManifest(themeName).Variables;

            var sb = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(sb))
            {

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("ThemeVars");
                xmlWriter.WriteAttributeString("for", themeName);

                foreach (var kvp in vars)
                {
                    string name = kvp.Key;
                    string value = kvp.Value.ToString();

                    ThemeVariableInfo info;
                    if (!infos.TryGetValue(name, out info))
                    {
                        // var not specified in metadata so don't export
                        continue;
                    }

                    xmlWriter.WriteStartElement("Var");
                    xmlWriter.WriteAttributeString("name", name);
                    xmlWriter.WriteAttributeString("type", info.TypeAsString);
                    xmlWriter.WriteString(value);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }

            return sb.ToString();
        }


        /// <summary>
        /// Validates the result SASS file by calling it's url.
        /// </summary>
        /// <param name="theme">Theme name</param>
        /// <param name="storeId">Stored Id</param>
        /// <returns>The error message when a parsing error occured, <c>null</c> otherwise</returns>
        private string ValidateSass(ThemeManifest manifest, int storeId)
        {
            string error = string.Empty;

            var virtualPath = "~/Themes/{0}/Content/theme.scss".FormatCurrent(manifest.ThemeName);
            var resolver = _themeFileResolver.Value;
            var file = resolver.Resolve(virtualPath);
            if (file != null)
            {
                virtualPath = file.ResultVirtualPath;
            }

            var url = "{0}?storeId={1}&theme={2}&validate=1".FormatInvariant(
                WebHelper.GetAbsoluteUrl(virtualPath, _httpContext.Request),
                storeId,
                manifest.ThemeName);

            var request = WebHelper.CreateHttpRequestForSafeLocalCall(new Uri(url));
            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    var webResponse = (HttpWebResponse)ex.Response;
                    var statusCode = webResponse.StatusCode;

                    if (statusCode == HttpStatusCode.InternalServerError)
                    {
                        // catch only 500, as this indicates a parsing error.
                        var stream = webResponse.GetResponseStream();

                        using (var streamReader = new StreamReader(stream))
                        {
                            // read the content (the error message has been put there)
                            error = streamReader.ReadToEnd();
                            streamReader.Close();
                            stream.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return error;
        }
    }

    class SaveThemeVariablesResult
    {
        public SaveThemeVariablesResult()
        {
            Inserted = new List<ThemeVariable>();
            Updated = new List<ThemeVariable>();
            Deleted = new List<ThemeVariable>();
        }

        public IList<ThemeVariable> Inserted { get; private set; }
        public IList<ThemeVariable> Updated { get; private set; }
        public IList<ThemeVariable> Deleted { get; private set; }

        public int TouchedVariablesCount => Inserted.Count + Updated.Count + Deleted.Count;
    }
}
