using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Notifications.Infrastructure.Dataverse;
using Notifications.Infrastructure.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Teams
{
    public class ScheduledNotificationsTeamsService
    {
        private readonly DataverseService _dataverseService;
        private readonly IConfiguration _configuration;
        private readonly LogService<ScheduledNotificationsTeamsService> _logger;
        public ScheduledNotificationsTeamsService(
            DataverseService dataverseService, 
            IConfiguration configuration, 
            LogService<ScheduledNotificationsTeamsService> logger)
        {
            _dataverseService = dataverseService;
            _configuration = configuration;
            _logger = logger;

        }
        public async Task<HttpResponseMessage> SaveScheduleNotificationsTeams(TeamsNotificationData data) {
            JObject content = new JObject();
            var table = "NotificacionesProgramadas";
            content[GetAttributeName(table, "Type")] = "Teams";
            content[GetAttributeName(table, "To")] = string.Join(";", data.To);
            content[GetAttributeName(table, "Subject")] = data.Subject;
            content[GetAttributeName(table, "SolutionId")] = data.SolutionId;
            content[GetAttributeName(table, "DeliverDate")] = data.DeliverDate;
            content[GetAttributeName(table, "ParametrosCuerpos")] = data.Message;
            var response = await _dataverseService.RegistarDatos(_configuration["Dataverse:Tables:NotificacionesProgramadas"], content);
            return response;
        }

        private string GetAttributeName(string tableName, string property)
        {
            return _dataverseService.GetAttributeName(tableName, property);
        }
    }
}
