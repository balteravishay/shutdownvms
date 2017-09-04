using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace FunctionApp1.ShutDownVmsInSubscription
{
    class FunctionInput
    {
        private FunctionInput()
        {

        }
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }

        public HashSet<string> ResourceGroupNameFilter { get; set; }

        public static FunctionInput GetInput(TraceWriter log)
        {
            var applicationId = ConfigurationManager.AppSettings["ApplicationId"];
            var applicationSecret = ConfigurationManager.AppSettings["ApplicationSecret"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];
            string subscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            string resourceGroupNameFilter = ConfigurationManager.AppSettings["ResourceGroupNameFilter"];

            if (!VerifyInput(applicationId, applicationSecret, tenantId,
                subscriptionId, log))
                throw new ArgumentException("Invalid arguments for function, see log for details");
            var resourceGroupNameFilterArray = resourceGroupNameFilter == null ? new HashSet<string>() : new HashSet<string>(resourceGroupNameFilter.Split(','));

            log.Info($"applicationId:{applicationId}; applicationSecret:{applicationSecret}; tenantId:{tenantId}; SubscriptionId:{subscriptionId};");
            return new FunctionInput()
            {
                ApplicationId = applicationId,
                ApplicationSecret = applicationSecret,
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                ResourceGroupNameFilter = resourceGroupNameFilterArray
            };
        }

        private static bool VerifyInput(string applicationId, string applicationSecret, string tenantId, string subscriptionId, TraceWriter log)
        {
            if (string.IsNullOrEmpty(applicationId))
            {
                log.Error($"applicatioId is invalid");
                return false;
            }
            if (string.IsNullOrEmpty(applicationSecret))
            {
                log.Error($"applicationSecret is invalid");
                return false;
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                log.Error($"tenantId is invalid");
                return false;
            }
            if (string.IsNullOrEmpty(subscriptionId))
            {
                log.Error($"SubscriptionId is invalid");
                return false;
            }
            return true;
        }
    }
}
