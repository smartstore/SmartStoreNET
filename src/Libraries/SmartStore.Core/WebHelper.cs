using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Core
{
    public partial class WebHelper : IWebHelper
    {
        private static readonly object s_lock = new object();
        private static bool? s_optimizedCompilationsEnabled;
        private static AspNetHostingPermissionLevel? s_trustLevel;
        private static readonly Regex s_staticExts = new Regex(@"(.*?)\.(css|js|png|jpg|jpeg|gif|webp|liquid|bmp|html|htm|xml|txt|pdf|doc|xls|rar|zip|7z|ico|eot|svg|ttf|woff|woff2|otf|json)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_htmlPathPattern = new Regex(@"(?<=(?:href|src)=(?:""|'))(?!https?://)(?<url>[^(?:""|')]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex s_cssPathPattern = new Regex(@"url\('(?<url>.+)'\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly ConcurrentDictionary<int, string> s_safeLocalHostNames = new ConcurrentDictionary<int, string>();

        private readonly HttpContextBase _httpContext;
        private bool? _isCurrentConnectionSecured;
        private string _storeHost;
        private string _storeHostSsl;
        private string _ipAddress;
        private bool? _appPathPossiblyAppended;
        private bool? _appPathPossiblyAppendedSsl;

        private Store _currentStore;

        public WebHelper(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public virtual string GetUrlReferrer()
        {
            return _httpContext.SafeGetHttpRequest()?.UrlReferrer?.ToString() ?? string.Empty;
        }

        public virtual string GetClientIdent()
        {
            var ipAddress = this.GetCurrentIpAddress();
            var userAgent = _httpContext.SafeGetHttpRequest()?.UserAgent.EmptyNull();

            if (ipAddress.HasValue() && userAgent.HasValue())
            {
                return (ipAddress + userAgent).GetHashCode().ToString();
            }

            return null;
        }

        public virtual string GetCurrentIpAddress()
        {
            if (_ipAddress != null)
            {
                return _ipAddress;
            }

            var httpRequest = _httpContext.SafeGetHttpRequest();
            if (httpRequest == null)
            {
                return string.Empty;
            }

            var vars = httpRequest.ServerVariables;

            var keysToCheck = new string[]
            {
                "HTTP_CLIENT_IP",
                "HTTP_X_FORWARDED_FOR",
                "HTTP_X_FORWARDED",
                "HTTP_X_CLUSTER_CLIENT_IP",
                "HTTP_FORWARDED_FOR",
                "HTTP_FORWARDED",
                "REMOTE_ADDR",
                "HTTP_CF_CONNECTING_IP"
            };

            string result = null;
            IPAddress ipv6 = null;

            foreach (var key in keysToCheck)
            {
                var ipString = vars[key];

                if (!string.IsNullOrEmpty(ipString))
                {
                    var arrStrings = ipString.Split(',');

                    // Iterate list from end to start (IPv6 addresses usually have precedence)
                    for (int i = arrStrings.Length - 1; i >= 0; i--)
                    {
                        ipString = arrStrings[i].Trim();

                        if (IPAddress.TryParse(ipString, out var address))
                        {
                            if (address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                ipv6 = address;
                            }
                            else
                            {
                                result = ipString;
                                break;
                            }
                        }
                    }      

                    if (!string.IsNullOrEmpty(result))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(result) && ipv6 != null)
            {
                result = ipv6.ToString() == "::1"
                    ? "127.0.0.1"
                    : ipv6.MapToIPv4().ToString();
            }

            return (_ipAddress = result.EmptyNull());
        }

        public virtual string GetThisPageUrl(bool includeQueryString)
        {
            bool useSsl = IsCurrentConnectionSecured();
            return GetThisPageUrl(includeQueryString, useSsl);
        }

        public virtual string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            string url = string.Empty;
            var httpRequest = _httpContext.SafeGetHttpRequest();

            if (httpRequest == null)
                return url;

            var authority = httpRequest.Url.GetLeftPart(UriPartial.Authority);
            var path = httpRequest.RawUrl;
            var schemeChanges = useSsl != IsCurrentConnectionSecured();

            if (!schemeChanges && includeQueryString)
            {
                // Return as is
                return authority + path;
            }

            if (schemeChanges)
            {
                authority = GetStoreHost(useSsl, out bool appPathPossiblyAppended).TrimEnd('/');
                if (appPathPossiblyAppended)
                {
                    path = _httpContext.GetOriginalAppRelativePath().TrimStart('~');
                    if (includeQueryString && httpRequest.Url?.Query != null)
                    {
                        path += httpRequest.Url.Query;
                    }
                }
            }

            if (!includeQueryString)
            {
                var queryIndex = path.IndexOf('?');
                if (queryIndex > -1)
                {
                    path = path.Substring(0, queryIndex);
                }
            }

            return authority + path;
        }

        public virtual bool IsCurrentConnectionSecured()
        {
            if (!_isCurrentConnectionSecured.HasValue)
            {
                _isCurrentConnectionSecured = false;
                var httpRequest = _httpContext.SafeGetHttpRequest();
                if (httpRequest != null)
                {
                    _isCurrentConnectionSecured = httpRequest.IsHttps();
                }
            }

            return _isCurrentConnectionSecured.Value;
        }

        public virtual string ServerVariables(string name)
        {
            return _httpContext.SafeGetHttpRequest()?.ServerVariables[name].EmptyNull();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private string GetHostPart(string url)
        {
            var uri = new Uri(url);
            var host = uri.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped);
            return host;
        }

        private string GetStoreHost(bool useSsl, out bool appPathPossiblyAppended)
        {
            string cached = useSsl ? _storeHostSsl : _storeHost;
            if (cached != null)
            {
                appPathPossiblyAppended = useSsl ? _appPathPossiblyAppendedSsl.Value : _appPathPossiblyAppended.Value;
                return cached;
            }

            appPathPossiblyAppended = false;
            var result = "";
            var httpHost = ServerVariables("HTTP_HOST");

            if (httpHost.HasValue())
            {
                result = "http://" + httpHost.EnsureEndsWith("/");
            }

            if (!DataSettings.DatabaseIsInstalled())
            {
                if (useSsl)
                {
                    // Secure URL is not specified.
                    // So a store owner wants it to be detected automatically.
                    result = result.Replace("http:/", "https:/");
                }
            }
            else
            {
                // Let's resolve IWorkContext  here.
                // Do not inject it via contructor because it'll cause circular references

                if (_currentStore == null)
                {
                    if (EngineContext.Current.ContainerManager.TryResolve<IStoreContext>(null, out IStoreContext storeContext)) // Unit test safe!
                    {
                        _currentStore = storeContext.CurrentStore;
                        if (_currentStore == null)
                            throw new Exception("Current store cannot be loaded");
                    }
                }

                if (_currentStore != null)
                {
                    var securityMode = _currentStore.GetSecurityMode();

                    if (httpHost.IsEmpty())
                    {
                        // HTTP_HOST variable is not available.
                        // It's possible only when HttpContext is not available (for example, running in a schedule task)
                        result = _currentStore.Url.EnsureEndsWith("/");

                        appPathPossiblyAppended = true;
                    }

                    if (useSsl)
                    {
                        if (securityMode == HttpSecurityMode.SharedSsl)
                        {
                            // Secure URL for shared ssl specified. 
                            // So a store owner doesn't want it to be resolved automatically.
                            // In this case let's use the specified secure URL
                            result = _currentStore.SecureUrl.EmptyNull();

                            if (!result.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                result = "https://" + result;
                            }

                            appPathPossiblyAppended = true;
                        }
                        else
                        {
                            // Secure URL is not specified.
                            // So a store owner wants it to be resolved automatically.
                            result = result.Replace("http:/", "https:/");
                        }
                    }
                    else // no ssl
                    {
                        if (securityMode == HttpSecurityMode.SharedSsl)
                        {
                            // SSL is enabled in this store and shared ssl URL is specified.
                            // So a store owner doesn't want it to be resolved automatically.
                            // In this case let's use the specified non-secure URL

                            result = _currentStore.Url;
                            appPathPossiblyAppended = true;
                        }
                    }
                }
            }

            // cache results for request
            result = result.EnsureEndsWith("/");
            if (useSsl)
            {
                _storeHostSsl = result;
                _appPathPossiblyAppendedSsl = appPathPossiblyAppended;
            }
            else
            {
                _storeHost = result;
                _appPathPossiblyAppended = appPathPossiblyAppended;
            }

            return result;
        }

        public virtual string GetStoreLocation()
        {
            bool useSsl = IsCurrentConnectionSecured();
            return GetStoreLocation(useSsl);
        }

        public virtual string GetStoreLocation(bool useSsl)
        {
            string result = GetStoreHost(useSsl, out var appPathPossiblyAppended);

            if (result.EndsWith("/"))
            {
                result = result.Substring(0, result.Length - 1);
            }

            var httpRequest = _httpContext.SafeGetHttpRequest();

            if (httpRequest != null)
            {
                var appPath = httpRequest.ApplicationPath;
                if (!appPathPossiblyAppended && !result.EndsWith(appPath, StringComparison.OrdinalIgnoreCase))
                {
                    // in a shared ssl scenario the user defined https url could contain
                    // the app path already. In this case we must not append.
                    result += appPath;
                }
            }

            if (!result.EndsWith("/"))
            {
                result += "/";
            }

            return result;
        }

        public virtual bool IsStaticResource(HttpRequest request)
        {
            return IsStaticResourceRequested(new HttpRequestWrapper(request));
        }

        public static bool IsStaticResourceRequested(HttpRequest request)
        {
            Guard.NotNull(request, nameof(request));
            return s_staticExts.IsMatch(request.Path);
        }

        public static bool IsStaticResourceRequested(HttpRequestBase request)
        {
            // unit testable
            Guard.NotNull(request, nameof(request));
            return s_staticExts.IsMatch(request.Path);
        }

        public virtual string MapPath(string path)
        {
            return CommonHelper.MapPath(path, false);
        }

        public virtual string ModifyQueryString(string url, string queryStringModification, string anchor)
        {
            url = url.EmptyNull();
            queryStringModification = queryStringModification.EmptyNull();

            string curAnchor = null;

            var hsIndex = url.LastIndexOf('#');
            if (hsIndex >= 0)
            {
                curAnchor = url.Substring(hsIndex);
                url = url.Substring(0, hsIndex);
            }

            var parts = url.Split(new[] { '?' });
            var current = new QueryString(parts.Length == 2 ? parts[1] : "");
            var modify = new QueryString(queryStringModification);

            foreach (var nv in modify.AllKeys)
            {
                current.Add(nv, modify[nv], true);
            }

            var result = string.Concat(
                parts[0],
                current.ToString(false),
                anchor.NullEmpty() == null ? (curAnchor == null ? "" : "#" + curAnchor) : "#" + anchor
            );

            return result;
        }

        public virtual string RemoveQueryString(string url, string queryString)
        {
            var parts = url.SplitSafe("?");

            var current = new QueryString(parts.Length == 2 ? parts[1] : "");

            if (current.Count > 0 && queryString.HasValue())
            {
                current.Remove(queryString);
            }

            var result = string.Concat(parts[0], current.ToString(false));
            return result;
        }

        public virtual T QueryString<T>(string name)
        {
            var queryParam = _httpContext.SafeGetHttpRequest()?.QueryString[name];

            if (!string.IsNullOrEmpty(queryParam))
            {
                return queryParam.Convert<T>();
            }

            return default(T);
        }

        public virtual void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "", bool aggressive = false)
        {
            HttpRuntime.UnloadAppDomain();

            if (aggressive)
            {
                // When plugins are (un)installed, 'aggressive' is always true.
                if (OptimizedCompilationsEnabled)
                {
                    // Very hackish:
                    // If optimizedCompilations is on per web.config, touching top-level resources
                    // like global.asax or bin folder is meaningless, 'cause ASP.NET skips these for
                    // hash calculation. This way we can throw in plugins like crazy without invalidating
                    // ASP.NET temp files, which boosts app startup performance dramatically.
                    // Unfortunately, MVC keeps a controller cache file in the temp files folder, which NEVER
                    // gets nuked, unless the 'compilation' element in web.config is changed.
                    // We MUST delete this file in order to ensure that it gets re-created with our new controller types in it.
                    DeleteMvcTypeCacheFiles();
                }
                else
                {
                    // Without optimizedCompilations, touching anything in the bin folder nukes ASP.NET temp folder completely,
                    // including compiled views, MVC cache files etc.
                    TryWriteBinFolder();
                }
            }
            else
            {
                // without this, MVC may fail resolving controllers for newly installed plugins after IIS restart
                Thread.Sleep(250);
            }

            // If setting up plugins requires an AppDomain restart, it's very unlikely the
            // current request can be processed correctly.  So, we redirect to the same URL, so that the
            // new request will come to the newly started AppDomain.
            if (_httpContext != null && makeRedirect)
            {
                if (_httpContext.Request.RequestType == "GET")
                {
                    if (string.IsNullOrEmpty(redirectUrl))
                    {
                        redirectUrl = GetThisPageUrl(true);
                    }
                    _httpContext.Response.Redirect(redirectUrl, true /*endResponse*/);
                }
                else
                {
                    // Don't redirect posts...
                    _httpContext.Response.ContentType = "text/html";
                    _httpContext.Response.WriteFile("~/refresh.html");
                    _httpContext.Response.End();
                }
            }
        }

        private void DeleteMvcTypeCacheFiles()
        {
            try
            {
                var userCacheDir = Path.Combine(HttpRuntime.CodegenDir, "UserCache");

                File.Delete(Path.Combine(userCacheDir, "MVC-ControllerTypeCache.xml"));
                File.Delete(Path.Combine(userCacheDir, "MVC-AreaRegistrationTypeCache.xml"));
            }
            catch { }
        }

        private bool TryWriteBinFolder()
        {
            try
            {
                var binMarker = MapPath("~/bin/HostRestart");
                Directory.CreateDirectory(binMarker);

                using (var stream = File.CreateText(Path.Combine(binMarker, "marker.txt")))
                {
                    stream.WriteLine("Restart on '{0}'", DateTime.UtcNow);
                    stream.Flush();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool OptimizedCompilationsEnabled
        {
            get
            {
                if (!s_optimizedCompilationsEnabled.HasValue)
                {
                    var section = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
                    s_optimizedCompilationsEnabled = section.OptimizeCompilations;
                }

                return s_optimizedCompilationsEnabled.Value;
            }
        }

        /// <summary>
        /// Finds the trust level of the running application (http://blogs.msdn.com/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx)
        /// </summary>
        /// <returns>The current trust level.</returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!s_trustLevel.HasValue)
            {
                // set minimum
                s_trustLevel = AspNetHostingPermissionLevel.None;

                // determine maximum
                foreach (AspNetHostingPermissionLevel trustLevel in
                        new[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal
                            })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        s_trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return s_trustLevel.Value;
        }

        /// <summary>
        /// Prepends protocol and host to all (relative) urls in a html string
        /// </summary>
        /// <param name="html">The html string</param>
        /// <param name="request">Request object</param>
        /// <returns>The transformed result html</returns>
        /// <remarks>
        /// All html attributed named <c>src</c> and <c>href</c> are affected, also occurences of <c>url('path')</c> within embedded stylesheets.
        /// </remarks>
        public static string MakeAllUrlsAbsolute(string html, HttpRequestBase request)
        {
            Guard.NotNull(request, nameof(request));

            if (request.Url == null)
            {
                return html;
            }

            return MakeAllUrlsAbsolute(html, request.Url.Scheme, request.Url.Authority);
        }

        /// <summary>
        /// Prepends protocol and host to all (relative) urls in a html string
        /// </summary>
        /// <param name="html">The html string</param>
        /// <param name="protocol">The protocol to prepend, e.g. <c>http</c></param>
        /// <param name="host">The host name to prepend, e.g. <c>www.mysite.com</c></param>
        /// <returns>The transformed result html</returns>
        /// <remarks>
        /// All html attributed named <c>src</c> and <c>href</c> are affected, also occurences of <c>url('path')</c> within embedded stylesheets.
        /// </remarks>
        public static string MakeAllUrlsAbsolute(string html, string protocol, string host)
        {
            Guard.NotEmpty(html, nameof(html));
            Guard.NotEmpty(protocol, nameof(protocol));
            Guard.NotEmpty(host, nameof(host));

            string baseUrl = protocol.EnsureEndsWith("://") + host.TrimEnd('/');

            MatchEvaluator evaluator = (match) =>
            {
                var url = match.Groups["url"].Value;
                return "{0}{1}".FormatCurrent(baseUrl, url.EnsureStartsWith("/"));
            };

            html = s_htmlPathPattern.Replace(html, evaluator);
            html = s_cssPathPattern.Replace(html, evaluator);

            return html;
        }

        /// <summary>
        /// Prepends protocol and host to the given (relative) url
        /// </summary>
        /// <param name="protocol">Changes the protocol if passed.</param>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public static string GetAbsoluteUrl(string url, HttpRequestBase request, bool enforceScheme = false, string protocol = null)
        {
            Guard.NotEmpty(url, nameof(url));
            Guard.NotNull(request, nameof(request));

            if (request.Url == null)
            {
                return url;
            }

            if (url.Contains("://"))
            {
                return url;
            }

            protocol = protocol ?? request.Url.Scheme;

            if (url.StartsWith("//"))
            {
                return enforceScheme
                    ? String.Concat(protocol, ":", url)
                    : url;
            }

            if (url.StartsWith("~"))
            {
                url = VirtualPathUtility.ToAbsolute(url);
            }

            url = string.Format("{0}://{1}{2}", protocol, request.Url.Authority, url);
            return url;
        }

        public static string GetPublicIPAddress()
        {
            string result = string.Empty;

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                    try
                    {
                        byte[] arr = client.DownloadData("http://checkip.amazonaws.com/");
                        string response = Encoding.UTF8.GetString(arr);
                        result = response.Trim();
                    }
                    catch { }
                }
            }
            catch { }

            var checkers = new string[]
            {
                "https://ipinfo.io/ip",
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://wtfismyip.com/text",
                "http://bot.whatismyipaddress.com/"
            };

            if (string.IsNullOrEmpty(result))
            {
                using (var client = new WebClient())
                {
                    foreach (var checker in checkers)
                    {
                        try
                        {
                            result = client.DownloadString(checker).Replace("\n", "");
                            if (!string.IsNullOrEmpty(result))
                            {
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                try
                {
                    var url = "http://checkip.dyndns.org";
                    var req = WebRequest.Create(url);
                    using (var resp = req.GetResponse())
                    {
                        using (var sr = new StreamReader(resp.GetResponseStream()))
                        {
                            var response = sr.ReadToEnd().Trim();
                            var a = response.Split(':');
                            var a2 = a[1].Substring(1);
                            var a3 = a2.Split('<');
                            result = a3[0];
                        }
                    }
                }
                catch { }
            }

            return result;
        }

        public static HttpWebRequest CreateHttpRequestForSafeLocalCall(Uri requestUri)
        {
            Guard.NotNull(requestUri, nameof(requestUri));

            var safeHostName = GetSafeLocalHostName(requestUri);

            var uri = requestUri;

            if (!requestUri.Host.Equals(safeHostName, StringComparison.OrdinalIgnoreCase))
            {
                var url = String.Format("{0}://{1}{2}",
                    requestUri.Scheme,
                    requestUri.IsDefaultPort ? safeHostName : safeHostName + ":" + requestUri.Port,
                    requestUri.PathAndQuery);
                uri = new Uri(url);
            }

            var request = WebRequest.CreateHttp(uri);
            request.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
            request.ServicePoint.Expect100Continue = false;
            request.UserAgent = "Smartstore {0}".FormatInvariant(SmartStoreVersion.CurrentFullVersion);

            return request;
        }

        private static string GetSafeLocalHostName(Uri requestUri)
        {
            return s_safeLocalHostNames.GetOrAdd(requestUri.Port, (port) =>
            {
                // first try original host
                if (TestHost(requestUri, requestUri.Host, 5000))
                {
                    return requestUri.Host;
                }

                // try loopback
                var hostName = Dns.GetHostName();
                var hosts = new List<string> { "localhost", hostName, "127.0.0.1" };
                foreach (var host in hosts)
                {
                    if (TestHost(requestUri, host, 500))
                    {
                        return host;
                    }
                }

                // try local IP addresses
                hosts.Clear();
                var ipAddresses = Dns.GetHostAddresses(hostName).Where(x => x.AddressFamily == AddressFamily.InterNetwork).Select(x => x.ToString());
                hosts.AddRange(ipAddresses);

                foreach (var host in hosts)
                {
                    if (TestHost(requestUri, host, 500))
                    {
                        return host;
                    }
                }

                // None of the hosts are callable. WTF?
                return requestUri.Host;
            });
        }

        private static bool TestHost(Uri originalUri, string host, int timeout)
        {
            var url = String.Format("{0}://{1}/taskscheduler/noop",
                originalUri.Scheme,
                originalUri.IsDefaultPort ? host : host + ":" + originalUri.Port);
            var uri = new Uri(url);

            var request = WebRequest.CreateHttp(uri);
            request.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
            request.ServicePoint.Expect100Continue = false;
            request.UserAgent = "Smartstore";
            request.Timeout = timeout;

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {
                // try the next host
            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }

            return false;
        }
    }
}
