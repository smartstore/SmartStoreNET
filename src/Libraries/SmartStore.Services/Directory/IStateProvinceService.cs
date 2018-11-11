using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// State province service interface
    /// </summary>
    public partial interface IStateProvinceService
    {
        /// <summary>
        /// Deletes a state/province
        /// </summary>
        /// <param name="stateProvince">The state/province</param>
        void DeleteStateProvince(StateProvince stateProvince);

		/// <summary>
		/// Get all states/provinces
		/// </summary>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns></returns>
		IQueryable<StateProvince> GetAllStateProvinces(bool showHidden = false);

		/// <summary>
		/// Gets a state/province
		/// </summary>
		/// <param name="stateProvinceId">The state/province identifier</param>
		/// <returns>State/province</returns>
		StateProvince GetStateProvinceById(int stateProvinceId);

        /// <summary>
        /// Gets a state/province 
        /// </summary>
        /// <param name="abbreviation">The state/province abbreviation</param>
        /// <returns>State/province</returns>
        StateProvince GetStateProvinceByAbbreviation(string abbreviation);
        
        /// <summary>
        /// Gets a state/province collection by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>State/province collection</returns>
        IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool showHidden = false);

        /// <summary>
        /// Inserts a state/province
        /// </summary>
        /// <param name="stateProvince">State/province</param>
        void InsertStateProvince(StateProvince stateProvince);

        /// <summary>
        /// Updates a state/province
        /// </summary>
        /// <param name="stateProvince">State/province</param>
        void UpdateStateProvince(StateProvince stateProvince);
    }
}
