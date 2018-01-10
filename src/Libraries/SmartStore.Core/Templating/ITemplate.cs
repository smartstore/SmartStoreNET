using System;

namespace SmartStore.Templating
{
	/// <summary>
	/// A compiled template intended to be stored in a singleton storage.
	/// </summary>
	public interface ITemplate
	{
		/// <summary>
		/// Gets the original template source
		/// </summary>
		string Source { get; }

		/// <summary>
		/// Renders the template in <see cref="Source"/>
		/// </summary>
		/// <param name="model">
		/// The model object which contains the data for the template.
		/// Can be a subclass of <see cref="IDictionary&lt;string, object&gt;"/>,
		/// a plain class object, or an anonymous type.
		/// </param>
		/// <param name="formatProvider">Provider to use for formatting numbers, dates, money etc.</param>
		/// <returns>The processed template result</returns>
		string Render(object model, IFormatProvider formatProvider);
	}
}
