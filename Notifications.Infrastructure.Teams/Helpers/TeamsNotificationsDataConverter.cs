using Microsoft.Extensions.Configuration;
using Notifications.Infrastructure.Dataverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Teams.Helpers
{
    public static class TeamsNotificationsDataConverter
    {

        private static string GetAttributeName(IConfiguration configuration, string property)
        {
            return configuration[$"DV:Tables:NotificacionesProgramadas:{property}"];
        }

        public static TeamsNotificationData From(NotificationScheduleData data, IConfiguration configuration) {

            return new TeamsNotificationData {
                Message = (string?)data.data[GetAttributeName(configuration, "ParametrosCuerpos")],
                SolutionId = Guid.Parse(data.data[GetAttributeName(configuration, "SolutionId")].ToString()),
                Subject = (string?)data.data[GetAttributeName(configuration, "Subject")],
                To = data.data[GetAttributeName(configuration, "To")].ToString().Split(';').ToList()
            };
        }
    }
}
