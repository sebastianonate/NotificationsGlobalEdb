using Microsoft.Extensions.Configuration;
using Notifications.Core;
using Notifications.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Notifications.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure.Logs;
using Newtonsoft.Json.Linq;
using Notifications.Infrastructure.Dataverse;
using Newtonsoft.Json;
using Notifications.Infrastructure.Dataverse.Helper;
using Notifications.Infrastructure.SystemNotifications;

namespace Notifications.Infrastructure.Mails
{
    public class MailSmtpService : INotificationSender<Task<EmailNotificationResponse>, EmailNotificationData>
    {
        private readonly SystemNotificationsService _systemNotificationsService;
        private readonly SmtpConfiguration _smtpConfiguration;
        private readonly LogService<MailSmtpService> _logger;
        private readonly DataverseService _dataverseService;
        private readonly IConfiguration _configuration;
        private readonly FetchTemplateService _fetchTemplateService;
        private readonly ScheduledNotificationsMailService _scheduledNotificationsService;
        public MailSmtpService(
            IConfiguration configuration, 
            SmtpConfiguration smtpConfiguration, 
            LogService<MailSmtpService> logger, 
            DataverseService dataverseService, 
            FetchTemplateService fetchTemplateService, 
            ScheduledNotificationsMailService scheduledNotificationsMailService,
             SystemNotificationsService systemNotificationsService)
        {
            _systemNotificationsService = systemNotificationsService;
            _smtpConfiguration = smtpConfiguration;
            _logger = logger;
            _dataverseService = dataverseService;
            _configuration = configuration;
            _fetchTemplateService = fetchTemplateService;
            _scheduledNotificationsService = scheduledNotificationsMailService;
        }

        public async Task<EmailNotificationResponse> Send(EmailNotificationData data)
        {
            string evento = "Envio email";
            var traceId = Guid.NewGuid();
            //Validate solution 
            var solution = await _dataverseService.GetSolution(data.SolutionId);
            if (!_dataverseService.SolutionExist(solution)) {
                _logger.LogError(evento, $"La solución que consume el servicio no se encuentra registrada. Id: {data.SolutionId}", traceId);
                 return new EmailNotificationResponse(false, 400, "La solución que consume el servicio no se encuentra registrada.", traceId);
            }

            if (!await _fetchTemplateService.TemplateExist(data.TemplateId.ToString())) {
                _logger.LogError(evento, $"La plantilla que intenta utilizar no existe. Id: {data.TemplateId}", traceId);
                return new EmailNotificationResponse(false, 400, "La plantilla que intenta utilizar no existe.", traceId);
            }

            ValidateData(data);

            var plantilla = await _fetchTemplateService.GetTemplate(data.TemplateId.ToString());
            var replacedTemplate = ReplaceTemplate(plantilla, data.ParamsTemplate);

            if (data.DeliverDate > DateTimeHelper.GetDateTimeNow()) {
                var responseScheduleNotification = await _scheduledNotificationsService.SaveScheduleNotifications(data);
                if (!responseScheduleNotification.IsSuccessStatusCode) {
                    _logger.LogError(evento, responseScheduleNotification.ReasonPhrase, traceId);
                    return new EmailNotificationResponse(false, 400, "Ha ocurrido un error al programar la notificación. Consulte el Log para conocer detalles.", traceId);
                }

                _logger.LogInformation(evento, "Notificación programada exitosamente.", traceId);
                return new EmailNotificationResponse(true, 200, "Notificación programada exitosamente.", traceId);
            }

            var tos = MailHelper.GetValidEmails(data.To, out var rejectTos);
            var ccs = MailHelper.GetValidEmails(data.Cc, out var rejectCcs);
            var bcc = MailHelper.GetValidEmails(data.Cco, out var rejectBccs);
            if (tos.Count == 0)
            {
                _logger.LogError("Enviar email", "No hay destinatarios válidos. Escriba direcciones de correo válidas.", traceId);
                return new EmailNotificationResponse(false, 400, "No hay destinatarios válidos. Escriba direcciones de correo válidas.", traceId);
            }

            MailMessage mm = new MailMessage();
            mm.Subject = data.Subject;
            mm.Body = replacedTemplate;
            mm.IsBodyHtml = true;
            mm.From = new MailAddress(data.From.Trim(), data.Name);
            mm.To.Add(String.Join(',',tos));
            if(ccs.Count > 0)
                mm.CC.Add(String.Join(',', ccs));
            if(bcc.Count > 0)
                mm.Bcc.Add(String.Join(',', bcc));
            SetAttachment(data.Attachments, mm.Attachments);
           
            //Configura Delivery Notifications
            mm.Sender = new MailAddress(data.From.Trim());
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.Delay |
                 DeliveryNotificationOptions.OnFailure |
                 DeliveryNotificationOptions.OnSuccess;     
            mm.Headers.Add("Disposition-Notification-To", data.From.Trim());

            //Configuration SMTP
            SmtpClient smtp = ConfigureSmtp();
            var responseOfSend = await Task.Run(() => SendEmail(smtp, mm, traceId));
            await SaveSmtpDataverse(responseOfSend.IsSuccess, data, solution[GetAttributeName("Soluciones", "Name")].ToString(), traceId);
            
            return responseOfSend;
        }

