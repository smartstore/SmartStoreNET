using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.SessionState;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Services.Configuration;
using SmartStore.Services.Hooks;
using SmartStore.Services.Media;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Infrastructure.Installation;
using SmartStore.Web.Models.Install;

namespace SmartStore.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class InstallController : Controller
    {
        private readonly IInstallationLocalizationService _locService;
        private readonly IAsyncState _asyncState;

        public InstallController(
            IInstallationLocalizationService locService,
            IAsyncState asyncState)
        {
            _locService = locService;
            _asyncState = asyncState;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger
        {
            get;
            set;
        }

        private InstallationResult GetInstallResult()
        {
            var result = _asyncState.Get<InstallationResult>();
            if (result == null)
            {
                result = new InstallationResult();
                _asyncState.Set<InstallationResult>(result);
            }
            return result;
        }

        private InstallationResult UpdateResult(Action<InstallationResult> fn)
        {
            var result = GetInstallResult();
            fn(result);
            _asyncState.Set<InstallationResult>(result);

            return result;
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        [NonAction]
        protected bool SqlServerDatabaseExists(string connectionString)
        {
            try
            {
                // just try to connect
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a database on the server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="collation">Server collation; the default one will be used if not specified</param>
        /// <returns>Error</returns>
        [NonAction]
        protected string CreateDatabase(string connectionString, string collation)
        {
            try
            {
                //parse database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                //now create connection string to 'master' dabatase. It always exists.
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}]", databaseName);
                if (!String.IsNullOrWhiteSpace(collation))
                    query = string.Format("{0} COLLATE {1}", query, collation);
                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Format(_locService.GetResource("DatabaseCreationError"), ex.Message);
            }
        }

        /// <summary>
        /// Create contents of connection strings used by the SqlConnection class
        /// </summary>
        /// <param name="trustedConnection">Avalue that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of SQL Server to connect to</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to SQL Server</param>
        /// <param name="password">The password for the SQL Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        [NonAction]
        protected string CreateConnectionString(
            bool trustedConnection,
            string serverName, string databaseName,
            string userName, string password, int timeout = 15)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;
            //builder.MultipleActiveResultSets = true;

            builder.UserInstance = false;
            builder.Pooling = true;
            builder.MinPoolSize = 1;
            builder.MaxPoolSize = 100;
            builder.Enlist = false;

            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }
            return builder.ConnectionString;
        }

        public ActionResult Index()
        {
            if (DataSettings.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            // set page timeout to 10 minutes
            this.Server.ScriptTimeout = 600;

            var model = new InstallModel
            {
                AdminEmail = _locService.GetResource("AdminEmailValue"),
                //AdminPassword = "admin",
                //ConfirmPassword = "admin",
                InstallSampleData = false,
                DatabaseConnectionString = "",
                DataProvider = "sqlserver", // "sqlce"
                SqlAuthenticationType = "sqlauthentication",
                SqlConnectionInfo = "sqlconnectioninfo_values",
                SqlServerCreateDatabase = false,
                UseCustomCollation = false,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
            };

            var curLanguage = _locService.GetCurrentLanguage();
            var availableLanguages = _locService.GetAvailableLanguages();

            foreach (var lang in availableLanguages)
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = curLanguage.Code == lang.Code,
                });
            }

            foreach (var lang in _locService.GetAvailableAppLanguages())
            {
                model.AvailableAppLanguages.Add(new SelectListItem
                {
                    Value = lang.Culture,
                    Text = lang.Name,
                    Selected = lang.UniqueSeoCode.IsCaseInsensitiveEqual(curLanguage.Code)
                });
            }

            if (!model.AvailableAppLanguages.Any(x => x.Selected))
            {
                model.AvailableAppLanguages.FirstOrDefault(x => x.Value.IsCaseInsensitiveEqual("en")).Selected = true;
            }

            model.AvailableMediaStorages.Add(new SelectListItem { Value = "fs", Text = _locService.GetResource("MediaStorage.FS"), Selected = true });
            model.AvailableMediaStorages.Add(new SelectListItem { Value = "db", Text = _locService.GetResource("MediaStorage.DB") });

            return View(model);
        }

        [HttpPost]
        public JsonResult Progress()
        {
            return Json(GetInstallResult());
        }

        [HttpPost]
        public async Task<JsonResult> Install(InstallModel model)
        {
            var t = AsyncRunner.Run((c, ct, state) => InstallCore(c, (InstallModel)state), model);
            return Json(await t);
        }

        [NonAction]
        protected virtual InstallationResult InstallCore(ILifetimeScope scope, InstallModel model)
        {
            UpdateResult(x =>
            {
                x.ProgressMessage = _locService.GetResource("Progress.CheckingRequirements");
                x.Completed = false;
                Logger.Info(x.ProgressMessage);
            });

            if (DataSettings.DatabaseIsInstalled())
            {
                return UpdateResult(x =>
                {
                    x.Success = true;
                    x.RedirectUrl = Url.Action("Index", "Home");
                    Logger.Info("Application already installed");
                });
            }

            // set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            if (model.DatabaseConnectionString != null)
            {
                model.DatabaseConnectionString = model.DatabaseConnectionString.Trim();
            }

            // SQL Server
            if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
            {
                if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
                {
                    // raw connection string
                    if (string.IsNullOrEmpty(model.DatabaseConnectionString))
                    {
                        UpdateResult(x =>
                        {
                            x.Errors.Add(_locService.GetResource("ConnectionStringRequired"));
                            Logger.Error(x.Errors.Last());
                        });
                    }

                    try
                    {
                        // try to create connection string
                        new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                    }
                    catch (Exception ex)
                    {
                        UpdateResult(x =>
                        {
                            x.Errors.Add(_locService.GetResource("ConnectionStringWrongFormat"));
                            Logger.Error(ex, x.Errors.Last());
                        });
                    }
                }
                else
                {
                    // values
                    if (string.IsNullOrEmpty(model.SqlServerName))
                    {
                        UpdateResult(x =>
                        {
                            x.Errors.Add(_locService.GetResource("SqlServerNameRequired"));
                            Logger.Error(x.Errors.Last());
                        });
                    }

                    if (string.IsNullOrEmpty(model.SqlDatabaseName))
                    {
                        UpdateResult(x =>
                        {
                            x.Errors.Add(_locService.GetResource("DatabaseNameRequired"));
                            Logger.Error(x.Errors.Last());
                        });
                    }

                    // authentication type
                    if (model.SqlAuthenticationType.Equals("sqlauthentication", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // SQL authentication
                        if (string.IsNullOrEmpty(model.SqlServerUsername))
                        {
                            UpdateResult(x =>
                            {
                                x.Errors.Add(_locService.GetResource("SqlServerUsernameRequired"));
                                Logger.Error(x.Errors.Last());
                            });
                        }

                        if (string.IsNullOrEmpty(model.SqlServerPassword))
                        {
                            UpdateResult(x =>
                            {
                                x.Errors.Add(_locService.GetResource("SqlServerPasswordRequired"));
                                Logger.Error(x.Errors.Last());
                            });
                        }
                    }
                }
            }


            // Consider granting access rights to the resource to the ASP.NET request identity. 
            // ASP.NET has a base process identity 
            // (typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
            // and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
            // If the application is impersonating via <identity impersonate="true"/>, 
            // the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.
            var webHelper = scope.Resolve<IWebHelper>();
            // validate permissions
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite(webHelper);
            foreach (string dir in dirsToCheck)
            {
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                {
                    UpdateResult(x =>
                    {
                        x.Errors.Add(string.Format(_locService.GetResource("ConfigureDirectoryPermissions"), WindowsIdentity.GetCurrent().Name, dir));
                        Logger.Error(x.Errors.Last());
                    });
                }
            }

            var filesToCheck = FilePermissionHelper.GetFilesWrite(webHelper);
            foreach (string file in filesToCheck)
            {
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                {
                    UpdateResult(x =>
                    {
                        x.Errors.Add(string.Format(_locService.GetResource("ConfigureFilePermissions"), WindowsIdentity.GetCurrent().Name, file));
                        Logger.Error(x.Errors.Last());
                    });
                }
            }

            if (GetInstallResult().HasErrors)
            {
                return UpdateResult(x =>
                {
                    x.Completed = true;
                    x.Success = false;
                    x.RedirectUrl = null;
                    Logger.Error("Aborting installation.");
                });
            }
            else
            {
                SmartObjectContext dbContext = null;
                var shouldDeleteDbOnFailure = false;

                try
                {
                    string connectionString = null;
                    if (model.DataProvider.Equals("sqlserver", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL Server

                        if (model.SqlConnectionInfo.Equals("sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //raw connection string

                            //we know that MARS option is required when using Entity Framework
                            //let's ensure that it's specified
                            var sqlCsb = new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                            sqlCsb.MultipleActiveResultSets = true;
                            connectionString = sqlCsb.ToString();
                        }
                        else
                        {
                            // values
                            connectionString = CreateConnectionString(
                                model.SqlAuthenticationType == "windowsauthentication",
                                model.SqlServerName, model.SqlDatabaseName,
                                model.SqlServerUsername, model.SqlServerPassword);
                        }

                        if (model.SqlServerCreateDatabase)
                        {
                            if (!SqlServerDatabaseExists(connectionString))
                            {
                                //create database
                                var collation = model.UseCustomCollation ? model.Collation : "";
                                var errorCreatingDatabase = CreateDatabase(connectionString, collation);
                                if (errorCreatingDatabase.HasValue())
                                {
                                    return UpdateResult(x =>
                                    {
                                        x.Errors.Add(errorCreatingDatabase);
                                        x.Completed = true;
                                        x.Success = false;
                                        x.RedirectUrl = null;
                                        Logger.Error(errorCreatingDatabase);
                                    });
                                }
                                else
                                {
                                    // Database cannot be created sometimes. Weird! Seems to be Entity Framework issue
                                    // that's just wait 3 seconds
                                    Thread.Sleep(3000);

                                    shouldDeleteDbOnFailure = true;
                                }
                            }
                        }
                        else
                        {
                            // check whether database exists
                            if (!SqlServerDatabaseExists(connectionString))
                            {
                                return UpdateResult(x =>
                                {
                                    x.Errors.Add(_locService.GetResource("DatabaseNotExists"));
                                    x.Completed = true;
                                    x.Success = false;
                                    x.RedirectUrl = null;
                                    Logger.Error(x.Errors.Last());
                                });
                            }
                        }
                    }
                    else
                    {
                        // SQL CE
                        string databaseFileName = "SmartStore.Db.sdf";
                        string databasePath = @"|DataDirectory|\Tenants\{0}\{1}".FormatInvariant(DataSettings.Current.TenantName, databaseFileName);
                        connectionString = "Data Source=" + databasePath + "; Persist Security Info=False";

                        // drop database if exists
                        string databaseFullPath = HostingEnvironment.MapPath(DataSettings.Current.TenantPath.EnsureEndsWith("/")) + databaseFileName;
                        if (System.IO.File.Exists(databaseFullPath))
                        {
                            System.IO.File.Delete(databaseFullPath);
                        }

                        shouldDeleteDbOnFailure = true;
                    }

                    // save settings
                    var dataProvider = model.DataProvider;
                    var settings = DataSettings.Current;
                    settings.AppVersion = SmartStoreVersion.Version;
                    settings.DataProvider = dataProvider;
                    settings.DataConnectionString = connectionString;
                    settings.Save();

                    // init data provider
                    var dataProviderInstance = scope.Resolve<IEfDataProvider>();

                    // Although obsolete we have no other chance than using this here.
                    // Delegating this to DbConfiguration is not possible during installation.
#pragma warning disable 618
                    Database.DefaultConnectionFactory = dataProviderInstance.GetConnectionFactory();
#pragma warning restore 618

                    // resolve SeedData instance from primary language
                    var lazyLanguage = _locService.GetAppLanguage(model.PrimaryLanguage);
                    if (lazyLanguage == null)
                    {
                        return UpdateResult(x =>
                        {
                            x.Errors.Add(_locService.GetResource("Install.LanguageNotRegistered").FormatInvariant(model.PrimaryLanguage));
                            x.Completed = true;
                            x.Success = false;
                            x.RedirectUrl = null;
                            Logger.Error(x.Errors.Last());
                        });
                    }

                    // create the DataContext
                    dbContext = new SmartObjectContext();

                    // AuditableHook must run during install
                    dbContext.DbHookHandler = new DefaultDbHookHandler(new[]
                    {
                        new Lazy<IDbSaveHook, HookMetadata>(() => new AuditableHook(), HookMetadata.Create<AuditableHook, SmartObjectContext>(typeof(IAuditable), true), false)
                    });

                    // IMPORTANT: Migration would run way too early otherwise
                    Database.SetInitializer<SmartObjectContext>(null);

                    // create Language domain object from lazyLanguage
                    var languages = dbContext.Set<Language>();
                    var primaryLanguage = languages.Create(); // create a proxied type, resources cannot be saved otherwise
                    primaryLanguage.Name = lazyLanguage.Metadata.Name;
                    primaryLanguage.LanguageCulture = lazyLanguage.Metadata.Culture;
                    primaryLanguage.UniqueSeoCode = lazyLanguage.Metadata.UniqueSeoCode;
                    primaryLanguage.FlagImageFileName = lazyLanguage.Metadata.FlagImageFileName;

                    // Build the seed configuration model
                    var seedConfiguration = new SeedDataConfiguration
                    {
                        DefaultUserName = model.AdminEmail,
                        DefaultUserPassword = model.AdminPassword,
                        SeedSampleData = model.InstallSampleData,
                        Data = lazyLanguage.Value,
                        Language = primaryLanguage,
                        StoreMediaInDB = model.MediaStorage == "db",
                        ProgressMessageCallback = msg => UpdateResult(x => x.ProgressMessage = _locService.GetResource(msg))
                    };

                    var seeder = new InstallDataSeeder(seedConfiguration, Logger);
                    Database.SetInitializer(new InstallDatabaseInitializer() { DataSeeders = new[] { seeder } });

                    UpdateResult(x =>
                    {
                        x.ProgressMessage = _locService.GetResource("Progress.BuildingDatabase");
                        Logger.Info(x.ProgressMessage);
                    });
                    // ===>>> actually performs the installation by calling "InstallDataSeeder.Seed()" internally.
                    dbContext.Database.Initialize(true);

                    // Install plugins.
                    PluginManager.MarkAllPluginsAsUninstalled();
                    var pluginFinder = scope.Resolve<IPluginFinder>();
                    var plugins = pluginFinder.GetPlugins<IPlugin>(false)
                        //.ToList()
                        .OrderBy(x => x.PluginDescriptor.Group)
                        .ThenBy(x => x.PluginDescriptor.DisplayOrder)
                        .ToList();

                    var ignoredPluginsSetting = CommonHelper.GetAppSetting<string>("sm:PluginsIgnoredDuringInstallation");
                    var pluginsIgnoredDuringInstallation = String.IsNullOrEmpty(ignoredPluginsSetting) ?
                        new List<string>() :
                        ignoredPluginsSetting
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                        .ToList();

                    if (pluginsIgnoredDuringInstallation.Count > 0)
                    {
                        plugins = plugins.Where(x => !pluginsIgnoredDuringInstallation.Contains(x.PluginDescriptor.SystemName, StringComparer.OrdinalIgnoreCase)).ToList();
                    }

                    var pluginsCount = plugins.Count;
                    var idx = 0;

                    using (var dbScope = new DbContextScope(autoDetectChanges: false, hooksEnabled: false))
                    {
                        foreach (var plugin in plugins)
                        {
                            try
                            {
                                idx++;
                                UpdateResult(x =>
                                {
                                    x.ProgressMessage = _locService.GetResource("Progress.InstallingPlugins").FormatInvariant(idx, pluginsCount);
                                    Logger.InfoFormat("Installing plugin '{0}'.", plugin.PluginDescriptor.FriendlyName ?? plugin.PluginDescriptor.SystemName);
                                });
                                plugin.Install();
                                dbScope.Commit();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);

                                if (plugin.PluginDescriptor.Installed)
                                {
                                    PluginManager.MarkPluginAsUninstalled(plugin.PluginDescriptor.SystemName);
                                }
                            }
                        }
                    }

                    // Detect media file tracks (must come after plugins installation)
                    UpdateResult(x =>
                    {
                        x.ProgressMessage = _locService.GetResource("Progress.ProcessingMedia");
                        Logger.Info(x.ProgressMessage);
                    });
                    var mediaTracker = scope.Resolve<IMediaTracker>();
                    foreach (var album in scope.Resolve<IAlbumRegistry>().GetAlbumNames(true))
                    {
                        mediaTracker.DetectAllTracks(album, true);
                    }

                    UpdateResult(x =>
                    {
                        x.ProgressMessage = _locService.GetResource("Progress.Finalizing");
                        Logger.Info(x.ProgressMessage);
                    });

                    // Do not ignore settings migrated by data seeder (e.g. default media storage provider).
                    scope.Resolve<ISettingService>().ClearCache();

                    // SUCCESS: Redirect to home page
                    return UpdateResult(x =>
                    {
                        x.Completed = true;
                        x.Success = true;
                        x.RedirectUrl = Url.Action("Index", "Home");
                        Logger.Info("Installation completed successfully");
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    // Clear provider settings if something got wrong
                    DataSettings.Delete();

                    // Delete Db if it was auto generated
                    if (dbContext != null && shouldDeleteDbOnFailure)
                    {
                        try
                        {
                            Logger.Debug("Deleting database");
                            dbContext.Database.Delete();
                        }
                        catch { }
                    }

                    var msg = ex.Message;
                    var realException = ex;
                    while (realException.InnerException != null)
                    {
                        realException = realException.InnerException;
                    }

                    if (!Object.Equals(ex, realException))
                    {
                        msg += " (" + realException.Message + ")";
                    }

                    return UpdateResult(x =>
                    {
                        x.Errors.Add(string.Format(_locService.GetResource("SetupFailed"), msg));
                        x.Success = false;
                        x.Completed = true;
                        x.RedirectUrl = null;
                    });
                }
                finally
                {
                    if (dbContext != null)
                    {
                        dbContext.Dispose();
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult Finalize(bool restart)
        {
            _asyncState.Remove<InstallationResult>();

            if (restart)
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                webHelper.RestartAppDomain();
            }

            return Json(new { Success = true });
        }

        public ActionResult ChangeLanguage(string language)
        {
            if (DataSettings.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            _locService.SaveCurrentLanguage(language);

            //Reload the page);
            return RedirectToAction("Index", "Install");
        }

        public ActionResult RestartInstall()
        {
            if (DataSettings.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            // Restart application
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            webHelper.RestartAppDomain();

            // Redirect to home page
            return RedirectToAction("Index", "Home");
        }
    }

    public class InstallationResult : ICloneable<InstallationResult>
    {
        public InstallationResult()
        {
            this.Errors = new List<string>();
        }

        public string ProgressMessage { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public string RedirectUrl { get; set; }
        public IList<string> Errors { get; private set; }
        public bool HasErrors => this.Errors.Count > 0;

        public InstallationResult Clone()
        {
            var clone = new InstallationResult
            {
                ProgressMessage = this.ProgressMessage,
                Completed = this.Completed,
                RedirectUrl = this.RedirectUrl,
                Success = this.Success
            };

            clone.Errors.AddRange(this.Errors);

            return clone;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
