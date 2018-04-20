using System;
using System.Collections.Generic;

namespace SmartStore.Templating
{
	/// <summary>
	/// Responsible for managing all compiled templates
	/// </summary>
	public interface ITemplateManager
	{
		/// <summary>
		/// Gets all compiled templates
		/// </summary>
		/// <returns></returns>
		IReadOnlyDictionary<string, ITemplate> All();

		/// <summary>
		/// Checks whether a compiled template exists in the storage
		/// </summary>
		/// <param name="name">Template name</param>
		/// <returns><c>true</c> when the template exists</returns>
		bool Contains(string name);

		/// <summary>
		/// Gets a compiled template matching the passed name
		/// </summary>
		/// <param name="name">Template name to get.</param>
		/// <returns>An instance of <see cref="ITemplate"/> or <c>null</c></returns>
		ITemplate Get(string name);

		/// <summary>
		/// Saves a compiled template in the storage. Any existing template gets overridden.
		/// </summary>
		/// <param name="name">The name to use as template key</param>
		/// <param name="template">The compiled template</param>
		void Put(string name, ITemplate template);

		/// <summary>
		/// Either gets a template with the passed name, or compiles and stores
		/// a template if it does not exist yet.
		/// </summary>
		/// <param name="name">Template name</param>
		/// <param name="sourceFactory">The factory used to generate/obtain the template source.</param>
		/// <returns>The compiled template</returns>
		ITemplate GetOrAdd(string name, Func<string> sourceFactory);

		/// <summary>
		/// Attempts to remove and return the compiled template that has the specified name.
		/// </summary>
		/// <param name="name">The name of the template to remove and return.</param>
		/// <param name="template">The removed template or <c>null</c></param>
		/// <returns>true if the template was removed, false otherwise.</returns>
		bool TryRemove(string name, out ITemplate template);

		/// <summary>
		/// Removes all compiled templates from the storage.
		/// </summary>
		void Clear();
	}
}
