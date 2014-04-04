using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Utilities;

namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Plugin files parser
    /// </summary>
    public static class PluginFileParser
    {
        internal class GroupComparer : Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                return Array.FindIndex( KnownGroups, s => s == x) - Array.FindIndex(KnownGroups, s => s == y);
            }
        }
        
        // codehint: sm-add
        internal readonly static string[] KnownGroups = new string[] 
        { 
            "Admin", // NEW
            "Marketing", // NEW
            "Payment",
            "Shipping",
            "Tax",
            "Analytics", // NEW
            "CMS", // NEW
            "SEO", // NEW
            "PromotionFeed",
            "DiscountRequirement",
            "Import",
            "CurrencyExchange",
            "ExternalAuth",
            "Api", // NEW
            "Mobile",
            "Social", // NEW
            "Security", // NEW
            "Widget",
            "Developer", // NEW
            "Misc"
        };
        public readonly static IComparer<string> KnownGroupComparer = new GroupComparer();


        public static IList<string> ParseInstalledPluginsFile(string filePath)
        {
            //read and parse the file
            if (!File.Exists(filePath))
                return new List<string>();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return new List<string>();
            
            //Old way of file reading. This leads to unexpected behavior when a user's FTP program transfers these files as ASCII (\r\n becomes \n).
            //var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            var lines = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (str.IsEmpty() || lines.Contains(str, StringComparer.CurrentCultureIgnoreCase))
                        continue;
                    lines.Add(str.Trim());
                }
            }
            return lines;
        }

        public static void SaveInstalledPluginsFile(IList<String> pluginSystemNames, string filePath)
        {
            if (pluginSystemNames == null || pluginSystemNames.Count == 0)
                return;

            string result = "";
            foreach (var sn in pluginSystemNames)
                result += string.Format("{0}{1}", sn, Environment.NewLine);

            File.WriteAllText(filePath, result);
        }

        public static PluginDescriptor ParsePluginDescriptionFile(string filePath)
        {
            var descriptor = new PluginDescriptor();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return descriptor;

			descriptor.FolderName = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    settings.Add(str.Trim());
                }
            }

            //Old way of file reading. This leads to unexpected behavior when a user's FTP program transfers these files as ASCII (\r\n becomes \n).
            //var settings = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            string group = null;

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(':');
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                group = null;

                switch (key)
                {
                    case "Group":
                        group = value; // descriptor.Group = value;
                        break;
                    case "FriendlyName":
                        descriptor.FriendlyName = value;
                        break;
                    case "SystemName":
                        descriptor.SystemName = value;
                        break;
                    case "Description":
                        descriptor.Description = value;
                        break;
                    case "Version":
                        descriptor.Version = value.ToVersion();
                        break;
                    case "SupportedVersions": // compat
                    case "MinAppVersion":
                        {
                            //parse supported min app version
                            descriptor.MinAppVersion = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .FirstOrDefault() // since V1.2 take the first only
                                .ToVersion();
                        }
                        break;
                    case "Author":
                        descriptor.Author = value;
                        break;
                    case "DisplayOrder":
                        {
                            int displayOrder;
                            int.TryParse(value, out displayOrder);
                            descriptor.DisplayOrder = displayOrder;
                        }
                        break;
                    case "FileName":
                        descriptor.PluginFileName = value;
                        break;
					case "ResourceRootKey":
						descriptor.ResourceRootKey = value;		// codehint: sm-add
						break;
                }
            }

            // codehint: sm-add
            descriptor.Group = GetKnownGroupName(descriptor.SystemName);
            if (descriptor.Group == "Misc" && group.HasValue())
            {
                descriptor.Group = group;
            }

            return descriptor;
        }
        
        // codehint: sm-add
        private static string GetKnownGroupName(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return "Misc";
            }

            string token = systemName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

            int idx = Array.FindIndex(
                KnownGroups,
                x => x.Equals(token, StringComparison.OrdinalIgnoreCase) || x.Equals(Inflector.Singularize(token), StringComparison.OrdinalIgnoreCase));

            if (idx >= 0)
            {
                return KnownGroups[idx];
            }

            return "Misc";
        }
        
        public static void SavePluginDescriptionFile(PluginDescriptor plugin)
        {
            if (plugin == null)
                throw new ArgumentException("plugin");

            //get the Description.txt file path
            if (plugin.OriginalAssemblyFile == null)
                throw new Exception(string.Format("Cannot load original assembly path for {0} plugin.", plugin.SystemName));
            var filePath = Path.Combine(plugin.OriginalAssemblyFile.Directory.FullName, "Description.txt");
            if (!File.Exists(filePath))
                throw new Exception(string.Format("Description file for {0} plugin does not exist. {1}", plugin.SystemName, filePath));

            var keyValues = new List<KeyValuePair<string, string>>();
            if (!plugin.IsInKnownGroup)
            {
                keyValues.Add(new KeyValuePair<string, string>("Group", plugin.Group));
            }
            keyValues.Add(new KeyValuePair<string, string>("FriendlyName", plugin.FriendlyName));
            keyValues.Add(new KeyValuePair<string, string>("SystemName", plugin.SystemName));
            keyValues.Add(new KeyValuePair<string, string>("Description", plugin.Description));
            keyValues.Add(new KeyValuePair<string, string>("Version", plugin.Version.ToString()));
            keyValues.Add(new KeyValuePair<string, string>("MinAppVersion", string.Join(",", plugin.MinAppVersion)));
            keyValues.Add(new KeyValuePair<string, string>("Author", plugin.Author));
            keyValues.Add(new KeyValuePair<string, string>("DisplayOrder", plugin.DisplayOrder.ToString()));
            keyValues.Add(new KeyValuePair<string, string>("FileName", plugin.PluginFileName));
			keyValues.Add(new KeyValuePair<string, string>("ResourceRootKey", plugin.ResourceRootKey));

            var sb = new StringBuilder();
            for (int i = 0; i < keyValues.Count; i++)
            {
                var key = keyValues[i].Key;
                var value = keyValues[i].Value;
                sb.AppendFormat("{0}: {1}", key, value);
                if (i != keyValues.Count -1)
                    sb.Append(Environment.NewLine);
            }
            //save the file
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
