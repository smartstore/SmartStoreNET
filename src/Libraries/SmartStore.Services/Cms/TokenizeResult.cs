using System;

namespace SmartStore.Services.Cms
{
    [Serializable]
    public partial class TokenizeResult
    {
        public TokenizeResult(TokenizeType type, object value)
        {
            Type = type;
            Value = value;
        }

        public static implicit operator string (TokenizeResult obj)
        {
            if (obj != null)
            {
                return obj.Result.EmptyNull();
            }

            return string.Empty;
        }

        public TokenizeType Type { get; private set; }

        public object Value { get; private set; }

        public string Result { get; set; }
    }


    public enum TokenizeType
    {
        Product = 0,
        Category,
        Manufacturer,
        Topic,
        Url = 20,
        File = 30
    }
}
