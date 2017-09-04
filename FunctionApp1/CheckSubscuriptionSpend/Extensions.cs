using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace FunctionApp1.CheckSubscuriptionSpend
{
    static class Extensions
    {
        public static async Task<string> GetAccessToken(this Tuple<string, string, string> @this)
        {
            var cred = await new AuthenticationContext($"https://login.windows.net/{@this.Item1}")
                .AcquireTokenAsync("https://management.azure.com/",
                    new ClientCredential(@this.Item2, @this.Item3));
            return cred.AccessToken;
        }
            
        
    }
}
