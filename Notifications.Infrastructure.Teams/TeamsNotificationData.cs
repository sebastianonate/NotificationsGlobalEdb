using System.ComponentModel.DataAnnotations;

namespace Notifications.Infrastructure.Teams
{
    public class TeamsNotificationData
    {
        [Required]
        public List<string> To { get; set; }
        public string? Subject { get; set; }

        public Guid SolutionId { get; set; }
         
        [Required]
        public string Message { get; set; }

        public DateTime? DeliverDate { get; set; }
    }   
}
