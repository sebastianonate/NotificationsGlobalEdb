using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notifications.Core.Contracts;
using Notifications.Core;
using Notifications.Infrastructure.Mails;
using Notifications.Infrastructure.Logs;
using Newtonsoft.Json;
using Notifications.WebAPI.DTOs;
using Notifications.Infrastructure.Mails.DTOs;
using Notifications.Infrastructure.Mails.Helpers;

namespace Notifications.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly MailSmtpService _mailService;
        private readonly ILogger<MailController> _logger;
        private readonly LogService<MailController> _logserivce;
        public MailController(MailSmtpService mailService, ILogger<MailController> logger, LogService<MailController> log)
        {
            _mailService = mailService;
            _logger = logger;
            _logserivce = log;
        }

        [DisableRequestSizeLimit]
        [HttpPost("Form/Send")]
        public async Task<IActionResult> SendEmail([FromForm] EmailNotificationDataForm data)
        {
            var exits = HttpContext.Request.Form.TryGetValue("paramsTemplate", out var stringParamsTemplate);
            Dictionary<string, string> paramsTemplate = new Dictionary<string, string>();
            stringParamsTemplate.ToList().ForEach(x => {
                paramsTemplate = JsonConvert.DeserializeObject<Dictionary<string, string>>(x);
            });
            data.ParamsTemplate = paramsTemplate;

            var mailData = MailNotificationDataConverter.From(data);

            var response = await _mailService.Send(mailData);
            if(!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        
        [DisableRequestSizeLimit]
        [HttpPost("Json/Send")]
        public async Task<IActionResult> SendEmailRequestJson([FromBody]EmailNotificationDataJson dataJson)
        {
            var data = MailNotificationDataConverter.From(dataJson);
            var response = await _mailService.Send(data);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }


        /*

        [HttpGet("Bounces")]
        public async Task<IActionResult> GetBounce()
        {
            var response = await _mailService.GetAllMessagesBounces();
            return Ok(response.Content);
        }


        [HttpGet("Block")]
        public async Task<IActionResult> GetBlock()
        {
            var response = await _mailService.GetAllMessageBlocks();
            return Ok(response.Content);
        }

        [HttpGet("Bounces/{id}")]
        public async Task<IActionResult> GetStartBounce(int id)
        {
            var response = await _mailService.GetStartMessageBounces(id);
            return Ok(response.Content);
        }

        [HttpPost("SendAsync")]
        public async Task<IActionResult> SendEmailAsync([FromBody] NotificationData data, double minutos)
        {
            var task = new Task(async () => { 
                await Run(async () => { await _mailService.Send(data); }, TimeSpan.FromMinutes(minutos));
            });
            task.Start();
            return Ok("Programdo");
        }

        public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            await Task.Delay(period, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
                action();
        }

        public static Task Run(Action action, TimeSpan period)
        {
            return Run(action, period, CancellationToken.None);
        }
        */
    }
    

    

}
