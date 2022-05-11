using System.Collections.Generic;

namespace Notifications.WebAPI.Contracts
{
    public class ErrorResponse
    {
        public List<ErrorModel> Errors { get; set; } = new List<ErrorModel>();
    }
}