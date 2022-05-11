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
using Notifications.Infrastructure.Sms;

namespace Notifications.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SmsService smsService;

        public SmsController(IConfiguration configuration, SmsService smsService)
        {
            _configuration = configuration;
            this.smsService = smsService;
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> SendMessage(SendSmsBulkRequest data)
        {
            var response = await smsService.SendSmsBulk(data);
            if (response.IsSuccessStatusCode) return Ok(await response.Content.ReadAsStringAsync());
            return BadRequest(response.ReasonPhrase);
        }

        [HttpPost("individual")]
        public async Task<IActionResult> SendIndividualMessage(SendSmsIndividual data)
        {
            var response = await smsService.SendSms(data);
            if(response.IsSuccessStatusCode) return Ok(await response.Content.ReadAsStringAsync());
            return BadRequest(response.ReasonPhrase);
        }


    }
}
