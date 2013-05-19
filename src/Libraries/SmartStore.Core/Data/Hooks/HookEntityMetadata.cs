using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SmartStore.Core.Data.Hooks
{
    public class HookEntityMetadata
    {
        public HookEntityMetadata(EntityState state)
        {
            _state = state;
        }

        private EntityState _state;
        public EntityState State
        {
            get { return this._state; }
            set
            {
                if (_state != value)
                {
                    this._state = value;
                    HasStateChanged = true;
                }
            }
        }

        public bool HasStateChanged { get; private set; }
    }
}
