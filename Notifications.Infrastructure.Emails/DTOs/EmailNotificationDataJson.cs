using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Mails.DTOs
{
    public class EmailNotificationDataJson
    {
        public List<string> To { get; set; }
        public List<string>? Cc { get; set; }
        public List<string>? Cco { get; set; }

        public string From { get; set; }
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public Guid TemplateId { get; set; }
        public Guid SolutionId { get; set; }
        public List<FileDataBase64> Attachments { get; set; }
        public DateTime? DeliverDate { get; set; }
        public Dictionary<string, string> ParamsTemplate { get; set; }
        private List<FileContentData>? Files { get; set; }
        public List<FileContentData> GetFiles() {
            return Files ?? new List<FileContentData>();
        }
        public void SetFiles(List<FileContentData> files) {
            Files = files;
        }

    }

    public record FileDataBase64(string Filename, string base64Data);

}