        private SmtpClient ConfigureSmtp() {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = _smtpConfiguration.Host;
            smtp.EnableSsl = _smtpConfiguration.EnableSsl;
            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = _smtpConfiguration.UserName;
            string password = _smtpConfiguration.Password;
            if(string.IsNullOrWhiteSpace(password))
                password = _configuration[_smtpConfiguration.PassSecretNameOnAKV];
            NetworkCred.Password = password;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = NetworkCred;
            smtp.Port = _smtpConfiguration.Port;
            return smtp;
        }

        private  EmailNotificationResponse SendEmail(SmtpClient smtp, MailMessage mm, Guid traceId) {
            try
            {
                smtp.Send(mm);
                _logger.LogInformation($"Envio de Email", "La notificacion se envió correctamente.", traceId);
                return new EmailNotificationResponse(true, 200, "Notificación entregada exitosamente.", traceId);
            }
            catch (Exception ex) {
                //
                _logger.LogError($"Envio de Email", ex.Message, traceId);
                var result = _systemNotificationsService.SendAsync("Error en el servicio SMTP ",ex.Message).Result;
                return new EmailNotificationResponse(false, 400, "Se ha producido un error. Revise el estado en los Logs.", traceId);
            }
        }

        private void ValidateData(EmailNotificationData data) {
            data.Cc ??= new List<string>();
            data.Cco ??= new List<string>();
            data.Attachments ??= new List<FileContentData>();
            data.Name ??= "";
            data.Subject ??= "";
            data.ParamsTemplate ??= new Dictionary<string, string>();  
            data.DeliverDate ??= DateTime.MinValue; 
        }

        public string ReplaceTemplate(string template, Dictionary<string,string> paramsTemplate) {

            foreach (var item in paramsTemplate)
            {
                var key = BuildStructureForReplace(item.Key.ToUpper());
                if(template.Contains(key))
                    template = template.Replace(key, item.Value);
            }
            return template;
        }

        public string BuildStructureForReplace(string key) {
            return $"[${key}$]";
        }

        private void SetAttachment(List<FileContentData> files, AttachmentCollection attachments)
        {
            files.ForEach(x => {
                Attachment att = new Attachment(new MemoryStream(x.ByteArray), x.FileName);
                attachments.Add(att);
            });
        }

        private async Task<HttpResponseMessage> SaveSmtpDataverse(bool isSuccess, EmailNotificationData data, string solutionName, Guid traceId) {
            JObject values = new JObject();
            DateTime currentDateTime = DateTimeHelper.GetDateTimeNow();
            var table = "Notificaciones";
            values[GetAttributeName(table, "Type")] = "Correo";
            values[GetAttributeName(table, "Cco")] = string.Join(";", data.Cco);
            values[GetAttributeName(table, "From")] = data.From;
            values[GetAttributeName(table, "To")] = string.Join(";", data.To);
            values[GetAttributeName(table, "Cc")] = string.Join(";", data.Cc);
            values[GetAttributeName(table, "DeliverDate")] = currentDateTime;
            values[GetAttributeName(table, "DeliverDateText")] = currentDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            values[GetAttributeName(table, "Envio")] = isSuccess? "Exitoso":"Fallido";
            values[GetAttributeName(table, "Subject")] = data.Subject;
            // solucionId : 83e44731-4e58-ec11-8f8f-002248384c5b
            values[$"{GetAttributeName(table, "SolutionId")}@odata.bind"] = FieldBuilderHelper.BuildRelationshipField(_configuration["Dataverse:Tables:Soluciones"], data.SolutionId);
            values[$"{GetAttributeName(table, "SolutionId")}@OData.Community.Display.V1.FormattedValue"] = solutionName;
            values[$"{GetAttributeName(table, "TemplateId")}@odata.bind"] = FieldBuilderHelper.BuildRelationshipField(_configuration["Dataverse:Tables:Plantillas"], data.TemplateId);
            //Plantillasnotificaciones

            var responseDataverse = await _dataverseService.RegistarDatos(_configuration["Dataverse:Tables:Notificaciones"], values);
            if (responseDataverse.IsSuccessStatusCode)
                _logger.LogInformation("Envio email", "Se ha guardado en dataverse la información", traceId);
            else 
                _logger.LogError("Envio email", $"No se pudo guardar la informacion en dataverse. {responseDataverse.ReasonPhrase}", traceId);
            
            return responseDataverse;
        }

        private string GetAttributeName(string tableName, string property)
        {
            return _dataverseService.GetAttributeName(tableName, property);
        }
    }

    public record EmailNotificationResponse(bool IsSuccess, int StatusCode, string Message, Guid TraceId);
}
