using System;
using System.Threading.Tasks;
using FunctionApp1.CheckSubscuriptionSpend;
using Microsoft.Azure;
using Microsoft.Rest;

namespace FunctionApp1.ShutDownVmsInSubscription
{
    static class Extensions
    {
        public static async Task<Tuple<TokenCredentials, SubscriptionCloudCredentials>> GetSubscriptionCredentials(this Tuple<string, string, string, string> @this) =>
            GetCredentials(@this.Item4, await new Tuple<string, string, string>(@this.Item1, @this.Item2, @this.Item3).GetAccessToken());

        private static Tuple<TokenCredentials, SubscriptionCloudCredentials> GetCredentials(string subscriptionId, string token) =>
            new Tuple<TokenCredentials, SubscriptionCloudCredentials>(
                new TokenCredentials(token) { } ,
                new TokenCloudCredentials(subscriptionId, token) );
    }
}
