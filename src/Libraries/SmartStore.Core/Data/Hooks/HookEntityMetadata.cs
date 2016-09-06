using System;

namespace SmartStore.Core.Data.Hooks
{
    public class HookEntityMetadata
    {
		private EntityState _state;

		public HookEntityMetadata(EntityState state)
        {
            _state = state;
        }
      
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

        public bool HasStateChanged
		{
			get;
			private set;
		}
    }
}
