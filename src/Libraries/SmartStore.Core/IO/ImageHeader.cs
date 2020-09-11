using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SmartStore.Core.IO
{
    /// <summary>
    /// Taken from http://stackoverflow.com/questions/111345/getting-image-dimensions-without-reading-the-entire-file/111349
    /// Minor improvements including supporting unsigned 16-bit integers when decoding Jfif and added logic
    /// to load the image using new Bitmap if reading the headers fails
    /// </summary>
    public static class ImageHeader
    {
        internal class UnknownImageFormatException : ArgumentException
        {
            public UnknownImageFormatException(string paramName = "", Exception e = null)
                : base("Could not recognise image format.", paramName, e)
            {
            }
        }

        private static readonly Dictionary<byte[], Func<BinaryReader, Size>> _imageFormatDecoders = new Dictionary<byte[], Func<BinaryReader, Size>>()
        {
            { new byte[] { 0x42, 0x4D }, DecodeBitmap },
            { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, DecodeGif },
            { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, DecodeGif },
            { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePng },
            { new byte[] { 0xff, 0xd8 }, DecodeJpeg },
            //{ new byte[] { 0xff, 0xd8, 0xff, 0xe0 }, DecodeJpeg },
            //{ new byte[] { 0xff }, DecodeJpeg2 }
        };

        private static readonly int _maxMagicBytesLength = 0;

        static ImageHeader()
        {
            _maxMagicBytesLength = _imageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;
        }

        /// <summary>        
        /// Gets the dimensions of an image.        
        /// </summary>        
        /// <param name="path">The path of the image to get the dimensions for.</param>        
        /// <returns>The dimensions of the specified image.</returns>       
        public static Size GetDimensions(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File '{0}' does not exist.".FormatInvariant(path));
            }

            var mime = MimeTypes.MapNameToMimeType(path);
            return GetDimensions(File.OpenRead(path), mime, false);
        }

        /// <summary>        
        /// Gets the dimensions of an image.        
        /// </summary>        
        /// <param name="buffer">The bytes of the image to get the dimensions for.</param>
        /// <param name="mime">The MIME type of the image. Can be <c>null</c>.</param> 
        /// <returns>The dimensions of the specified image.</returns>    
        public static Size GetDimensions(byte[] buffer, string mime = null)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return Size.Empty;
            }

            return GetDimensions(new MemoryStream(buffer), mime, false);
        }

        /// <summary>        
        /// Gets the dimensions of an image.        
        /// </summary>        
        /// <param name="input">The stream of the image to get the dimensions for.</param>    
        /// <param name="leaveOpen">If false, the passed stream will get disposed</param>
        /// <returns>The dimensions of the specified image.</returns>    
        public static Size GetDimensions(Stream input, bool leaveOpen = true)
        {
            return GetDimensions(input, null, leaveOpen);
        }

        /// <summary>        
        /// Gets the dimensions of an image.        
        /// </summary>        
        /// <param name="input">The stream of the image to get the dimensions for.</param> 
        /// <param name="mime">The MIME type of the image. Can be <c>null</c>.</param> 
        /// <param name="leaveOpen">If false, the passed stream will get disposed</param>
        /// <returns>The dimensions of the specified image.</returns>    
        public static Size GetDimensions(Stream input, string mime, bool leaveOpen = true)
        {
            Guard.NotNull(input, nameof(input));

            var gdip = false;

            if (!input.CanSeek || input.Length == 0)
            {
                return Size.Empty;
            }

            try
            {
                //if (mime == "image/jpeg")
                //{
                //	// Reading JPEG header does not work reliably
                //	gdip = true;
                //	return GetDimensionsByGdip(input);
                //}
                if (mime == "image/svg+xml")
                {
                    return GetDimensionsFromSvg(input);
                }

                using (var reader = new BinaryReader(input, Encoding.Unicode, true))
                {
                    return GetDimensions(reader);
                }
            }
            catch (Exception ex)
            {
                if (gdip)
                {
                    throw ex;
                }

                // something went wrong with fast image access,
                // so get original size the classic way
                try
                {
                    input.Seek(0, SeekOrigin.Begin);
                    return GetDimensionsByGdip(input);
                }
                catch
                {
                    throw ex;
                }
            }
            finally
            {
                if (!leaveOpen)
                {
                    input.Dispose();
                }
                else
                {
                    input.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>        
        /// Gets the dimensions of an image.        
        /// </summary>        
        /// <param name="path">The path of the image to get the dimensions of.</param>        
        /// <returns>The dimensions of the specified image.</returns>        
        /// <exception cref="ArgumentException">The image was of an unrecognised format.</exception>            
        public static Size GetDimensions(BinaryReader binaryReader)
        {
            byte[] magicBytes = new byte[_maxMagicBytesLength];
            for (int i = 0; i < _maxMagicBytesLength; i += 1)
            {
                magicBytes[i] = binaryReader.ReadByte();
                foreach (var kvPair in _imageFormatDecoders)
                {
                    if (StartsWith(magicBytes, kvPair.Key))
                    {
                        var size = kvPair.Value(binaryReader);
                        if (size.IsEmpty)
                        {
                            break;
                        }
                        else
                        {
                            return size;
                        }
                    }
                }
            }

            throw new UnknownImageFormatException("binaryReader");
        }

        private static Size GetDimensionsByGdip(Stream input)
        {
            using (var b = Image.FromStream(input, false, false))
            {
                return new Size(b.Width, b.Height);
            }
        }

        private static Size GetDimensionsFromSvg(Stream input)
        {
            using (var reader = XmlReader.Create(input))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "svg")
                        {
                            var width = reader["width"];
                            var height = reader["height"];

                            var size = new Size(width.ToInt(), height.ToInt());
                            if (size.Width == 0 || size.Height == 0)
                            {
                                var viewBox = reader["viewBox"];
                                if (viewBox.HasValue())
                                {
                                    var arrViewBox = viewBox.Trim().Split(' ');
                                    if (arrViewBox.Length == 4)
                                    {
                                        size = new Size(arrViewBox[2].ToInt(), arrViewBox[3].ToInt());
                                    }
                                }
                            }

                            return size;
                        }
                    }
                }
            }

            return Size.Empty;
        }

        private static bool StartsWith(byte[] thisBytes, byte[] thatBytes)
        {
            for (int i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static short ReadLittleEndianInt16(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(short)];

            for (int i = 0; i < sizeof(short); i += 1)
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        private static ushort ReadLittleEndianUInt16(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(ushort)];

            for (int i = 0; i < sizeof(ushort); i += 1)
            {
                bytes[sizeof(ushort) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToUInt16(bytes, 0);
        }

        private static int ReadLittleEndianInt32(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        private static Size DecodeBitmap(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(16);
            int width = binaryReader.ReadInt32();
            int height = binaryReader.ReadInt32();
            return new Size(width, height);
        }

        private static Size DecodeGif(BinaryReader binaryReader)
        {
            int width = binaryReader.ReadInt16();
            int height = binaryReader.ReadInt16();
            return new Size(width, height);
        }

        private static Size DecodePng(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(8);
            int width = ReadLittleEndianInt32(binaryReader);
            int height = ReadLittleEndianInt32(binaryReader);
            return new Size(width, height);
        }

        private static Size DecodeJpeg(BinaryReader reader)
        {
            string state = "started";
            while (true)
            {
                byte[] c;
                if (state == "started")
                {
                    c = reader.ReadBytes(1);
                    state = (c[0] == 0xFF) ? "sof" : "started";
                }
                else if (state == "sof")
                {
                    c = reader.ReadBytes(1);
                    if (c[0] >= 0xe0 && c[0] <= 0xef)
                    {
                        state = "skipframe";
                    }
                    else if ((c[0] >= 0xC0 && c[0] <= 0xC3) || (c[0] >= 0xC5 && c[0] <= 0xC7) || (c[0] >= 0xC9 && c[0] <= 0xCB) || (c[0] >= 0xCD && c[0] <= 0xCF))
                    {
                        state = "readsize";
                    }
                    else if (c[0] == 0xFF)
                    {
                        state = "sof";
                    }
                    else
                    {
                        state = "skipframe";
                    }
                }
                else if (state == "skipframe")
                {
                    c = reader.ReadBytes(2);
                    int skip = ReadInt(c) - 2;
                    reader.ReadBytes(skip);
                    state = "started";
                }
                else if (state == "readsize")
                {
                    c = reader.ReadBytes(7);
                    var width = ReadInt(new[] { c[5], c[6] });
                    var height = ReadInt(new[] { c[3], c[4] });
                    return new Size(width, height);
                }
            }

            throw new UnknownImageFormatException();
        }

        private static int ReadInt(byte[] chars)
        {
            return (chars[0] << 8) + chars[1];
        }

        #region Experiments

        //private static Size DecodeJpeg2(BinaryReader reader)
        //{
        //	bool found = false;
        //	bool eof = false;

        //	while (!found || eof)
        //	{
        //		// read 0xFF and the type
        //		//reader.ReadByte();
        //		byte type = reader.ReadByte();

        //		// get length
        //		int len = 0;
        //		switch (type)
        //		{
        //			// start and end of the image
        //			case 0xD8:
        //			case 0xD9:
        //				len = 0;
        //				break;

        //			// restart interval
        //			case 0xDD:
        //				len = 2;
        //				break;

        //			// the next two bytes is the length
        //			default:
        //				int lenHi = reader.ReadByte();
        //				int lenLo = reader.ReadByte();
        //				len = (lenHi << 8 | lenLo) - 2;
        //				break;
        //		}

        //		// EOF?
        //		if (type == 0xD9)
        //			eof = true;

        //		// process the data
        //		if (len > 0)
        //		{
        //			// read the data
        //			byte[] data = reader.ReadBytes(len);

        //			// this is what we are looking for
        //			if (type == 0xC0)
        //			{
        //				int width = data[1] << 8 | data[2];
        //				int height = data[3] << 8 | data[4];
        //				return new Size(width, height);
        //			}
        //		}
        //	}

        //	throw new UnknownImageFormatException();
        //}

        //private static Size DecodeJfif(BinaryReader reader)
        //{
        //	while (reader.ReadByte() == 0xff)
        //	{
        //		byte marker = reader.ReadByte();
        //		short chunkLength = ReadLittleEndianInt16(reader);
        //		if (marker == 0xc0)
        //		{
        //			reader.ReadByte();
        //			int height = ReadLittleEndianInt16(reader);
        //			int width = ReadLittleEndianInt16(reader);
        //			return new Size(width, height);
        //		}

        //		if (chunkLength < 0)
        //		{
        //			ushort uchunkLength = (ushort)chunkLength;
        //			reader.ReadBytes(uchunkLength - 2);
        //		}
        //		else
        //		{
        //			reader.ReadBytes(chunkLength - 2);
        //		}
        //	}

        //	throw new UnknownImageFormatException();
        //}

        #endregion
    }
}
