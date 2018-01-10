using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;

namespace SmartStore.Services.Polls
{
    public partial class PollService : IPollService
    {
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<PollAnswer> _pollAnswerRepository;
        private readonly IRepository<PollVotingRecord> _pollVotingRecords;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IEventPublisher _eventPublisher;
		private readonly IStoreContext _storeContext;

        public PollService(
			IRepository<Poll> pollRepository, 
            IRepository<PollAnswer> pollAnswerRepository,
            IRepository<PollVotingRecord> pollVotingRecords,
			IRepository<StoreMapping> storeMappingRepository,
			IEventPublisher eventPublisher,
			IStoreContext storeContext)
        {
            this._pollRepository = pollRepository;
            this._pollAnswerRepository = pollAnswerRepository;
            this._pollVotingRecords = pollVotingRecords;
			this._storeMappingRepository = storeMappingRepository;
            this._eventPublisher = eventPublisher;
			this._storeContext = storeContext;

			this.QuerySettings = DbQuerySettings.Default;
        }

		public DbQuerySettings QuerySettings { get; set; }

		private IQueryable<Poll> Filter(IQueryable<Poll> query, bool showHidden)
		{
			if (!showHidden)
			{
				var utcNow = DateTime.UtcNow;
				query = query.Where(p => p.Published);
				query = query.Where(p => !p.StartDateUtc.HasValue || p.StartDateUtc <= utcNow);
				query = query.Where(p => !p.EndDateUtc.HasValue || p.EndDateUtc >= utcNow);

				if (!QuerySettings.IgnoreMultiStore)
				{
					var currentStoreId = _storeContext.CurrentStore.Id;

					query =
						from p in query
						join sm in _storeMappingRepository.Table
						on new { c1 = p.Id, c2 = "Poll" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
						from sm in p_sm.DefaultIfEmpty()
						where !p.LimitedToStores || currentStoreId == sm.StoreId
						select p;
				}
			}

			return query;
		}

        public virtual Poll GetPollById(int pollId)
        {
            if (pollId == 0)
                return null;

            return _pollRepository.GetById(pollId);
        }

		public virtual Poll GetPollBySystemKeyword(string systemKeyword, int languageId, bool showHidden = false)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return null;

            var query = 
				from p in _pollRepository.Table
				where p.SystemKeyword == systemKeyword && p.LanguageId == languageId
				select p;

			query = Filter(query, showHidden);

            var poll = query.FirstOrDefault();
            return poll;
        }
        
        public virtual IPagedList<Poll> GetPolls(int languageId, bool loadShownOnHomePageOnly, int pageIndex, int pageSize, bool showHidden = false)
        {
            var query = _pollRepository.Table;
			query = Filter(query, showHidden);
            
			if (loadShownOnHomePageOnly)
            {
                query = query.Where(p => p.ShowOnHomePage);
            }
            
			if (languageId > 0)
            {
                query = query.Where(p => p.LanguageId == languageId);
            }
            
			query = query.OrderBy(p => p.DisplayOrder);

            var polls = new PagedList<Poll>(query, pageIndex, pageSize);
            return polls;
        }

        public virtual void DeletePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Delete(poll);
        }

        public virtual void InsertPoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Insert(poll);
        }

        public virtual void UpdatePoll(Poll poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");

            _pollRepository.Update(poll);
        }
        
        public virtual PollAnswer GetPollAnswerById(int pollAnswerId)
        {
            if (pollAnswerId == 0)
                return null;

            var query = from pa in _pollAnswerRepository.Table
                        where pa.Id == pollAnswerId
                        select pa;
            var pollAnswer = query.SingleOrDefault();
            return pollAnswer;
        }
        
        public virtual void DeletePollAnswer(PollAnswer pollAnswer)
        {
            if (pollAnswer == null)
                throw new ArgumentNullException("pollAnswer");

            _pollAnswerRepository.Delete(pollAnswer);
        }

        public virtual bool AlreadyVoted(int pollId, int customerId)
        {
            if (pollId == 0 || customerId == 0)
                return false;

            var result = (from pa in _pollAnswerRepository.Table
                          join pvr in _pollVotingRecords.Table on pa.Id equals pvr.PollAnswerId
                          where pa.PollId == pollId && pvr.CustomerId == customerId
                          select pvr).Count() > 0;
            return result;
        }
    }
}
