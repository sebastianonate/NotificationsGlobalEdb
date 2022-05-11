using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Core.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime GetDateTimeNow() {
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo myZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, myZone);
            return currentDateTime;
        }
       
    }
}
