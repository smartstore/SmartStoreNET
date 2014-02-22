using System;
using System.Text;
using System.IO;
using System.Net.Mime;
using net = System.Net.Mail;

namespace SmartStore.Core.Email
{
    /// <summary>
    /// attachment-multipart
    /// </summary>
    public class Attachment
    {
        //private readonly net.Attachment _attachment;
        private net.Attachment _attachment;

        public Attachment()
        {
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(""));
            this.Stream = stream;

            _attachment = new net.Attachment(this.Stream, "");
        }

        public Attachment(string filePath)
        {
            _attachment = new net.Attachment(filePath);
            this.GetContentFromFile(filePath);
        }

        public Attachment(string filePath, string mediaType)
        {
            _attachment = new net.Attachment(filePath, mediaType);
        }

        public ContentType ContentType
        {
            get
            {
                return _attachment.ContentType;
            }
            set
            {
                _attachment.ContentType = value;
            }
        }
        public string Name
        {
            get
            {
                return _attachment.Name;
            }
            set
            {
                _attachment.Name = value;
            }
        }
        public string MediaType
        {
            get
            {
                return _attachment.ContentType.MediaType;
            }
            set
            {
                _attachment.ContentType.MediaType = value;
            }
        }
        public ContentDisposition ContentDisposition
        {
            get
            {
                return _attachment.ContentDisposition;
            }
        }
        public TransferEncoding ContentTransferEncoding
        {
            get
            {
                return _attachment.TransferEncoding;
            }
            set
            {
                _attachment.TransferEncoding = value;
            }
        }
        public string ContentDescription { get; set; }
        public Stream Stream { get; set; }
        //public string FileName { get; set; }

        public void GetContentFromFile(string location)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(location, FileMode.Open, FileAccess.Read);
            
            int length = (int)fileStream.Length;
            buffer = new byte[length];
            int count;
            int sum = 0;

            while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                sum += count;

            this.Stream = new MemoryStream(buffer);

            string extension = Path.GetExtension(location);
            string filename = Path.GetFileName(location);

            switch(extension)
            {
                case ".txt":
                    this.MediaType = "text/plain";
                    break;
                case ".jpg":
                    this.MediaType = "image/jpeg";
                    break;
                default:
                    this.MediaType = "application/octet-stream";
                    break;
            }

            this.Name = filename;
            this.ContentTransferEncoding = TransferEncoding.Base64;
            this.ContentDisposition.FileName = filename;
            this.ContentDisposition.DispositionType = "attachment";
        }

        public void GetContentFromString(string content)
        {
            this.Stream = new MemoryStream(Encoding.UTF8.GetBytes(content));   
            //this.Stream = new MemoryStream(Convert.FromBase64String(content));   
        }

        public void GetContentFromBase64String(string content)
        {
            this.Stream = new MemoryStream(Convert.FromBase64String(content));
        }

        //NEW METHODS TO WRAP ATTACHMENT
        //TODO: Testen
        public net.Attachment Instance
        {
            get
            {
                return _attachment;
            }
            set
            {
                _attachment = value;
            }
        }

        public Attachment CreateAttachmentFromString(string content, ContentType contentType)
        {
            this.Instance = net.Attachment.CreateAttachmentFromString(content, contentType);
            return InitAttachment(_attachment);
        }

        public Attachment CreateAttachmentFromString(string content, string name)
        {
            this.Instance = net.Attachment.CreateAttachmentFromString(content, name);
            return InitAttachment(_attachment);
        }

        public Attachment CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
        {
            this.Instance = net.Attachment.CreateAttachmentFromString(content, name, contentEncoding, mediaType);
            return InitAttachment(_attachment);
        }

        public Attachment InitAttachment(net.Attachment tempAttachment) 
        { 
            this.Stream = tempAttachment.ContentStream;
            this.Name = tempAttachment.Name;
            this.ContentTransferEncoding = tempAttachment.TransferEncoding;
            this.ContentType = tempAttachment.ContentType;
            this.MediaType = tempAttachment.ContentType.MediaType;

            return this;
        }

    }
}