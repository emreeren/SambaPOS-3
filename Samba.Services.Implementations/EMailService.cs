using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace Samba.Services.Implementations
{
    [Export(typeof(IEmailService))]
    public class EMailService : IEmailService
    {
        private readonly ILogService _logService;

        [ImportingConstructor]
        public EMailService(ILogService logService)
        {
            _logService = logService;
        }

        public void SendEmail(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string ccEmailAddresses, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile, bool bypassSslErrors)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(smtpServerAddress);
            try
            {
                mail.From = new MailAddress(fromEmailAddress);
                toEmailAddress.Split(';').ToList().ForEach(x => mail.To.Add(x));
                mail.Subject = subject;
                mail.Body = body;
                ccEmailAddresses.Split(';').ToList().ForEach(x => mail.CC.Add(x));
                if (!string.IsNullOrEmpty(fileName))
                    fileName.Split(',').ToList().ForEach(x => mail.Attachments.Add(new Attachment(x)));

                smtpServer.Port = smtpPort;
                smtpServer.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                smtpServer.EnableSsl = true;
                if (bypassSslErrors)
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                smtpServer.Send(mail);
            }
            catch (Exception e)
            {
                _logService.LogError(e);
            }
            finally
            {
                if (deleteFile && !string.IsNullOrEmpty(fileName))
                {
                    fileName.Split(',').ToList().ForEach(
                        x =>
                        {
                            if (File.Exists(x))
                            {
                                try
                                {
                                    File.Delete(x);
                                }
                                catch (Exception) { }
                            }
                        });
                }
            }
        }

        public void SendEMailAsync(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string ccEmailAddresses, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile, bool byPassSslErrors)
        {
            var thread = new Thread(() => SendEmail(smtpServerAddress, smtpUser, smtpPassword, smtpPort, toEmailAddress, ccEmailAddresses, fromEmailAddress, subject, body, fileName, deleteFile, byPassSslErrors));
            thread.Start();
        }
    }
}
