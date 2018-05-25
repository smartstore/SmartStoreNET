using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;
using SmartStore.Collections;

namespace SmartStore.Core.Themes
{   
    internal class ThemeManifestMaterializer
    {
        private readonly ThemeManifest _manifest;

        public ThemeManifestMaterializer(ThemeFolderData folderData)
        {
			Guard.NotNull(folderData, nameof(folderData));

            _manifest = new ThemeManifest();

			_manifest.ThemeName = folderData.FolderName;
			_manifest.BaseThemeName = folderData.BaseTheme;
            _manifest.Location = folderData.VirtualBasePath;
            _manifest.Path = folderData.FullPath;
			_manifest.IsSymbolicLink = folderData.IsSymbolicLink;
            _manifest.ConfigurationNode = folderData.Configuration.DocumentElement;
        }
        
        public ThemeManifest Materialize()
        {
            var root = _manifest.ConfigurationNode;

			_manifest.ThemeTitle = root.GetAttribute("title").NullEmpty() ?? _manifest.ThemeName;
            _manifest.PreviewImageUrl = root.GetAttribute("previewImageUrl").NullEmpty() ?? "~/Themes/{0}/preview.png".FormatCurrent(_manifest.ThemeName);
            _manifest.PreviewText = root.GetAttribute("previewText").ToSafe();
            _manifest.Author = root.GetAttribute("author").ToSafe();
			_manifest.Url = root.GetAttribute("url").ToSafe();
            _manifest.Version = root.GetAttribute("version").ToSafe().HasValue() ? root.GetAttribute("version") : "1.0";

            _manifest.Selects = MaterializeSelects();
            _manifest.Variables = MaterializeVariables();

            return _manifest;
        }

	    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
	    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	    private Multimap<string, string> MaterializeSelects()
        {
            var selects = new Multimap<string, string>();
            var root = _manifest.ConfigurationNode;
            var xndSelects = root.SelectNodes(@"Selects/Select").Cast<XmlElement>();

            foreach (var xel in xndSelects)
            {
                string id = xel.GetAttribute("id").ToSafe();
                if (id.IsEmpty() || selects.ContainsKey(id))
                {
                    throw new SmartException("A 'Select' element must contain a unique id. Affected: '{0}' - element: {1}", _manifest.FullPath, xel.OuterXml);
                }

                var xndOptions = xel.SelectNodes(@"Option").Cast<XmlElement>();
                if (!xndOptions.Any())
                {
                    throw new SmartException("A 'Select' element must contain at least one 'Option' child element. Affected: '{0}' - element: {1}", _manifest.FullPath, xel.OuterXml);
                }

                foreach (var xelOption in xndOptions)
                {
                    string option = xelOption.InnerText;
                    if (option.IsEmpty())
                    {
                        throw new SmartException("A select option cannot be empty. Affected: '{0}' - element: {1}", _manifest.FullPath, xel.OuterXml);
                    }

                    selects.Add(id, option);
                }

            }

            return selects;
        }

        private IDictionary<string, ThemeVariableInfo> MaterializeVariables()
        {
            var vars = new Dictionary<string, ThemeVariableInfo>(StringComparer.OrdinalIgnoreCase);
            var root = _manifest.ConfigurationNode;
            var xndVars = root.SelectNodes(@"Vars/Var").Cast<XmlElement>();

            foreach (var xel in xndVars)
            {
                var info = MaterializeVariable(xel);
                if (info != null && info.Name.HasValue())
                {
                    if (vars.ContainsKey(info.Name))
                    {
                        throw new SmartException("Duplicate variable name '{0}' in '{1}'. Variable names must be unique.", info.Name, _manifest.FullPath);
                    }
                    vars.Add(info.Name, info);
                }
            }

            return vars;
        }

        private ThemeVariableInfo MaterializeVariable(XmlElement xel)
        {
            string name = xel.GetAttribute("name");
            string value = xel.InnerText;

            if (name.IsEmpty())
            {
                throw new SmartException("The name attribute is required for the 'Var' element. Affected: '{0}' - element: {1}", _manifest.FullPath, xel.OuterXml);
            }

            string type = xel.GetAttribute("type").ToSafe("String");

            string selectRef = null;
            var varType = ConvertVarType(type, xel, out selectRef);

			if (varType != ThemeVariableType.String && value.IsEmpty())
			{
				throw new SmartException("A value is required for non-string 'Var' elements. Affected: '{0}' - element: {1}", _manifest.FullPath, xel.OuterXml);
			}

			var info = new ThemeVariableInfo
            {
                Name = name,
                DefaultValue = value,
                Type = varType,
                SelectRef = selectRef,
                Manifest = _manifest
            };

            return info;
        }

        private ThemeVariableType ConvertVarType(string type, XmlElement affected, out string selectRef)
        {
            ThemeVariableType result = ThemeVariableType.String;
            selectRef = null;

            if (type.ToLower().StartsWith("select", StringComparison.CurrentCultureIgnoreCase))
            {
                var arr = type.Split(new char[] { '#' });
                if (arr.Length < 1 || arr[1].IsEmpty())
                {
                    throw new SmartException("The 'id' of a select element must be provided (pattern: Select#MySelect). Affected: '{0}' - element: {1}", _manifest.FullPath, affected.OuterXml);
                }

                selectRef = arr[1];
                return ThemeVariableType.Select;
            }

            switch (type.ToLowerInvariant())
            {
                case "string":
                    result = ThemeVariableType.String;
                    break;
                case "color":
                    result = ThemeVariableType.Color;
                    break;
                case "boolean":
                    result = ThemeVariableType.Boolean;
                    break;
                case "number":
                    result = ThemeVariableType.Number;
                    break;
            }

            return result;
        }
    }
}
