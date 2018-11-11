using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace SmartStore.Core.Packaging
{
	internal class FileBasedProjectSystem : PhysicalFileSystem, IProjectSystem
	{

		public FileBasedProjectSystem(string root)
			: base(root)
		{
		}

		public void AddFrameworkReference(string name)
		{
			throw new NotSupportedException();
		}

		public void AddImport(string targetPath, ProjectImportLocation location)
		{
			throw new NotSupportedException();
		}

		public void AddReference(string referencePath, Stream stream)
		{
			throw new NotSupportedException();
		}

		public bool FileExistsInProject(string path)
		{
			return FileExists(path);
		}

		public bool IsBindingRedirectSupported
		{
			get { return false; }
		}

		public bool IsSupportedFile(string path)
		{
			return true;
		}

		public string ProjectName
		{
			get { return Root; }
		}

		protected virtual string GetReferencePath(string name)
		{
			return Path.Combine("bin", name);
		}

		public bool ReferenceExists(string name)
		{
			string path = GetReferencePath(name);
			return FileExists(path);
		}

		public void RemoveImport(string targetPath)
		{
			throw new NotSupportedException();
		}

		public void RemoveReference(string name)
		{
			throw new NotSupportedException();
		}

		public string ResolvePath(string path)
		{
			return GetFullPath(path);
		}

		public FrameworkName TargetFramework
		{
			get { return VersionUtility.DefaultTargetFramework; }
		}

		public dynamic GetPropertyValue(string propertyName)
		{
			if (propertyName == null)
			{
				return null;
			}

			// Return empty string for the root namespace of this project.
			return propertyName.Equals("RootNamespace", StringComparison.OrdinalIgnoreCase) ? String.Empty : null;
		}
	}

}
