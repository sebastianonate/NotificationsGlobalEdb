using Microsoft.Extensions.Configuration;
using Notifications.Core;
using Notifications.Core.Contracts;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using Notifications.Core.Helpers;
using System.Text.RegularExpressions;

namespace Notifications.Infrastructure.Mails
{
    public class MailSendGridService : INotificationSender<Task<Response>, EmailNotificationData>
    {
        private readonly IConfiguration _configuration;
        public MailSendGridService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Response> Send(EmailNotificationData data)
        {
            var apiKey = _configuration["SendGrid:API_KEY"];
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(data.From, "Notificador"),

                Subject = data.Subject,
                PlainTextContent = "ss",
                HtmlContent = $"<strong>ss</strong>"
            };

            var tos = EmailAddressFromString(data.To);
            var ccs = EmailAddressFromString(data.Cc);
            var bcc = EmailAddressFromString(data.Cco);

            msg.AddTos(tos);

            if (ccs.Count > 0)
                msg.AddCcs(ccs);

            if (bcc.Count > 0)
                msg.AddBccs(bcc);

            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            return response;
        }

        private List<EmailAddress> EmailAddressFromString(List<string> emailAddress) {

            List<EmailAddress> result = new List<EmailAddress>();

            emailAddress.ForEach(email => {
                if (Core.Helpers.MailHelper.IsValidEmail(email))
                    result.Add(new EmailAddress(email));
            });

            return result;
        }


        public async Task<IRestResponse> GetAllMessagesBounces()
        {
            var apiKey = _configuration["SendGrid:API_KEY"];
            var client = new RestClient("https://api.sendgrid.com/v3/suppression/bounces");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {apiKey}");
            request.AddParameter("application/json", "{}", ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<IRestResponse> GetAllMessageBlocks()
        {
            var apiKey = _configuration["SendGrid:API_KEY"];
            var client = new RestClient("https://api.sendgrid.com/v3/suppression/blocks");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Bearer {apiKey}");
            request.AddParameter("application/json", "{}", ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        // startcode seria la fecha 1637102398 
        public async Task<IRestResponse> GetStartMessageBounces(int startCode)
        {
            var apiKey = _configuration["SendGrid:API_KEY"];
            var client = new RestClient($"https://api.sendgrid.com/v3/suppression/bounces?start_time={startCode}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {apiKey}");
            request.AddParameter("application/json", "{}", ParameterType.RequestBody);
            IRestResponse response = await client.ExecuteAsync(request);
            return response;
        }


        public async Task<Response> ScheduledSend(EmailNotificationData data)
        {
            var apiKey = _configuration["SendGrid:API_KEY"];
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(data.From, "Notificador"),

                Subject = data.Subject,
                PlainTextContent = "ss",
                HtmlContent = $"<strong>ss</strong>",
                SendAt = DateTimeToUnix( data.DeliverDate??=DateTime.Now)
                
            };

            var tos = EmailAddressFromString(data.To);
            var ccs = EmailAddressFromString(data.Cc);
            var bcc = EmailAddressFromString(data.Cco);

            msg.AddTos(tos);

            if (ccs.Count > 0)
                msg.AddCcs(ccs);

            if (bcc.Count > 0)
                msg.AddBccs(bcc);

            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            return response;
        }

        private long DateTimeToUnix(DateTime MyDateTime)
        {
            TimeSpan timeSpan = MyDateTime - new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();

            return (long)timeSpan.TotalSeconds;
        }
    }
}
