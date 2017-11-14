// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostProcessor.cs" company="James Jackson-South">
//   Copyright (c) James Jackson-South.
//   Licensed under the Apache License, Version 2.0.
//	 Modified by Murat Cakir for SmartStore.NET
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Web;
using SmartStore.Core.Logging;
using IODirectory = System.IO.Directory;

namespace SmartStore.Services.Media
{
	internal static class ImagePostProcessor
	{
		private static bool IsProcessableFormat(string extension)
		{
			switch (extension)
			{
				case "jpg":
				case "jpeg":
				case "png":
				case "gif":
					return true;
			}

			return false;
		}
		
		/// <summary>
		/// Post processes the image.
		/// </summary>
		/// <param name="stream">The source image stream.</param>
		/// <param name="extension">The image extension.</param>
		/// <returns>
		/// The <see cref="MemoryStream"/>.
		/// </returns>
		public static MemoryStream PostProcessImage(MemoryStream stream, string fileName, string extension, ILogger logger)
		{
			if (!ImagePostProcessorBootstrapper.Instance.IsInstalled || !IsProcessableFormat(extension))
			{
				return stream;
			}

			// Create a temporary source file with the correct extension.
			long length = stream.Length;

			string tempSourceFile = Path.GetTempFileName();
			string sourceFile = Path.ChangeExtension(tempSourceFile, extension);
			File.Move(tempSourceFile, sourceFile);

			// Give our destination file a unique name.
			string destinationFile = sourceFile.Replace(extension, "-out." + extension);

			// Save the input stream to our source temp file for post processing.
			using (FileStream fileStream = File.Create(sourceFile))
			{
				stream.CopyTo(fileStream);
			}

			var result = RunProcess(fileName, sourceFile, destinationFile, length, logger);

			// If our result is good and a saving is made we replace our original stream contents with our new compressed file.
			if (result != null && result.ResultFileSize > 0 && result.Saving > 0)
			{
				using (FileStream fileStream = File.OpenRead(destinationFile))
				{
					stream.SetLength(0);
					fileStream.CopyTo(stream);
				}
			}

			// Cleanup the temp files.
			try
			{
				// Ensure files exist, are not read only, and delete
				if (File.Exists(sourceFile))
				{
					File.SetAttributes(sourceFile, FileAttributes.Normal);
					File.Delete(sourceFile);
				}

				if (File.Exists(destinationFile))
				{
					File.SetAttributes(destinationFile, FileAttributes.Normal);
					File.Delete(destinationFile);
				}
			}
			catch
			{
				// Normally a No no, but logging would be excessive + temp files get cleaned up eventually.
			}

			stream.Position = 0;
			return stream;
		}

		/// <summary>
		/// Runs the process to optimize the images.
		/// </summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destinationFile">The destination file.</param>
		/// <param name="length">The source file length in bytes.</param>
		/// <returns>
		/// The <see cref="ImagePostProcessingResult"/> containing post-processing information.
		/// </returns>
		private static ImagePostProcessingResult RunProcess(string fileName, string sourceFile, string destinationFile, long length, ILogger logger)
		{
			// Create a new, hidden process to run our postprocessor command.
			// We allow no more than the set timeout (default 5 seconds) for the process to run before killing it to prevent blocking the app.
			int timeout = ImagePostProcessorBootstrapper.Instance.Timout;
			ImagePostProcessingResult result = null;
			string arguments = GetArguments(sourceFile, destinationFile, length);

			if (string.IsNullOrWhiteSpace(arguments))
			{
				// Not a file we can post process.
				return null;
			}

			ProcessStartInfo start = new ProcessStartInfo("cmd")
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = ImagePostProcessorBootstrapper.Instance.WorkingPath,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process process = null;
			try
			{
				process = new Process
				{
					StartInfo = start,
					EnableRaisingEvents = true
				};

				// Process has completed successfully within the time limit.
				process.Exited += (sender, args) =>
				{
					result = new ImagePostProcessingResult(destinationFile, length);
				};

				process.Start();

				// Wait for processing to finish, but not more than our timeout.
				if (!process.WaitForExit(timeout))
				{
					process.Kill();
					logger.Warn($"Unable to post process image '{fileName ?? sourceFile}' within {timeout}ms. Original image returned.");
				}
			}
			catch (Exception ex)
			{
				// Some security policies don't allow execution of programs in this way
				logger.Error(ex);
			}
			finally
			{
				// Make sure we always dispose and release
				process?.Dispose();
			}

			return result;
		}

