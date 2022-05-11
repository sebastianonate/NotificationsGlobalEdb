using Microsoft.Extensions.Configuration;
using Notifications.Infrastructure.Dataverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Dataverse
{
    public class FetchTemplateService
    {

        private readonly IConfiguration _configuration;
        private readonly GenerateDataverseToken _generateToken;
        private readonly DataverseService _dataverseService;
        public FetchTemplateService(
            IConfiguration configuration, 
            GenerateDataverseToken generateToken, 
            DataverseService dataverseService
            )
        {
            _configuration = configuration;
            _generateToken = generateToken;
            _dataverseService = dataverseService;
        }

        public async Task<bool> TemplateExist(string templateId) { 
            var entityFileOrAttributeAttributeLogicalName = _configuration["DV:Tables:Plantillas:File"];
            string customEntitySetName = _configuration["Dataverse:Tables:Plantillas"];
            var url = new Uri(_configuration["Dataverse:api"] + $"{customEntitySetName}({templateId})/{entityFileOrAttributeAttributeLogicalName}/$value?size=full");
            var token = await _generateToken.GetToken();
            var Client = new HttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await Client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }

        }

        public async Task<string> GetTemplate(string entityId)
        {
            var entityFileOrAttributeAttributeLogicalName = _configuration["DV:Tables:Plantillas:File"];
            string tableName = _configuration["Dataverse:Tables:Plantillas"];
            var downloaded = await _dataverseService.GetFileBytes(tableName,  entityId,  entityFileOrAttributeAttributeLogicalName);
            var stream = new StreamReader(new MemoryStream(downloaded.ByteArray));
            var plantilla = stream.ReadToEnd();
            return plantilla;
        }

    }
}
