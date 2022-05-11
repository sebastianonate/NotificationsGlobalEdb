using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Mails
{
    public class SmtpConfiguration
    {

        public int Port { get; set; }
        public string Host { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PassSecretNameOnAKV { get; set; }


    }
}
