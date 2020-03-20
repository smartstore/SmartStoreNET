using System;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// A service interface responsible for resizing/processing images.
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// Determines whether the given file name is processable by the image resizer
        /// </summary>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>A value indicating whether processing is possible</returns>
        bool IsSupportedImage(string extension);

        /// <summary>
        /// Processes an image
        /// </summary>
        /// <param name="request">Resize request</param>
        /// <param name="disposeOutput">Whether to dispose the output stream when <see cref="ProcessImageResult"/> instance gets disposed.</param>
        /// <returns>The resizing result encapsulated in <see cref="ProcessImageResult"/> type</returns>
        ProcessImageResult ProcessImage(ProcessImageQuery query, bool disposeOutput = true);

		/// <summary>
		/// Gets the cumulative total processing time since app start in miliseconds
		/// </summary>
		long TotalProcessingTimeMs { get; }
	}
}
