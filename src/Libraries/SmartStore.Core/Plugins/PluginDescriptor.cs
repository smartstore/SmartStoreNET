using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Information about an assembly referenced by a plugin.
    /// </summary>
    public sealed class AssemblyReference
    {
        /// <summary>
        /// File in ASP.NET temp folder
        /// </summary>
        public FileInfo File { get; internal set; }
        /// <summary>
        /// The original assembly file that a shadow copy was made from it
        /// </summary>
        public FileInfo OriginalFile { get; internal set; }
        /// <summary>
        /// The assembly that has been shadow copied and was loaded into the AppDomain
        /// </summary>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Exception thrown during activation/probing
        /// </summary>
        internal Exception ActivationException { get; set; }
    }

    [DataContract]
    public class PluginDescriptor : IComparable<PluginDescriptor>
    {
        private string _resourceRootKey;
        private string _brandImageFileName;

        public PluginDescriptor()
        {
            Version = new Version("1.0");
            MinAppVersion = SmartStoreVersion.Version;
        }

        // Unit tests
        public PluginDescriptor(Assembly referencedAssembly, FileInfo originalAssemblyFile, Type pluginClrType)
            : this()
        {
            Assembly = new AssemblyReference { Assembly = referencedAssembly, OriginalFile = originalAssemblyFile, File = originalAssemblyFile };
            PluginClrType = pluginClrType;
        }

        /// <summary>
        /// Plugin file name
        /// </summary>
        public string PluginFileName { get; set; }

        /// <summary>
        /// The physical path of the runtime plugin
        /// </summary>
		public string PhysicalPath { get; set; }

        /// <summary>
        /// Gets the folder name
        /// </summary>
        [DataMember]
        public string FolderName { get; internal set; }

        /// <summary>
        /// The virtual path of the runtime plugin
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Gets the file name of the brand image (without path)
        /// or an empty string if no image is specified
        /// </summary>
        public string BrandImageFileName
        {
            get
            {
                if (_brandImageFileName == null)
                {
                    // "null" means we haven't checked yet!
                    var filesToCheck = new[] { "branding.png", "branding.gif", "branding.jpg", "branding.jpeg" };
                    var dir = this.PhysicalPath;
                    foreach (var file in filesToCheck)
                    {
                        if (File.Exists(Path.Combine(dir, "Content", file)))
                        {
                            _brandImageFileName = file;
                            break;
                        }
                    }

                    // indicate that we have checked already (although no file was found)
                    if (_brandImageFileName == null)
                        _brandImageFileName = String.Empty;
                }

                return _brandImageFileName;
            }
        }

        /// <summary>
        /// Plugin runtime type
        /// </summary>
        public Type PluginClrType { get; set; }

        /// <summary>
        /// The plugin assembly.
        /// </summary>
        public AssemblyReference Assembly { get; internal set; } = new AssemblyReference();

        /// <summary>
        /// List of assemblies files found in the plugin folder, except the main plugin assembly.
        /// </summary>
        internal AssemblyReference[] ReferencedLocalAssemblies { get; set; } = new AssemblyReference[0];

        /// <summary>
        /// Gets or sets the plugin group
        /// </summary>
        [DataMember]
        public string Group { get; internal set; }

        public bool IsInKnownGroup => PluginFileParser.KnownGroups.Contains(this.Group, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
		[DataMember]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the system name
        /// </summary>
		[DataMember]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets the plugin description
        /// </summary>
		[DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
		[DataMember]
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the minimum supported app version
        /// </summary>
		[DataMember]
        public Version MinAppVersion { get; set; }

        /// <summary>
        /// Gets or sets the author
        /// </summary>
		[DataMember]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the project/marketplace url
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is installed
        /// </summary>
		[DataMember]
        public bool Installed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the plugin is incompatible with the current application version
        /// </summary>
        [DataMember]
        public bool Incompatible { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the plugin is configurable
        /// </summary>
        /// <remarks>
        /// A plugin is configurable when it implements the <see cref="IConfigurable"/> interface
        /// </remarks>
        [DataMember]
        public bool IsConfigurable { get; set; }

        /// <summary>
        /// Gets or sets the root key of string resources.
        /// </summary>
        /// <remarks>
        /// Tries to get it from first entry of resource XML file if not specified.
        /// In that case the first resource name should not contain a dot if it's not part of the root key.
        /// Otherwise you get the wrong root key.
        /// </remarks>
        public string ResourceRootKey
        {
            get
            {
                if (_resourceRootKey == null)
                {
                    _resourceRootKey = "";

                    try
                    {
                        // Try to get root-key from first entry of XML file
                        var localizationDir = new DirectoryInfo(Path.Combine(PhysicalPath, "Localization"));

                        if (localizationDir.Exists)
                        {
                            var localizationFile = localizationDir.EnumerateFiles("*.xml").FirstOrDefault();
                            if (localizationFile != null)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load(localizationFile.FullName);
                                var key = doc.SelectSingleNode(@"//Language/LocaleResource")?.Attributes["Name"]?.InnerText;
                                if (key.HasValue() && key.Contains('.'))
                                {
                                    _resourceRootKey = key.Substring(0, key.LastIndexOf('.'));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Dump();
                    }
                }

                return _resourceRootKey;
            }
            set => _resourceRootKey = value;
        }

        public T Instance<T>() where T : class, IPlugin
        {
            if (!EngineContext.Current.ContainerManager.TryResolve(PluginClrType, null, out var instance))
            {
                // Not registered
                instance = EngineContext.Current.ContainerManager.ResolveUnregistered(PluginClrType);
            }

            var typedInstance = instance as T;
            if (typedInstance != null)
                typedInstance.PluginDescriptor = this;

            return typedInstance;
        }

        public IPlugin Instance()
        {
            return Instance<IPlugin>();
        }

        [SuppressMessage("ReSharper", "StringCompareToIsCultureSpecific")]
        public int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);
            else if (FriendlyName != null)
                return FriendlyName.CompareTo(other.FriendlyName);
            return 0;
        }

        public string GetSettingKey(string name)
        {
            return "PluginSetting.{0}.{1}".FormatWith(SystemName, name);
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PluginDescriptor;
            return other != null &&
                SystemName != null &&
                SystemName.Equals(other.SystemName);
        }

        public override int GetHashCode()
        {
            return SystemName.GetHashCode();
        }
    }
}
