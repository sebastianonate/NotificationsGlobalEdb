//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//using Newtonsoft.Json.Linq;
//using Notifications.Core;
//using Notifications.Infrastructure.Dataverse;
//using Notifications.Infrastructure.Logs;

//namespace Notifications.WebAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class DataverseController : ControllerBase
//    {
//        private readonly DataverseService _dataverseService;
//        private readonly LogService<MailController> _logserivce;
//        private readonly FetchTemplateService _consultarPlantillasService;
//        private readonly FetchNotificationsScheduleService _consultarArchivoService;
//        private readonly IConfiguration _configuration;
//        public DataverseController(DataverseService dataverseService, FetchTemplateService consultarPlantillasService, FetchNotificationsScheduleService consultarArchivoService, IConfiguration configuration)
//        {
//            _dataverseService = dataverseService;
//            _consultarPlantillasService = consultarPlantillasService;
//            _consultarArchivoService = consultarArchivoService;
//            _configuration = configuration;
//        }

//        [HttpPost("Save")]
//        public async Task<IActionResult> SaveData(string tableName, JObject contentCreate)
//        {
//            var response = await _dataverseService.RegistarDatos(tableName, contentCreate);
//            return Ok(response);
//        }

//        [HttpGet("DataTable/{tableName}")]
//        public async Task<IActionResult> GetData(string tableName)
//        {
//            var response = await _dataverseService.GetDataFromTable(tableName);
//            return Ok(response);
//        }
//        [HttpGet("DataTable/{tableName}/{Id}")]
//        public async Task<IActionResult> GetDataUser(string tableName, Guid Id)
//        {
//            var response = await _dataverseService.GetRegisterById(tableName, Id);
//            return Ok(response);
//        }

//        [HttpDelete("DataTable/{tableName}/{Id}")]
//        public async Task<IActionResult> DeleteRegister(string tableName, Guid Id)
//        {
//            var response = await _dataverseService.DeleteRegisterById(tableName, Id);
//            return Ok(response);
//        }

//        [HttpGet("DataTableDownload/plantilla")]
//        public async Task<IActionResult> GetPlantilla()
//        {
//            var response = await _consultarPlantillasService.GetTemplate("ec469b82-e25d-ec11-8f8f-000d3a88db19");
//            return Ok(response);
//        }


//        [HttpGet("DataTableDownload/archivo")]
//        public async Task<IActionResult> GetFile()
//        {
//            var response = await _consultarArchivoService.GetFile("9d7905f3-485f-ec11-8f8f-000d3a88db19");
//            //var respuesta = response.FileName;
//            return Ok(response);
//        }

//        [HttpGet("ByteArray/{entityId}")]
//        public async Task<IActionResult> GetFile(string entityId) {
//            var f = await _dataverseService.GetFileBytes(_configuration["Dataverse:Tables:NotificacionesProgramadasArchivos"], entityId, _configuration["DV:Tables:NotifacionesProgramadasArchivos:File"]);
//            return Ok(f);
//        }
//        [HttpGet("GetScheduledNotifications")]
//        public async Task<IActionResult> GetNotificationsScheduled() {

//            var response = await _consultarArchivoService.GetNotificationFromCurrentDate();

//            return Ok(response);
        
//        }
//    }

//}
