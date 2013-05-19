using System;

namespace SmartStore
{

    /// <summary>
    /// Generic variant of <see cref="ICloneable" />.
    /// </summary>
    /// <typeparam name="T">The type of object that is cloned</typeparam>
    public interface ICloneable<T> : ICloneable
    {
        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>The cloned instance</returns>
        new T Clone();
    }

}
