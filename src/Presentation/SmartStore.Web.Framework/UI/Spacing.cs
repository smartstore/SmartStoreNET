using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public struct Spacing
    {
        public byte? Left { get; set; }
        public byte? Right { get; set; }
        public byte? Top { get; set; }
        public byte? Bottom { get; set; }

        public byte? X
        {
            get
            {
                if (Left.HasValue && Left == Right)
                    return Left;

                return null;
            }
        }

        public byte? Y
        {
            get
            {
                if (Top.HasValue && Top == Bottom)
                    return Top;

                return null;
            }
        }

        public bool AllEqual()
        {
            return Left.HasValue && Left == Right && Right == Bottom && Bottom == Top;
        }

        public bool XEquals(Spacing other)
        {
            return X.HasValue && X == other.X;
        }

        public bool YEquals(Spacing other)
        {
            return Y.HasValue && Y == other.Y;
        }

        public Spacing Difference(Spacing other)
        {
            var result = this;

            if (Left == other.Left) result.Left = null;
            if (Right == other.Right) result.Right = null;
            if (Top == other.Top) result.Top = null;
            if (Bottom == other.Bottom) result.Bottom = null;

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is Spacing other)
            {
                return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                .Add(typeof(Spacing))
                .Add(this.Left)
                .Add(this.Right)
                .Add(this.Top)
                .Add(this.Bottom)
                .CombinedHash;
        }
    }
}
