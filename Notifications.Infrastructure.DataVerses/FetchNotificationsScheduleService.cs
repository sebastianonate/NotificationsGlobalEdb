using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Notifications.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Dataverse
{
    public class FetchNotificationsScheduleService
    {
        private readonly DataverseService _dataverseService;
        private readonly IConfiguration _configuration;
        public FetchNotificationsScheduleService(DataverseService dataverseService, IConfiguration configuration)
        {
            _dataverseService = dataverseService;
            _configuration = configuration;
        }

        // Notificacion programada : 1 reg ->  * archivos
        public async Task<List<NotificationScheduleData>> GetNotificationFromCurrentDate() {
            DateTime currentDateTime = DateTimeHelper.GetDateTimeNow();

            var buildedNotificationsToSend = new List<NotificationScheduleData>();
            var table = "NotificacionesProgramadas";
            var data = await _dataverseService.GetDataFromTable(_configuration["Dataverse:Tables:NotificacionesProgramadas"]);
            var notificationsToSends = data.Where(x => currentDateTime >= Convert.ToDateTime(x[_dataverseService.GetAttributeName(table, "DeliverDate")].ToString()));
            if (notificationsToSends.Any()) {
                foreach (var x in notificationsToSends)
                {
                    var id = (string?)x[_dataverseService.GetAttributeName(table, "Id")];
                    var files = await GetAllfilesByScheduleNotification(id);
                    buildedNotificationsToSend.Add(new NotificationScheduleData(x, files));
                }
            }
            
            return buildedNotificationsToSend;
        }

        public async Task<FileResponse> GetFile(string entityId)
        {
            string entityFileOrAttributeAttributeLogicalName = _dataverseService.GetAttributeName("NotificacionesProgramadasArchivos", "File");
            string tableName = _configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"];
            var downloaded = await _dataverseService.GetFileBytes(tableName, entityId, entityFileOrAttributeAttributeLogicalName);
            if (downloaded == null)
                return null;
            return downloaded;
        }

        public string GetAttributeName(string table, string property) {
            return _dataverseService.GetAttributeName(table, property);
        }

        public async Task<List<FileResponse>> GetAllfilesByScheduleNotification(string notificacionesprogramadasId) {

            string tableName = _configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"];
            string table = "NotificacionesProgramadasArchivos";
            var notificationsFiles = await _dataverseService.GetDataFromTable(tableName);
            var filteredNotificationFiles = notificationsFiles
                .Where(x =>((string?)x[_dataverseService.GetAttributeName(table, "NotificacionesProgramadasIdAttribute")]) == notificacionesprogramadasId);
            List<FileResponse> files = new List<FileResponse>();

            foreach (var item in filteredNotificationFiles) {
                var notiFileId = (string?)item[_dataverseService.GetAttributeName(table, "Id")];
                var file = await GetFile(notiFileId);
                if (file != null) 
                    files.Add(file);
            }
            return files;
        }

        public async Task DeleteNotificationSchedule(string id, List<FileResponse> files) {
            string tableName = _configuration["Dataverse:Tables:NotificacionesProgramadas"];
            await _dataverseService.DeleteRegisterById(tableName, Guid.Parse(id));
            string tableFilesName = _configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"];
            if (files != null) {

                foreach (var item in files)
                {
                    await _dataverseService.DeleteRegisterById(tableFilesName, Guid.Parse(item.FileId));
                }
            }

        
        }
    }

    public record NotificationScheduleData(JToken data, List<FileResponse> files);
}