		/// <summary>
		/// Gets the correct arguments to pass to the post-processor.
		/// </summary>
		/// <param name="sourceFile">The source file.</param>
		/// <param name="destinationFile">The source file.</param>
		/// <param name="length">The source file length in bytes.</param>
		/// <returns>
		/// The <see cref="string"/> containing the correct command arguments.
		/// </returns>
		private static string GetArguments(string sourceFile, string destinationFile, long length)
		{
			if (!Uri.IsWellFormedUriString(sourceFile, UriKind.RelativeOrAbsolute) && !File.Exists(sourceFile))
			{
				return null;
			}

			string ext;

			string extension = Path.GetExtension(sourceFile);
			if (extension != null)
			{
				ext = extension.ToLowerInvariant();
			}
			else
			{
				return null;
			}

			switch (ext)
			{
				case ".png":
					return string.Format(CultureInfo.CurrentCulture, "/c png.cmd \"{0}\" \"{1}\"", sourceFile, destinationFile);

				case ".jpg":
				case ".jpeg":

					// If it's greater than 10Kb use progressive
					// http://yuiblog.com/blog/2008/12/05/imageopt-4/
					if (length > 10000)
					{
						return string.Format(CultureInfo.CurrentCulture, "/c cjpeg -quality 80,60 -smooth 5 -outfile \"{1}\" \"{0}\"", sourceFile, destinationFile);
					}

					return string.Format(CultureInfo.CurrentCulture, "/c jpegtran -copy all -optimize -outfile \"{1}\" \"{0}\"", sourceFile, destinationFile);

				case ".gif":
					return string.Format(CultureInfo.CurrentCulture, "/c gifsicle --optimize=3 \"{0}\" --output=\"{1}\"", sourceFile, destinationFile);
			}

			return null;
		}
	}

	internal class ImagePostProcessingResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImagePostProcessingResult"/> class.
		/// </summary>
		/// <param name="resultFileName">The original file name.</param>
		/// <param name="length">The original file length in bytes.</param>
		public ImagePostProcessingResult(string resultFileName, long length)
		{
			FileInfo result = new FileInfo(resultFileName);
			this.OriginalFileSize = length;
			if (result.Exists)
			{
				this.ResultFileName = result.FullName;
				this.ResultFileSize = result.Length;
			}
		}

		/// <summary>
		/// Gets or sets the original file size in bytes.
		/// </summary>
		public long OriginalFileSize { get; set; }

		/// <summary>
		/// Gets or sets the result file size in bytes.
		/// </summary>
		public long ResultFileSize { get; set; }

		/// <summary>
		/// Gets or sets the result file name.
		/// </summary>
		public string ResultFileName { get; set; }

		/// <summary>
		/// Gets the difference in file size in bytes.
		/// </summary>
		public long Saving => this.OriginalFileSize - this.ResultFileSize;

		/// <summary>
		/// Gets the difference in file size as a percentage.
		/// </summary>
		public double Percent => Math.Round(100 - ((this.ResultFileSize / (double)this.OriginalFileSize) * 100), 1);

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Optimized " + Path.GetFileName(this.ResultFileName));
			stringBuilder.AppendLine("Before: " + this.OriginalFileSize + " bytes");
			stringBuilder.AppendLine("After: " + this.ResultFileSize + " bytes");
			stringBuilder.AppendLine("Saving: " + this.Saving + " bytes / " + this.Percent + "%");

			return stringBuilder.ToString();
		}
	}
}
