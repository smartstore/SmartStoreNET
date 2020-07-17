using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Core.Plugins
{
    public class PluginLocalizationStatus
    {
        public PluginDescriptor Descriptor { get; set; }
        public string CurrentHash { get; set; }
        public string LastHash { get; set; }
        public bool Differs
        {
            get => CurrentHash != LastHash;
        }
    }
    
    public interface IPluginLocalizationDiffer
    {
        IEnumerable<PluginLocalizationStatus> GetChangedLocalizations();
        PluginLocalizationStatus GetStatus(PluginDescriptor descriptor);
        void SaveStatus(PluginLocalizationStatus state);
        void RemoveStatus(PluginLocalizationStatus state);
    }

    public class PluginLocalizationDiffer : IPluginLocalizationDiffer
    {
        private readonly IDictionary<PluginDescriptor, PluginLocalizationStatus> _cache;
        private readonly IPluginFinder _pluginFinder;

        public PluginLocalizationDiffer(IPluginFinder pluginFinder)
        {
            _cache = new Dictionary<PluginDescriptor, PluginLocalizationStatus>();
            _pluginFinder = pluginFinder;
        }

        public IEnumerable<PluginLocalizationStatus> GetChangedLocalizations()
        {
            var descriptors = _pluginFinder.GetPluginDescriptors(true);

            foreach (var d in descriptors)
            {
                var status = GetStatus(d);
                if (status != null && status.Differs)
                {
                    yield return status;
                }
            }
        }

        public PluginLocalizationStatus GetStatus(PluginDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            if (!_cache.TryGetValue(descriptor, out var state))
            {
                var path = Path.Combine(descriptor.PhysicalPath, "Localization");
                var di = new DirectoryInfo(path);

                if (di.Exists)
                {
                    var files = di.GetFiles("resources.*.xml", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        // Calculate current hash
                        var hashCombiner = new HashCodeCombiner();
                        foreach (var file in files)
                        {
                            hashCombiner.Add(file);
                        }

                        // Get last hash
                        string lastHash = null;
                        path = Path.Combine(path, ".hash");
                        if (File.Exists(path))
                        {
                            lastHash = File.ReadAllText(path, Encoding.UTF8);
                        }

                        state = new PluginLocalizationStatus
                        {
                            Descriptor = descriptor,
                            CurrentHash = hashCombiner.CombinedHashString,
                            LastHash = lastHash
                        };
                    }
                }

                _cache[descriptor] = state;
            }

            return state;
        }

        public void SaveStatus(PluginLocalizationStatus status)
        {
            Guard.NotNull(status, nameof(status));

            if (status.Differs)
            {
                var file = new FileInfo(Path.Combine(status.Descriptor.PhysicalPath, "Localization\\.hash"));
                if (file.Exists)
                {
                    file.Attributes &= ~FileAttributes.Hidden;
                }

                using (var stream = file.OpenWrite())
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(status.CurrentHash);
                }
                
                file.Attributes |= FileAttributes.Hidden;
                status.LastHash = status.CurrentHash;
            }
        }

        public void RemoveStatus(PluginLocalizationStatus status)
        {
            Guard.NotNull(status, nameof(status));

            var path = Path.Combine(status.Descriptor.PhysicalPath, "Localization\\.hash");
            if (File.Exists(path))
            {
                File.Delete(path);
            }       

            _cache.Remove(status.Descriptor);
        }
    }
}
