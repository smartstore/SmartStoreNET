using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// Activated and executed BEFORE Application_Start. Don't use any dependencies here, they are not bootstrapped yet.
    /// Only invoke low-level code like registering an HttpModule.
    /// </summary>
    public interface IPreApplicationStart
    {
        void Start();
    }

    /// <summary>
    /// Activated and executed DURING Application_Start. Don't use request scoped dependencies here, because <see cref="HttpContext.Current"/> is still <c>null</c> at this stage.
    /// </summary>
    public interface IApplicationStart
    {
        void Start();
        int Order { get; }
    }

    /// <summary>
    /// Activated and executed once AFTER Application_Start with the very first request and very early in the request lifecycle.
    /// Invoke app initialization code here that depends on other services like IDbContext etc.
    /// </summary>
    public interface IPostApplicationStart
    {
        void Start(HttpContextBase httpContext);

        /// <summary>
        /// Called when an error occurred and <see cref="ThrowOnError"/> is <c>false</c>.
        /// </summary>
        /// <param name="exception">The error</param>
        /// <param name="willRetry"><c>true</c> when current attempt count is less than <see cref="MaxAttempts"/>, <c>false</c> otherwise.</param>
        void OnFail(Exception exception, bool willRetry);

        int Order { get; }

        /// <summary>
        /// Whether to throw any error and stop execution of subsequent tasks.
        /// If this is <c>false</c>, the task will be executed <see cref="MaxAttempts"/> times max when an error occurs.
        /// For every error <see cref="OnFail(Exception, bool)"/> will be invoked to give you the chance to do some logging or fix things.
        /// After that, the task will be removed from the queue.
        /// </summary>
        bool ThrowOnError { get; }

        /// <summary>
        /// The number of maximum execution attempts before this task is removed from the queue.
        /// Has no effect if <see cref="ThrowOnError"/> is <c>true</c>.
        /// </summary>
        int MaxAttempts { get; }
    }

    public sealed class PostApplicationStartFilter : IAuthenticationFilter
    {
        private readonly static object _lock = new object();
        private static bool _initializing = false;
        private static List<StarterModuleInfo> _starterModuleInfos;

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            var request = filterContext?.HttpContext?.Request;
            if (request == null)
                return;

            if (filterContext.IsChildAction)
                return;

            lock (_lock)
            {
                if (!_initializing)
                {
                    _initializing = true;

                    var pendingModules = GetStarterModuleInfos();

                    var modules = pendingModules
                        .Select(x => new StarterModule
                        {
                            Info = x,
                            Instance = EngineContext.Current.ContainerManager.ResolveUnregistered(x.ModuleType) as IPostApplicationStart
                        })
                        //.Where(x => x.Info.Attempts < Math.Max(1, x.Instance.MaxAttempts))
                        .OrderBy(x => x.Instance.Order)
                        .ToArray();

                    foreach (var module in modules)
                    {
                        var info = module.Info;
                        var instance = module.Instance;
                        var maxAttempts = Math.Max(1, instance.MaxAttempts);
                        var fail = false;

                        try
                        {
                            info.Attempts++;
                            instance.Start(filterContext.HttpContext);
                        }
                        catch (Exception ex)
                        {
                            fail = true;
                            if (instance.ThrowOnError)
                            {
                                if (info.Attempts <= maxAttempts)
                                {
                                    // Don't pollute event log 
                                    var logger = EngineContext.Current.Resolve<ILoggerFactory>().CreateLogger<PostApplicationStartFilter>();
                                    logger.ErrorFormat(ex, "Error while executing post startup task '{0}': {1}", info.ModuleType, ex.Message);
                                }
                                _initializing = false;
                                throw;
                            }
                            else
                            {
                                instance.OnFail(ex, info.Attempts < maxAttempts);
                            }
                        }
                        finally
                        {
                            var tooManyFailures = info.Attempts >= maxAttempts;
                            var canRemove = !fail || (!instance.ThrowOnError && tooManyFailures);

                            if (canRemove)
                            {
                                pendingModules.Remove(info);
                            }
                        }
                    }

                    if (pendingModules.Count == 0)
                    {
                        // No more pending starter modules anymore.
                        // Don't run this filter from now on.
                        GlobalFilters.Filters.Remove(this);
                    }

                    _initializing = false;
                }
            }
        }

        private static List<StarterModuleInfo> GetStarterModuleInfos()
        {
            if (_starterModuleInfos == null)
            {
                var typeFinder = EngineContext.Current.Resolve<ITypeFinder>();
                var starterTypes = typeFinder.FindClassesOfType<IPostApplicationStart>(true, true);
                _starterModuleInfos = starterTypes
                    .Select(x => new StarterModuleInfo { ModuleType = x })
                    .ToList();
            }

            return _starterModuleInfos;
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            // Noop
        }

        class StarterModuleInfo
        {
            public Type ModuleType { get; set; }
            public int Attempts { get; set; }
        }

        class StarterModule
        {
            public StarterModuleInfo Info { get; set; }
            public IPostApplicationStart Instance { get; set; }
        }
    }
}
