using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Provides enumeration for the available bit depths.
    /// </summary>
    public enum BitDepth
    {
        /// <summary>
        /// 1 bit per pixel
        /// </summary>
        Bit1 = 1,

        /// <summary>
        /// 4 bits per pixel
        /// </summary>
        Bit4 = 4,

        /// <summary>
        /// 8 bits per pixel
        /// </summary>
        Bit8 = 8,

        /// <summary>
        /// 16 bits per pixel
        /// </summary>
        Bit16 = 16,

        /// <summary>
        /// 24 bits per pixel
        /// </summary>
        Bit24 = 24,

        /// <summary>
        /// 32 bits per pixel
        /// </summary>
        Bit32 = 32
    }
}
