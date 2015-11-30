using System;
using System.Net;
using System.Net.Configuration;
using System.Configuration;
using System.Net.Mail;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Core.Email
{
    public class SmtpContext
    {

        public SmtpContext(string host, int port = 25)
        {
			Guard.ArgumentNotEmpty(() => host);
			Guard.ArgumentIsPositive(port, "port");
			
			this.Host = host;
            this.Port = port;
        }

		public SmtpContext(EmailAccount account)
		{
			Guard.ArgumentNotNull(() => account);

			this.Host = account.Host;
			this.Port = account.Port;
			this.EnableSsl = account.EnableSsl;
			this.Password = account.Password;
			this.UseDefaultCredentials = account.UseDefaultCredentials;
			this.Username = account.Username;
		}
		
		public bool UseDefaultCredentials 
		{ 
			get; 
			set; 
		}

		public string Host 
		{ 
			get; 
			set; 
		}

		public int Port 
		{ 
			get; 
			set; 
		}

		public string Username 
		{ 
			get; 
			set; 
		}

		public string Password 
		{ 
			get; 
			set; 
		}

		public bool EnableSsl 
		{ 
			get; 
			set; 
		}

		public SmtpClient ToSmtpClient()
		{
			var smtpClient = new SmtpClient(this.Host, this.Port);

			smtpClient.UseDefaultCredentials = this.UseDefaultCredentials;
			smtpClient.EnableSsl = this.EnableSsl;
			if (this.UseDefaultCredentials)
			{
				smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
			}
			else
			{
				smtpClient.Credentials = new NetworkCredential(this.Username, this.Password);
			}

			return smtpClient;
		}

    }
}
