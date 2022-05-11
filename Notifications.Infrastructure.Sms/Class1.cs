using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Notifications.Infrastructure.Sms
{
    public record SmsBulk
    {
        public string numero { get; set; }
        public string sms { get; set; }

    }

    public record SendSmsBulkRequest
    {
        public List<SmsBulk> bulk { get; set; }
    }

    public record SendSmsIndividual {
        public string toNumber { get; set; }
        public string sms { get; set; }
    }

    public class SmsService {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SmsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration["Sms:Api"]),
                Timeout = new TimeSpan(0, 2, 0)    // Standard two minute timeout on web service calls.
            };
            _httpClient.DefaultRequestHeaders.Add("token", _configuration["Sms:Token"]);
            _httpClient.DefaultRequestHeaders.Add("account", _configuration["Sms:Account"]);
            _httpClient.DefaultRequestHeaders.Add("apiKey", _configuration["Sms:ApiKey"]);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> SendSmsBulk(SendSmsBulkRequest request)
        {
            var dataString = JsonConvert.SerializeObject(request);
            HttpContent content = new StringContent(dataString, UnicodeEncoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("bulk", content);
            return response;
        }

        public async Task<HttpResponseMessage> SendSms(SendSmsIndividual request)
        {
            var dataString = JsonConvert.SerializeObject(request);
            HttpContent content = new StringContent(dataString, UnicodeEncoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("", content);
            return response;
        }
    }
    
}