using System.Collections.Generic;
using System.Dynamic;

namespace SmartStore.Services.Themes
{
    public interface IThemeVariablesService
    {
        /// <summary>
        /// Gets a dynamic object which holds all runtime theme variables
        /// </summary>
        /// <param name="themeName">The theme to get variables for.</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>The dynamic variables object</returns>
        ExpandoObject GetThemeVariables(string themeName, int storeId);

        /// <summary>
        /// Saves theme variables to the database
        /// </summary>
        /// <param name="themeName">The theme for which to save variables</param>
		/// <param name="storeId">Store identifier</param>
        /// <param name="variables">The variables to save</param>
        /// <returns>The count of successfully updated or inserted variables</returns>
		int SaveThemeVariables(string themeName, int storeId, IDictionary<string, object> variables);

        /// <summary>
        /// Deletes all variables of the specified theme from the database
        /// </summary>
		/// <param name="themeName">The theme to get variables for.</param>
		/// <param name="storeId">Store identifier</param>
		void DeleteThemeVariables(string themeName, int storeId);

        /// <summary>
        /// Imports variables from xml
        /// </summary>
        /// <param name="themeName">The theme for which to import variables</param>
		/// <param name="storeId">Store identifier</param>
        /// <param name="configurationXml">The xml configuration</param>
        /// <returns>The number of successfully imported variables</returns>
		int ImportVariables(string themeName, int storeId, string configurationXml);

        /// <summary>
        /// Exports the configuration of a theme to xml
        /// </summary>
        /// <param name="themeName">The theme to export variables for</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>The configuration xml</returns>
		string ExportVariables(string themeName, int storeId);
    }
}
