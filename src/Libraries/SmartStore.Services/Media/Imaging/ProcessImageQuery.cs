using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using SmartStore.Collections;
using System.Drawing;

namespace SmartStore.Services.Media.Imaging
{
    public class ProcessImageQuery : QueryString
    {
        // Key = Supported token name, Value = Validator
        private readonly static Dictionary<string, Func<string, string, bool>> _supportedTokens = new Dictionary<string, Func<string, string, bool>> 
        { 
            ["w"] = (k, v) => ValidateSizeToken(k, v),
            ["h"] = (k, v) => ValidateSizeToken(k, v),
            ["size"] = (k, v) => ValidateSizeToken(k, v),
            ["q"] = (k, v) => ValidateQualityToken(k, v),
            ["m"] = (k, v) => ValidateScaleModeToken(k, v)
        };

        public ProcessImageQuery()
            : this(null, new NameValueCollection())
        {
        }

        public ProcessImageQuery(byte[] source)
            : this(source, new NameValueCollection())
        {
        }

        public ProcessImageQuery(Stream source)
            : this(source, new NameValueCollection())
        {
        }

        public ProcessImageQuery(Image source)
            : this(source, new NameValueCollection())
        {
        }

        public ProcessImageQuery(string source)
            : this(source, new NameValueCollection())
        {
        }

        public ProcessImageQuery(object source, NameValueCollection query)
        {
            Guard.NotNull(query, nameof(query));

            Source = source;
            DisposeSource = true;
            Notify = true;

            // Add tokens sanitized
            query.AllKeys.Each(key => Add(key, query[key], false));
        }

        public ProcessImageQuery(ProcessImageQuery query)
            : base(query)
        {
            Guard.NotNull(query, nameof(query));

            Source = query.Source;
            Format = query.Format;
            DisposeSource = query.DisposeSource;
        }

        /// <summary>
        /// The source image's physical path, app-relative virtual path, or a Stream, byte array, Image or IFile instance.
        /// </summary>
        public object Source { get; set; }

        public string FileName { get; set; }

        /// <summary>
        /// Whether to dispose the source stream after processing completes
        /// </summary>
        public bool DisposeSource { get; set; }

        /// <summary>
        /// Whether to execute an applicable post processor which
        /// can reduce the resulting file size drastically, but also
        /// can slow down processing time.
        /// </summary>
        public bool ExecutePostProcessor { get; set; } = true;

        public int? MaxWidth
        {
            get => Get<int?>("w") ?? Get<int?>("size");
            set => Set("w", value);
        }

        public int? MaxHeight
        {
            get => Get<int?>("h") ?? Get<int?>("size");
            set => Set("h", value);
        }

        public int? MaxSize
        {
            get => Get<int?>("size");
            set => Set("size", value);
        }

        public int? Quality
        {
            get => Get<int?>("q");
            set => Set("q", value);
        }

        /// <summary>
        /// max (default) | boxpad | crop | min | pad | stretch
        /// </summary>
        public string ScaleMode
        {
            get => Get<string>("m");
            set => Set("m", value);
        }

        /// <summary>
        /// center (default) | top | bottom | left | right | top-left | top-right | bottom-left | bottom-right
        /// </summary>
        public string AnchorPosition
        {
            get => Get<string>("pos");
            set => Set("pos", value);
        }

        public string BackgroundColor
        {
            get => Get<string>("bg");
            set => Set("bg", value);
        }

        /// <summary>
        /// Gets or sets the output file format either as a string ("png", "jpg", "gif" and "svg"),
        /// or as a format object instance.
        /// When format is not specified, the original format of the source image is used (unless it is not a web safe format - jpeg is the fallback in that scenario).
        /// </summary>
        public object Format { get; set; }

        public bool IsValidationMode { get; set; }

        public bool Notify { get; set; }

        public override QueryString Add(string name, string value, bool isUnique)
        {
            // Keep away invalid tokens from underlying query
            if (_supportedTokens.TryGetValue(name, out var validator) && validator(name, value))
            {
                return base.Add(name, value, isUnique);
            }

            return this;
        }

        private T Get<T>(string name)
        {
            return this[name].Convert<T>();
        }

        private void Set<T>(string name, T val)
        {
            if (val == null)
                Remove(name);
            else
                Add(name, val.Convert<string>(), true);
        }


        public bool NeedsProcessing(bool ignoreQualityFlag = false)
        {
            if (this.Count == 0)
                return false;

            if (object.Equals(Format, "svg"))
            {
                // SVG cannot be processed.
                return false;
            }

            if (ignoreQualityFlag && this.Count == 1 && this["q"] != null)
            {
                // Return false if ignoreQualityFlag is true and "q" is the only flag.
                return false;
            }

            if (this.Count == 1 && Quality >= 90)
            {
                // If "q" is the only flag and its value is >= 90, we don't need to process
                return false;
            }

            return true;
        }

        public string CreateHash()
        {
            var hash = string.Empty;

            foreach (var key in this.AllKeys)
            {
                if (key == "m" && this["m"] == "max")
                    continue; // Mode 'max' is default and can be omitted

                hash += "-" + key + this[key];
            }

            return hash;
        }

        public string GetResultExtension()
        {
            if (Format == null)
            {
                return null;
            }
            else if (Format is IImageFormat imageFormat)
            {
                return imageFormat.DefaultExtension;
            }
            else if (Format is string str)
            {
                return str;
            }

            return null;
        }

        #region Static Helpers

        public static ResizeMode ConvertScaleMode(string mode)
        {
            switch (mode.EmptyNull().ToLower())
            {
                case "boxpad":
                    return ResizeMode.BoxPad;
                case "crop":
                    return ResizeMode.Crop;
                case "min":
                    return ResizeMode.Min;
                case "pad":
                    return ResizeMode.Pad;
                case "stretch":
                    return ResizeMode.Stretch;
                default:
                    return ResizeMode.Max;
            }
        }

        public static AnchorPosition ConvertAnchorPosition(string anchor)
        {
            switch (anchor.EmptyNull().ToLower())
            {
                case "top":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.Top;
                case "bottom":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.Bottom;
                case "left":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.Left;
                case "right":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.Right;
                case "top-left":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.TopLeft;
                case "top-right":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.TopRight;
                case "bottom-left":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.BottomLeft;
                case "bottom-right":
                    return SmartStore.Services.Media.Imaging.AnchorPosition.BottomRight;
                default:
                    return SmartStore.Services.Media.Imaging.AnchorPosition.Center;
            }
        }

        private static bool ValidateSizeToken(string key, string value)
        {
            return uint.TryParse(value, out var size) && size < 10000;
        }

        private static bool ValidateQualityToken(string key, string value)
        {
            return uint.TryParse(value, out var q) && q <= 100;
        }

        private static bool ValidateScaleModeToken(string key, string value)
        {
            return (new[] { "max", "boxpad", "crop", "min", "pad", "stretch" }).Contains(value);
        }

        #endregion
    }
}
