using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Mails.DTOs
{
    public class EmailNotificationDataForm
    {
        public List<string> To { get; set; }
        public List<string>? Cc { get; set; }
        public List<string>? Cco { get; set; }

        // [EmailAddress(ErrorMessage ="Debe ingresar un email valido")]
        public string From { get; set; }
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public Guid TemplateId { get; set; }
        public Guid SolutionId { get; set; }

        //[ListIFormFileFormatAttribute]
        //[ListIFormFileSizeAttribute]
        public List<IFormFile>? Attachments { get; set; }
        //Este campo es la fecha que se va a enviar y debe llegar en Unixtime si se programa la fecha de envio

        public DateTime? DeliverDate { get; set; }
        public Dictionary<string, string> ParamsTemplate { get; set; }
    }


}
