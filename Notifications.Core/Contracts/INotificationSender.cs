using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Core.Contracts
{
    public interface INotificationSender<T,K> where T : class where K : class
    {
       T Send(K data);
    }
}
