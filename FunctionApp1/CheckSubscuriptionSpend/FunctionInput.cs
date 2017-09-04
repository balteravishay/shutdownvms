using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;

namespace FunctionApp1.CheckSubscuriptionSpend
{
    class FunctionInput
    {
        private FunctionInput() { }
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string OfferNumber { get; set; }
        public string ArmBillingService { get; set; }
       
        public int MonthSpendLimit { get; set; }
        public int CurrentMonth { get; set; }

        public static FunctionInput GetInput(TraceWriter log)
        {
            var applicationId = System.Configuration.ConfigurationManager.AppSettings["ApplicationId"];
            var applicationSecret = System.Configuration.ConfigurationManager.AppSettings["ApplicationSecret"];
            var tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantId"];
            string subscriptionId = System.Configuration.ConfigurationManager.AppSettings["SubscriptionId"];
            string offerNumber = System.Configuration.ConfigurationManager.AppSettings["OfferNumber"];
            string armBillingService = System.Configuration.ConfigurationManager.AppSettings["ArmBillingService"];
            var spendMapString = System.Configuration.ConfigurationManager.AppSettings["SpendMap"];
            if (!VerifyInput(applicationId, applicationSecret, tenantId,
                subscriptionId, offerNumber, armBillingService, spendMapString, log))
                throw new ArgumentException("Invalid arguments for function, see log for details");
            log.Info($"applicationId:{applicationId}; applicationSecret:{applicationSecret}; tenantId:{tenantId}; SubscriptionId:{subscriptionId};");

            var spendMap = spendMapString.Split(',');
            if (spendMap.Length != 12)
            {
                log.Error($"C# Timer trigger function input for spendMap is invalid");
                throw new ArgumentException("Invalid arguments for function, see log for details");
            }
            var currentMonth = CalcCurrentMonth();
            var spendMaArray = spendMap.Select(int.Parse).ToArray();
            var allowedSpend = spendMaArray[currentMonth - 1] <= 0 ? 1 : spendMaArray[currentMonth - 1];

            return new FunctionInput()
            {
                ApplicationId = applicationId,
                ApplicationSecret = applicationSecret,
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                OfferNumber = offerNumber,
                ArmBillingService = armBillingService,
                MonthSpendLimit = allowedSpend,
                CurrentMonth = currentMonth
            };
        }

        private static int CalcCurrentMonth()
        {
            var currentMonth = DateTime.Now.Month;
            // if this is the first day of month we calculate the previous month
            if (DateTime.Now.Day == 1)
                currentMonth--;
            if (currentMonth == 0)
                currentMonth = 12;
            return currentMonth;
        }

        private static bool VerifyInput(string applicationId, string applicationSecret, string tenantId,
            string subscriptionId, string offerNumber,
            string armBillingService, string spendMap, TraceWriter log)
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
            if (string.IsNullOrEmpty(offerNumber))
            {
                log.Error($"offerNumber is invalid");
                return false;
            }
            
            if (string.IsNullOrEmpty(armBillingService))
            {
                log.Error($"armBillingService is invalid");
                return false;
            }

            if (string.IsNullOrEmpty(spendMap))
            {
                log.Error($"spendMap is invalid");
                return false;
            }
            return true;
        }
    }
}
