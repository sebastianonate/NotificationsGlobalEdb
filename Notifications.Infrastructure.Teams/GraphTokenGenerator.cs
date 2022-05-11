using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Notifications.Infrastructure.Logs;
using Notifications.Infrastructure.SystemNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Teams
{
    public class GraphTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly LogService<GraphTokenGenerator> _logger;
        private string _token;
        private DateTimeOffset _expirationTimeToken;
        private readonly IServiceProvider _serviceProvider;
        public GraphTokenGenerator(
            IConfiguration configuration,
            LogService<GraphTokenGenerator> logger,
            IServiceProvider serviceProvider
            )
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetToken() {
            if (string.IsNullOrWhiteSpace(_token))
            {
                var authResult = await GenerateToken();
                _token = authResult.AccessToken;
                _expirationTimeToken = authResult.ExpiresOn;
            }

            if (DateTimeOffset.UtcNow.ToLocalTime() >= _expirationTimeToken.ToLocalTime())
            {
                var authResult = await GenerateToken();
                _token = authResult.AccessToken;
                _expirationTimeToken = authResult.ExpiresOn;
            }

            return _token;
        }

        private async Task<AuthenticationResult> GenerateToken()
        {
            Console.WriteLine("Generando token graph");
            string email = _configuration["TeamsAccount:Email"];
            string password = _configuration[_configuration["TeamsAccount:PassSecretNameOnAKV"]];
            SecureString securePassword = new SecureString();
            password.ToList().ForEach(x => securePassword.AppendChar(x));

            var app = PublicClientApplicationBuilder.Create(_configuration["AzureAd:ClientId"])
                                          .WithAuthority(new Uri(_configuration["AzureAd:Authority"]))
                                          .Build();
            var _scopesGraph = new string[] { _configuration["GraphScopes:Scopes"] };

            try
            {
                var responseForGraphAuth = await app.AcquireTokenByUsernamePassword(_scopesGraph, email, securePassword).ExecuteAsync();
                return responseForGraphAuth;
            }
            catch (Exception ex) {
                using (var scope = _serviceProvider.CreateScope()) {
                    var _systemNotificationsService = scope.ServiceProvider.GetRequiredService<SystemNotificationsService>();
                    await _systemNotificationsService.SendAsync("Error al obtener token de graph", ex.Message);
                }
                _logger.LogError("Error al obtener token de graph", ex.Message);
                throw;
            }
        }
    }

   
}
