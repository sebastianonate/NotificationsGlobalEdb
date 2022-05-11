using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notifications.Core;
using Notifications.Infrastructure.Logs;
using System.Net.Http.Headers;
using System.Text;

namespace Notifications.Infrastructure.Dataverse
{
    public class DataverseService
    {
        private readonly HttpClient client;
        private readonly IConfiguration _configuration;
        private readonly GenerateDataverseToken _generateDataverseToken;
        private string _tableName;
        public readonly string TableName;
        private readonly LogService<DataverseService> _logger;
        public DataverseService(
            IConfiguration configuration, 
            GenerateDataverseToken generateDataverseToken,
            LogService<DataverseService> logger)
        {
            _configuration = configuration;
            _generateDataverseToken = generateDataverseToken;
            _logger = logger;
            client = new HttpClient
            {
                BaseAddress = new Uri(_configuration["Dataverse:Api"]),
                Timeout = new TimeSpan(0, 2, 0)    // Standard two minute timeout on web service calls.
            };
            var token = generateDataverseToken.GetToken().Result;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<JToken>> GetDataFromTable(string tableName) {

            var response = await client.GetAsync(tableName);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Obtener datos de tabla", response.ReasonPhrase);
                return new List<JToken>();
            }
            var content = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            return content["value"].ToList();
        }

        public async Task<JObject> GetRegisterById(string tablaName, Guid RegisterId) {
            //cr942_solucioneses(83e44731-4e58-ec11-8f8f-002248384c5b)
            string uri = $"{tablaName}({RegisterId})";
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

            _logger.LogError("Obtener registro por Id", response.ReasonPhrase);
            return null;
        }

        public async Task<HttpResponseMessage> DeleteRegisterById(string tablaName, Guid RegisterId)
        {
            //cr942_solucioneses(83e44731-4e58-ec11-8f8f-002248384c5b)
            string uri = $"{tablaName}({RegisterId})";
            var response = await client.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
                _logger.LogError("Eliminar registro", response.ReasonPhrase);
            return response;
        }

        public async Task<HttpResponseMessage> SaveFilesAttachment(byte[] fileBytes, string fileName, string tableName,string registerID, string fieldName)
        {
            string token = await _generateDataverseToken.GetToken();
            var urlx = new Uri(_configuration["Dataverse:api"] + $"{tableName}({registerID})/{fieldName}");
            StreamContent streamContext;

            using (var ms = new MemoryStream())
            {
                var memory = new MemoryStream(fileBytes);
                streamContext = new StreamContent(memory);
            }
            
            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), urlx))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = streamContext;
                request.Content.Headers.Add("Content-Type", "application/octet-stream");
                request.Content.Headers.Add("x-ms-file-name", fileName);
                
                using (var respuesta = await client.SendAsync(request))
                 {
                    respuesta.EnsureSuccessStatusCode();
                    return respuesta;
                }
            }
        }

        public async Task<HttpResponseMessage> RegistarDatos(string tableName,JObject contentCreate)
        {
            JObject x = new JObject();
            HttpContent content = new StringContent(contentCreate.ToString(), UnicodeEncoding.UTF8, "application/json");
            HttpResponseMessage httpResponseMessageCreate = await client.PostAsync(tableName, content);
            return httpResponseMessageCreate;
        }

        public async Task<bool> SolutionExist(Guid solutionId) 
        {
            var response = await GetRegisterById(_configuration["Dataverse:Tables:Soluciones"], solutionId);
            return response != null;
        }

        public bool SolutionExist(JObject solution)
        {
            return solution != null;
        }
        public async Task<JObject> GetSolution(Guid solutionId)
        {
            var response = await GetRegisterById(_configuration["Dataverse:Tables:Soluciones"], solutionId);
            return response;
        }

        public async Task<FileResponse> GetFileBytes(string tableName, string entityId, string entityFileOrAttributeAttributeLogicalName)
        {
            var url = new Uri(_configuration["Dataverse:api"] + $"{tableName}({entityId})/{entityFileOrAttributeAttributeLogicalName}/$value?size=full");
            //_configuration["Dataverse:ClientId"]
            string namefile;
            var increment = 4194304;
            var from = 0;
            var fileSize = 0;
            byte[] downloaded = null;
            var Client = new HttpClient();
            var token = await _generateDataverseToken.GetToken();
            do
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(from, from + increment - 1);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    using (var response = await Client.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                            return null;
                        if (downloaded == null)
                        {
                            fileSize = int.Parse(response.Headers.GetValues("x-ms-file-size").First());
                            downloaded = new byte[fileSize];
                        }
                         namefile = response.Content.Headers.ContentDisposition.FileName;
                        var responseContent = await response.Content.ReadAsByteArrayAsync();
                        responseContent.CopyTo(downloaded, from);
                    }
                    
                }
               
                from += increment;
            } while (from < fileSize);
            FileResponse fileResponse = new FileResponse(downloaded, namefile, entityId);
            return fileResponse;
        }

        public string GetAttributeName(string table, string property)
        {
            return _configuration[$"DV:Tables:{table}:{property}"];
        }

    }


    public record FileInTableDataVerse (string TableName, string RegisterId, string FieldName);

}