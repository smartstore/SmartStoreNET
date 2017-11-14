// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessorBootstrapper.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
//	 Modified by Murat Cakir for SmartStore.NET
// </copyright>
// <summary>
//   The postprocessor bootstrapper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IODirectory = System.IO.Directory;
using System.Reflection;

namespace SmartStore.Services.Media
{
	internal sealed class ImagePostProcessorBootstrapper
	{
		/// <summary>
		/// The assembly version.
		/// </summary>
		private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

		/// <summary>
		/// A new instance of the <see cref="T:SmartStore.Services.Media.ImagePostProcessorBootstrapper"/> class.
		/// with lazy initialization.
		/// </summary>
		private static readonly Lazy<ImagePostProcessorBootstrapper> Lazy 
			= new Lazy<ImagePostProcessorBootstrapper>(() => new ImagePostProcessorBootstrapper());

		/// <summary>
		/// Prevents a default instance of the <see cref="ImagePostProcessorBootstrapper"/> class from being created.
		/// </summary>
		private ImagePostProcessorBootstrapper()
		{
			if (!Lazy.IsValueCreated)
			{
				this.RegisterExecutables();
			}
		}

		/// <summary>
		/// Gets the current instance of the <see cref="ImagePostProcessorBootstrapper"/> class.
		/// </summary>
		public static ImagePostProcessorBootstrapper Instance => Lazy.Value;

		/// <summary>
		/// Gets the working directory path.
		/// </summary>
		public string WorkingPath { get; private set; }

		/// <summary>
		/// Gets or a value indicating whether the post processor has been installed.
		/// </summary>
		public bool IsInstalled { get; private set; }

		/// <summary>
		/// Gets the allowed time in milliseconds for postprocessing an image file.
		/// </summary>
		public int Timout { get; internal set; } = 5000;

		/// <summary>
		/// Registers the embedded executables.
		/// </summary>
		public void RegisterExecutables()
		{
			// None of the tools used here are called using dllimport so we don't go through the normal registration channel.
			string folder = Environment.Is64BitProcess ? "x64" : "x86";
			Assembly assembly = Assembly.GetExecutingAssembly();

			if (assembly.Location == null)
			{
				Debug.WriteLine("Unable to install postprocessor - No images will be post-processed. Unable to map location for assembly.");
				return;
			}

			this.WorkingPath = Path.GetFullPath(
				Path.Combine(new Uri(assembly.Location).LocalPath, 
				"..\\smartstore.imageprocessors" + AssemblyVersion + "\\"));

			string path = Path.GetDirectoryName(this.WorkingPath);

			if (string.IsNullOrWhiteSpace(path))
			{
				Debug.WriteLine("Unable to install postprocessor - No images will be post-processed. Unable to map working path for processors.");
				return;
			}

			// Create the folder for storing the executables.
			// Delete any previous instances to make sure we copy over the new files.
			try
			{
				if (IODirectory.Exists(path))
				{
					IODirectory.Delete(path, true);
				}

				IODirectory.CreateDirectory(path);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}, {ex.StackTrace}. Inner: {ex.InnerException?.Message}, {ex.InnerException?.StackTrace}");
				Debug.WriteLine("Unable to install postprocessor - No images will be post-processed. Unable to map working path for processors.");

				return;
			}

			// Get the resources and copy them across.
			Dictionary<string, string> resources = new Dictionary<string, string>
			{
				{ "gifsicle.exe", "SmartStore.Services.Media.Resources.Native." + folder + ".gifsicle.exe" },
				{ "jpegtran.exe", "SmartStore.Services.Media.Resources.Native.x86.jpegtran.exe" },
				{ "cjpeg.exe", "SmartStore.Services.Media.Resources.Native.x86.cjpeg.exe" },
				{ "libjpeg-62.dll", "SmartStore.Services.Media.Resources.Native.x86.libjpeg-62.dll" },
				{ "pngquant.exe", "SmartStore.Services.Media.Resources.Native.x86.pngquant.exe" },
				{ "pngout.exe", "SmartStore.Services.Media.Resources.Native.x86.pngout.exe" },
				{ "TruePNG.exe", "SmartStore.Services.Media.Resources.Native.x86.TruePNG.exe" },
				{ "png.cmd", "SmartStore.Services.Media.Resources.Native.x86.png.cmd" }
			};

			// Write the files out to the bin folder.
			foreach (var resource in resources)
			{
				using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource.Value))
				{
					if (resourceStream != null)
					{
						using (FileStream fileStream = File.OpenWrite(Path.Combine(this.WorkingPath, resource.Key)))
						{
							resourceStream.CopyTo(fileStream);
						}
					}
				}
			}

			this.IsInstalled = true;
		}
	}
}