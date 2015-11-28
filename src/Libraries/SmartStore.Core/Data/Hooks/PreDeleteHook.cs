using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// Implements a hook that will run before an entity gets deleted from the database.
    /// </summary>
    public abstract class PreDeleteHook<TEntity> : PreActionHook<TEntity>
    {
        /// <summary>
        /// Returns <see cref="EntityState.Deleted"/> as the hookstate to listen for.
        /// </summary>
        public override EntityState HookStates
        {
            get { return EntityState.Deleted; }
        }
    }
}
