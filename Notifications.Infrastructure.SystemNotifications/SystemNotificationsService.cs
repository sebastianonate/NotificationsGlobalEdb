using Microsoft.Extensions.Configuration;
using Notifications.Infrastructure.Logs;
using System.Net;
using System.Net.Mail;

namespace Notifications.Infrastructure.SystemNotifications
{
    public class SystemNotificationsService
    {
        private readonly LogService<SystemNotificationsService> _logService;
        private readonly IConfiguration _configuration;
        public SystemNotificationsService(IConfiguration configuration, LogService<SystemNotificationsService> logService)
        {
                _configuration = configuration;
            _logService = logService;   
        }
        private SmtpClient ConfigureSmtp()
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = _configuration["InternalSystemNotifications:Host"];
            bool enableSsl = true;
            bool.TryParse(_configuration["InternalSystemNotifications:EnableSsl"], out enableSsl);
            smtp.EnableSsl = enableSsl ;
            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = _configuration["InternalSystemNotifications:UserName"];
            string password = _configuration["InternalSystemNotifications:Password"];
            if (string.IsNullOrWhiteSpace(password))
                password = _configuration[_configuration["InternalSystemNotifications:PassSecretNameOnAKV"]];
            NetworkCred.Password = password;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = NetworkCred;
            int port = 587;
            int.TryParse(_configuration["InternalSystemNotifications:Port"], out port);
            smtp.Port = port;
            return smtp;
        }


        public async Task<bool> SendAsync(string subject , string body) {


            MailMessage mm = new MailMessage();
            mm.Subject = subject;
            mm.Body = body;
          
            mm.From = new MailAddress(_configuration["InternalSystemNotifications:From"],"InternalSystemGestorNotificaciones");
            mm.To.Add(_configuration["InternalSystemNotifications:To"]);
      
            //Configura Delivery Notifications
            mm.Sender = new MailAddress(_configuration["InternalSystemNotifications:From"]);
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.Delay |
                 DeliveryNotificationOptions.OnFailure |
                 DeliveryNotificationOptions.OnSuccess;
            mm.Headers.Add("Disposition-Notification-To", _configuration["InternalSystemNotifications:From"]);

            //Configuration SMTP
            SmtpClient smtp = ConfigureSmtp();
            var responseOfSend = await Task.Run(() => SendEmail(smtp, mm));
         

            return responseOfSend;

        }
        private bool SendEmail(SmtpClient smtp, MailMessage mm)
        {
            try
            {
                smtp.Send(mm);
                _logService.LogInformation("Envio de notificacion al administrador", "La notificacion se envió correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Envio de notificacion al administrador", ex.Message);
                return false;
            }
        }
    }
}