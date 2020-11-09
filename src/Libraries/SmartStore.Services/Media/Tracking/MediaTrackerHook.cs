using System;
using System.Collections.Generic;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Media;
using SmartStore.Data;

namespace SmartStore.Services.Media
{
    [Important]
    public sealed class MediaTrackerHook : DbSaveHook<ObjectContextBase, BaseEntity>
    {
        // Track items for the current (SaveChanges) unit.
        private readonly HashSet<MediaTrack> _actionsUnit = new HashSet<MediaTrack>();

        // Track items already processed during the current request.
        private readonly HashSet<MediaTrack> _actionsAll = new HashSet<MediaTrack>();

        // Entities that are not saved yet but contain effective changes. We won't track if an error occurred during save.
        private readonly IDictionary<BaseEntity, HashSet<MediaTrack>> _actionsTemp = new Dictionary<BaseEntity, HashSet<MediaTrack>>();

        private readonly Lazy<IMediaTracker> _mediaTracker;
        private readonly IDbContext _dbContext;

        public MediaTrackerHook(Lazy<IMediaTracker> mediaTracker, IDbContext dbContext)
        {
            _mediaTracker = mediaTracker;
            _dbContext = dbContext;
        }

        internal static bool Silent { get; set; }

        protected override void OnUpdating(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entry, true);
        }

        protected override void OnDeleted(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entry, false);
        }

        protected override void OnInserted(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entry, false);
        }

        protected override void OnUpdated(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entry, false);
        }

        private void HookObject(IHookedEntity entry, bool beforeSave)
        {
            if (Silent)
                return;

            var type = entry.EntityType;

            if (!_mediaTracker.Value.TryGetTrackedPropertiesFor(type, out var properties))
            {
                throw new NotSupportedException();
            }

            var state = entry.InitialState;
            var actions = new HashSet<MediaTrack>();

            foreach (var prop in properties)
            {
                if (beforeSave)
                {
                    if (entry.Entry.TryGetModifiedProperty(_dbContext, prop.Name, out object prevValue))
                    {
                        // Untrack the previous file relation (if not null)
                        TryAddTrack(prop.Album, entry.Entity, prop.Name, prevValue, MediaTrackOperation.Untrack, actions);

                        // Track the new file relation (if not null)
                        TryAddTrack(prop.Album, entry.Entity, prop.Name, entry.Entry.CurrentValues[prop.Name], MediaTrackOperation.Track, actions);

                        _actionsTemp[entry.Entity] = actions;
                    }
                }
                else
                {
                    switch (state)
                    {
                        case EntityState.Added:
                        case EntityState.Deleted:
                            var value = FastProperty.GetProperty(type, prop.Name).GetValue(entry.Entity);
                            TryAddTrack(prop.Album, entry.Entity, prop.Name, value, state == EntityState.Added ? MediaTrackOperation.Track : MediaTrackOperation.Untrack);
                            break;
                        case EntityState.Modified:
                            if (_actionsTemp.TryGetValue(entry.Entity, out actions))
                            {
                                _actionsUnit.AddRange(actions);
                            }
                            break;
                    }
                }
            }
        }

        private void TryAddTrack(string album, BaseEntity entity, string prop, object value, MediaTrackOperation operation, HashSet<MediaTrack> actions = null)
        {
            if (value == null)
                return;

            if ((int)value > 0)
            {
                (actions ?? _actionsUnit).Add(new MediaTrack
                {
                    Album = album,
                    EntityId = entity.Id,
                    EntityName = entity.GetEntityName(),
                    Property = prop,
                    MediaFileId = (int)value,
                    Operation = operation
                });
            }
        }

        public override void OnAfterSaveCompleted()
        {
            // Remove already processed items during this request.
            _actionsUnit.ExceptWith(_actionsAll);

            if (_actionsUnit.Count == 0)
            {
                return;
            }

            _actionsAll.UnionWith(_actionsUnit);

            // Commit all track items in one go
            var tracker = _mediaTracker.Value;
            using (tracker.BeginScope(false))
            {
                tracker.TrackMany(_actionsUnit);
            }

            _actionsUnit.Clear();
            _actionsTemp.Clear();
        }
    }
}
