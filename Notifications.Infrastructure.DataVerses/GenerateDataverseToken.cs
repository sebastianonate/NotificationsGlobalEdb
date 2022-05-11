using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Dataverse
{
   public  class GenerateDataverseToken
    {

        public string token;
        private DateTimeOffset expirationTimeToken;
        private readonly IConfiguration _configuration;
        public GenerateDataverseToken(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GetToken() {

            if (string.IsNullOrWhiteSpace(token)) {
                var authResult = await GenerateTokens();
                token = authResult.AccessToken;
                expirationTimeToken = authResult.ExpiresOn;
            }

            if (DateTimeOffset.UtcNow.ToLocalTime() >= expirationTimeToken)
            {
                var authResult = await GenerateTokens();
                token = authResult.AccessToken;
                expirationTimeToken = authResult.ExpiresOn;
            }

            return token;
        }
        private   async Task<AuthenticationResult> GenerateTokens()
        {
            Console.WriteLine("Estoy generando nuevo token");
            ClientCredential credentials = new ClientCredential(_configuration["Dataverse:ClientId"], _configuration["Dataverse:ClientSecret"]);
            var authContext = new AuthenticationContext(_configuration["Dataverse:Authority"]);
            var result = await authContext.AcquireTokenAsync(_configuration["Dataverse:Url"], credentials);
            //Console.WriteLine(result.AccessToken);
            return result;
        }
    }
}
