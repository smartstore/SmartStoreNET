using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// Implements a hook that will run before an entity gets updated in the database.
    /// </summary>
    public abstract class PreUpdateHook<TEntity> : PreActionHook<TEntity>
    {
        /// <summary>
        /// Returns <see cref="EntityState.Modified"/> as the hookstate to listen for.
        /// </summary>
        public override EntityState HookStates
        {
            get { return EntityState.Modified; }
        }
    }
}
