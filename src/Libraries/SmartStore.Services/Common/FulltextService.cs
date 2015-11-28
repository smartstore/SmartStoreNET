using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Data;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Full-Text service
    /// </summary>
    public partial class FulltextService : IFulltextService
    {
        #region Fields
 
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly CommonSettings _commonSettings;
        #endregion
 
        #region Ctor
 
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="commonSettings">Common settings</param>
        public FulltextService(IDataProvider dataProvider, IDbContext dbContext,
            CommonSettings commonSettings)
        {
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._commonSettings = commonSettings;
        }
 
        #endregion
 
        #region Methods
 
        /// <summary>
        /// Gets value indicating whether Full-Text is supported
        /// </summary>
        /// <returns>Result</returns>
        public virtual bool IsFullTextSupported()
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduresSupported)
            {
                //stored procedures are enabled and supported by the database. 
				try
				{
					var result = _dbContext.SqlQuery<int>("EXEC [FullText_IsSupported]");
					return result.Any() && result.FirstOrDefault() > 0;
				}
				catch
				{
					return false;
				}
            }
            else
            {
                //stored procedures aren't supported
                return false;
            }
        }
 
        /// <summary>
        /// Enable Full-Text support
        /// </summary>
        public virtual void EnableFullText()
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduresSupported)
            {
                //stored procedures are enabled and supported by the database.
                _dbContext.ExecuteSqlCommand("EXEC [FullText_Enable]", true);
            }
            else
            {
                throw new Exception("Stored procedures are not supported by your database");
            }
        }
 
        /// <summary>
        /// Disable Full-Text support
        /// </summary>
        public virtual void DisableFullText()
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduresSupported)
            {
                //stored procedures are enabled and supported by the database.
                _dbContext.ExecuteSqlCommand("EXEC [FullText_Disable]", true);
            }
            else
            {
                throw new Exception("Stored procedures are not supported by your database");
            }
        }
 
        #endregion
    }
}