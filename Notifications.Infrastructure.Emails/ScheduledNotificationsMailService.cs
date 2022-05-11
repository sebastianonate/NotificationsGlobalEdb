using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notifications.Infrastructure.Dataverse;
using Notifications.Infrastructure.Dataverse.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Mails
{
    public class ScheduledNotificationsMailService
    {
        private readonly DataverseService _dataverseService;
        private readonly IConfiguration _configuration;
        public ScheduledNotificationsMailService(DataverseService dataverseService, IConfiguration configuration)
        {
             _dataverseService = dataverseService;
            _configuration = configuration;
        }
        public async Task<HttpResponseMessage> SaveScheduleNotifications(EmailNotificationData data)
        {

            JObject content = new JObject();
            var parametros = JsonConvert.SerializeObject(data.ParamsTemplate);
            var table = "NotificacionesProgramadas";
            content[GetAttributeName(table, "Type")] = "Correo";
            content[GetAttributeName(table, "Cco")] = string.Join(";", data.Cco);
            content[GetAttributeName(table, "To")] = string.Join(";", data.To);
            content[GetAttributeName(table, "Cc")] = string.Join(";", data.Cc);
            content[GetAttributeName(table, "TemplateId")] = data.TemplateId;
            content[GetAttributeName(table, "ParametrosCuerpos")] = parametros;
            content[GetAttributeName(table, "SolutionId")] = data.SolutionId;
            content[GetAttributeName(table, "SenderName")] = data.Name;
            content[GetAttributeName(table, "SenderEmail")] = data.From;
            content[GetAttributeName(table, "DeliverDate")] = data.DeliverDate;
            content[GetAttributeName(table, "Subject")] = data.Subject;

            var response = await _dataverseService.RegistarDatos(_configuration["Dataverse:Tables:NotificacionesProgramadas"], content);
            var notificacionId = GetId(response.Headers.Location.AbsolutePath);
            if (data.Attachments.Count > 0) 
             await  SaveFileScheduleNotifications(data, notificacionId);

            return response;
        }
        private string GetAttributeName(string tableName, string property)
        {
            return _dataverseService.GetAttributeName(tableName, property);
        }
        public async Task<string> SaveFileScheduleNotifications(EmailNotificationData data, string notificacionesProgramadasId)
        {
            var table = "NotificacionesProgramadasArchivos";
            JObject content = new JObject();
            content[$"{GetAttributeName(table, "RNotificacionesProgramadasNPAchivos")}@odata.bind"] = FieldBuilderHelper.BuildRelationshipField(_configuration["Dataverse:Tables:NotificacionesProgramadas"], Guid.Parse(notificacionesProgramadasId)); 
            content[$"{GetAttributeName(table, "RNotificacionesProgramadasNPAchivos")}@OData.Community.Display.V1.FormattedValue"] = "";

            var listaArchivos = data.Attachments;
            foreach (var item in listaArchivos)
            {
                var response = await _dataverseService.RegistarDatos(_configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"], content);
                var notificacionId = GetId(response.Headers.Location.AbsolutePath);
                Console.WriteLine(notificacionId);
                await _dataverseService.SaveFilesAttachment(item.ByteArray, item.FileName, _configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"], notificacionId, _configuration["DV:Tables:NotificacionesProgramadasArchivos:File"]);
            }
            return "todo bien";
        }

        private string GetId(string response) {
          
            var indexInicio = response.IndexOf('(') + 1;
            var IndexFinal = response.IndexOf(')') - 1;

            var Lista = new StringBuilder();
            var generateId = response.ToCharArray();

            for (int i = indexInicio; i <= IndexFinal; i++)
            {
                Lista.Append(generateId[i]);

            }
            var notificacionesidGenerate = Lista.ToString();
            return notificacionesidGenerate;
        }
    }
}
