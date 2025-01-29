using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace CyberBazaECommerce.Services
{
	public class EmailService
	{
		private readonly string _smtpServer;
		private readonly int _smtpPort;
		private readonly string _smtpUsername;
		private readonly string _smtpPassword;
		private readonly string _emailFrom;


		public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword, string emailFrom)
		{
			_smtpServer = smtpServer;
			_smtpPort = smtpPort;
			_smtpUsername = smtpUsername;
			_smtpPassword = smtpPassword;
			_emailFrom = emailFrom;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("CyberBaza", _emailFrom));
			message.To.Add(new MailboxAddress("", toEmail));
			message.Subject = subject;

			message.Body = new TextPart("html")
			{
				Text = body
			};

			using (var client = new SmtpClient())
			{
				client.Connect(_smtpServer, _smtpPort, false);
				client.Authenticate(_smtpUsername, _smtpPassword);
				await client.SendAsync(message);
				client.Disconnect(true);
			}
		}
		public async Task SendResetPasswordEmailAsync(string toEmail, string resetCode)
		{
			var subject = "Password Reset Request";
			var body = $"<h1>Your code for password reset is:</h1><h1><strong>{resetCode}</strong></h1>";
			await SendEmailAsync(toEmail, subject, body);
		}
	}
}