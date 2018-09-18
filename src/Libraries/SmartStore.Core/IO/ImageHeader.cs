using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

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

		private static Dictionary<byte[], Func<BinaryReader, Size>> _imageFormatDecoders = new Dictionary<byte[], Func<BinaryReader, Size>>()
		{
			{ new byte[] { 0x42, 0x4D }, DecodeBitmap },
			{ new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, DecodeGif },
			{ new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, DecodeGif },
			{ new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePng },
   //         { new byte[] { 0xff, 0xd8 }, DecodeJfif },
			//{ new byte[] { 0xff, 0xd8, 0xff, 0xe0 }, DecodeJpeg },
			//{ new byte[] { 0xff }, DecodeJpeg2 },
		};

		private static int _maxMagicBytesLength = 0;

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
				if (mime == "image/jpeg")
				{
					// Reading JPEG header does not work reliably
					gdip = true;
					return GetDimensionsByGdip(input);
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
						return kvPair.Value(binaryReader);
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

		#region Experiments

		private static Size DecodeJpeg(BinaryReader reader)
		{
			// For JPEGs, we need to read the first 12 bytes of each chunk.
			// We'll read those 12 bytes at buf+2...buf+14, i.e. overwriting the existing buf.

			var buf = (new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }).Concat(reader.ReadBytes(20)).ToArray();

			using (var f = new MemoryStream(buf))
			{
				if (buf[6] == (byte)'J' && buf[7] == (byte)'F' && buf[8] == (byte)'I' && buf[9] == (byte)'F')
				{
					var len = buf.Length;
					long pos = 2;
					while (buf[2] == 0xFF)
					{
						if (buf[3] == 0xC0 || buf[3] == 0xC1 || buf[3] == 0xC2 || buf[3] == 0xC3 || buf[3] == 0xC9 || buf[3] == 0xCA || buf[3] == 0xCB) break;
						pos += 2 + (buf[4] << 8) + buf[5];
						if (pos + 12 > len) break;
						//fseek(f, pos, SEEK_SET);
						f.Seek(pos, SeekOrigin.Begin);
						//fread(buf + 2, 1, 12, f);
						f.Read(buf, 0, 12);
					}
				}
			}

			// JPEG: (first two bytes of buf are first two bytes of the jpeg file; rest of buf is the DCT frame
			if (buf[0] == 0xFF && buf[1] == 0xD8 && buf[2] == 0xFF)
			{
				var height = (buf[7] << 8) + buf[8];
				var width = (buf[9] << 8) + buf[10];

				return new Size(width, height);
			}

			throw new UnknownImageFormatException();
		}

		private static Size DecodeJpeg2(BinaryReader reader)
		{
			bool found = false;
			bool eof = false;

			while (!found || eof)
			{
				// read 0xFF and the type
				//reader.ReadByte();
				byte type = reader.ReadByte();

				// get length
				int len = 0;
				switch (type)
				{
					// start and end of the image
					case 0xD8:
					case 0xD9:
						len = 0;
						break;

					// restart interval
					case 0xDD:
						len = 2;
						break;

					// the next two bytes is the length
					default:
						int lenHi = reader.ReadByte();
						int lenLo = reader.ReadByte();
						len = (lenHi << 8 | lenLo) - 2;
						break;
				}

				// EOF?
				if (type == 0xD9)
					eof = true;

				// process the data
				if (len > 0)
				{
					// read the data
					byte[] data = reader.ReadBytes(len);

					// this is what we are looking for
					if (type == 0xC0)
					{
						int width = data[1] << 8 | data[2];
						int height = data[3] << 8 | data[4];
						return new Size(width, height);
					}
				}
			}

			throw new UnknownImageFormatException();
		}

		private static Size DecodeJfif(BinaryReader reader)
		{
			while (reader.ReadByte() == 0xff)
			{
				byte marker = reader.ReadByte();
				short chunkLength = ReadLittleEndianInt16(reader);
				if (marker == 0xc0)
				{
					reader.ReadByte();
					int height = ReadLittleEndianInt16(reader);
					int width = ReadLittleEndianInt16(reader);
					return new Size(width, height);
				}

				if (chunkLength < 0)
				{
					ushort uchunkLength = (ushort)chunkLength;
					reader.ReadBytes(uchunkLength - 2);
				}
				else
				{
					reader.ReadBytes(chunkLength - 2);
				}
			}

			throw new UnknownImageFormatException();
		}

		#endregion
	}
}
