using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notifications.Core.Contracts;
using Notifications.Core;
using Microsoft.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Notifications.Infrastructure.Teams;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security;
using Microsoft.Identity.Client;

namespace Notifications.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly TeamsService _teamsService;
        private readonly string _token;
        private readonly IConfiguration _configuration;
        public TeamsController(TeamsService teamsService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _configuration = configuration;
            _teamsService = teamsService;
        }

        [HttpPost("Chat/Message")]
        public async Task<IActionResult> SendMessage(TeamsNotificationData data)
        {
            try
            {
                var response = await _teamsService.Send(data);
                if(response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (ServiceException ex) {
                return BadRequest(ex.Message);
            }
        }

    
     
    }
}
