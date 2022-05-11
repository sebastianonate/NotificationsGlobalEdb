using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Notifications.Infrastructure.Dataverse;
using Notifications.Infrastructure.Mails.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Mails.Helpers
{
    public static class MailNotificationDataConverter
    {

        public static EmailNotificationData From(EmailNotificationDataJson data) {

            var files = new List<FileContentData>();
            if (data.Attachments != null) {
                data.Attachments.ForEach(x =>
                {
                    byte[] bytes = Convert.FromBase64String(x.base64Data);
                    files.Add(new FileContentData(bytes, x.Filename));
                });
            }
            return new EmailNotificationData
            {
                From = data.From,
                To = data.To,
                Subject = data.Subject,
                Cc = data.Cc,
                Cco = data.Cco,
                Attachments = files,
                ParamsTemplate = data.ParamsTemplate,
                DeliverDate = data.DeliverDate,
                Name = data.Name,
                SolutionId = data.SolutionId,
                TemplateId = data.TemplateId
            };
        }
        public static EmailNotificationData From(EmailNotificationDataForm data)
        {
            var files = new List<FileContentData>();
            if (data.Attachments != null)
            {
                data.Attachments.ForEach(x =>
                {
                    using (var ms = new MemoryStream())
                    {
                        x.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        files.Add(new FileContentData(fileBytes, x.FileName));
                    }
                });
            }
            return new EmailNotificationData
            {
                From = data.From,
                To = data.To,
                Subject = data.Subject,
                Cc = data.Cc,
                Cco = data.Cco,
                Attachments = files,
                ParamsTemplate = data.ParamsTemplate,
                DeliverDate = data.DeliverDate,
                Name = data.Name,
                SolutionId = data.SolutionId,
                TemplateId = data.TemplateId
            };

        }

        private static string GetAttributeName(IConfiguration configuration, string property ) {
            return configuration[$"DV:Tables:NotificacionesProgramadas:{property}"];
        }
        public static EmailNotificationData From(NotificationScheduleData data, IConfiguration configuration)
        {

            var files = new List<FileContentData>();
            if (data.files != null)
            {
                data.files.ForEach(x =>
                {
                    files.Add(new FileContentData(x.ByteArray, x.FileName));
                });
            }
            return new EmailNotificationData
            {
                From = (string?)data.data[GetAttributeName(configuration, "SenderEmail")],
                To = data.data[GetAttributeName(configuration, "To")].ToString().Split(';').ToList(),
                Subject = (string?)data.data[GetAttributeName(configuration, "Subject")],
                Cc = data.data[GetAttributeName(configuration, "Cc")].ToString().Split(';').ToList(),
                Cco = data.data[GetAttributeName(configuration, "Cco")].ToString().Split(';').ToList(),
                Attachments = files,
                ParamsTemplate = JsonConvert.DeserializeObject<Dictionary<string,string>>(data.data[GetAttributeName(configuration, "ParametrosCuerpos")].ToString()),
                DeliverDate = Convert.ToDateTime(data.data[GetAttributeName(configuration, "DeliverDate")].ToString()),
                Name = (string?)data.data[GetAttributeName(configuration, "SenderName")],
                SolutionId = Guid.Parse((string?)data.data[GetAttributeName(configuration, "SolutionId")]),
                TemplateId = Guid.Parse((string?)data.data[GetAttributeName(configuration, "TemplateId")])
            };
        }
    }
}
