
using System.Data.Common;

namespace SmartStore.Core.Data
{
    /// <summary>
    /// Data provider interface
    /// </summary>
    public interface IDataProvider
    {

        /// <summary>
        /// A value indicating whether this data provider supports stored procedures
        /// </summary>
        bool StoredProceduresSupported { get; }

        /// <summary>
        /// Gets a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        DbParameter GetParameter();

		/// <summary>
		/// Gets the db provider invariant name (e.g. <c>System.Data.SqlClient</c>)
		/// </summary>
		string ProviderInvariantName { get; }
    }
}
