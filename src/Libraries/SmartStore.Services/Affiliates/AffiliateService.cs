using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Affiliates;
using SmartStore.Core.Events;

namespace SmartStore.Services.Affiliates
{
    public partial class AffiliateService : IAffiliateService
    {
        private readonly IRepository<Affiliate> _affiliateRepository;

        public AffiliateService(IRepository<Affiliate> affiliateRepository)
        {
            _affiliateRepository = affiliateRepository;
        }

        public virtual Affiliate GetAffiliateById(int affiliateId)
        {
            if (affiliateId == 0)
                return null;

            return _affiliateRepository.GetById(affiliateId);
        }

        /// <summary>
        /// Marks affiliate as deleted 
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void DeleteAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            affiliate.Deleted = true;
            UpdateAffiliate(affiliate);
        }

        /// <summary>
        /// Gets all affiliates
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Affiliate collection</returns>
        public virtual IList<Affiliate> GetAllAffiliates(bool showHidden = false)
        {
            var query = _affiliateRepository.Table;
            if (!showHidden)
                query = query.Where(a => a.Active);
            query = query.Where(a => !a.Deleted);
            query = query.OrderBy(a => a.Id);
            var affiliates = query.ToList();
            return affiliates;
        }

        /// <summary>
        /// Inserts an affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void InsertAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            _affiliateRepository.Insert(affiliate);
        }

        /// <summary>
        /// Updates the affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void UpdateAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException("affiliate");

            _affiliateRepository.Update(affiliate);
        }
    }
}