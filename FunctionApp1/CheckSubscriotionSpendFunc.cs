using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using FunctionApp1.CheckSubscuriptionSpend.ExternalApis;
using FunctionApp1.CheckSubscuriptionSpend.Model;
using FunctionApp1.CheckSubscuriptionSpend;

namespace FunctionApp1
{
    public static class CheckSubscriotionSpendFunc
    {
        [FunctionName("CheckSubscriptionSpendFunc")]
        public static async Task<string> Run([TimerTrigger("0 30 1 * * *")]TimerInfo myTimer, TraceWriter log)
        //public static async Task<string> Run([TimerTrigger("* * * * * 1")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            var input = FunctionInput.GetInput(log);
            // get the token
            string token;
            try
            {
                token = await new Tuple<string, string, string>(input.TenantId, input.ApplicationId, input.ApplicationSecret).GetAccessToken();
            }
            catch (Exception e)
            {
                log.Error($"Error retrieving access token to subscription {e.Message}");
                return null;
            }
            log.Info($"token: {token}");

            return BuildCurrentSpendObject(token, input, log);
        }

        private static string BuildCurrentSpendObject(string token, FunctionInput input, TraceWriter log)
        {
            try
            {
                var currentMonthSpend = 
                    SpendDuringCurrentMonth(token,
                    input.ArmBillingService,
                    input.SubscriptionId,
                    input.OfferNumber,
                    log,
                    input.CurrentMonth);

                var percentOfSpend = (int)((currentMonthSpend / input.MonthSpendLimit) * 100);

                if (percentOfSpend >= 100)
                {
                    log.Info("sending over budget");
                    return JsonConvert.SerializeObject(
                        new
                        {
                            over_budget = true,
                            current_spend = currentMonthSpend,
                            percent_of_spend = percentOfSpend
                        });
                }

                else if (percentOfSpend >= 70)
                {
                    log.Info("sending over 70%");
                    return JsonConvert.SerializeObject(
                        new
                        {
                            over_budget = false,
                            current_spend = currentMonthSpend,
                            percent_of_spend = percentOfSpend
                        });
                }

                else
                {
                    log.Info("not sending. spend is at " + percentOfSpend);
                    return null;
                }
            }
            catch (Exception e)
            {
                log.Error($"Error retrieving budget details for subscription: {e.Message}");
                return null;
            }
        }

        private static double SpendDuringCurrentMonth(string token, string armBillingService,
    string subscriptionId, string subscriptionType, TraceWriter log, int currentMonth) =>
   Math.Round(
       Calculate(UsagePayloads.GetUsagePayLoads(token, armBillingService, subscriptionId, log, currentMonth),
        RateCardPayload.GetRateCardPayLoad(token, armBillingService, subscriptionId, subscriptionType, log),
        log), 1);

        private static double Calculate(IEnumerable<UsagePayload> usagePayload, RateCardPayload rateCardPayload, TraceWriter log)
        {
            double totalSpend = 0;
            if (usagePayload == null || rateCardPayload == null)
                return totalSpend;
            foreach (var usageGroup in usagePayload.SelectMany(usage => usage.Value).GroupBy(usage => $"{usage.Properties.MeterCategory} {usage.Properties.MeterSubCategory}"))
            {
                double typeSpend = 0;
                foreach (var usage in usageGroup)
                {
                    var meter = rateCardPayload.Meters.FirstOrDefault(iterMeter => iterMeter.MeterId.Equals(usage.Properties.MeterId));
                    if (meter != null)
                    {
                        typeSpend += CalcMeterPerUsage(usage, meter, log);
                    }
                    else
                    {
                        log.Info($"could not find meter for usage: {usage.Name}");
                    }
                }
                log.Info($"total spend in month on {usageGroup.Key} is {typeSpend}");
                totalSpend += typeSpend;
            }
            log.Info($"total spend in month {totalSpend}");
            return totalSpend;
        }

        private static double CalcMeterPerUsage(UsageAggregate usage, Resource meter, TraceWriter log) =>
            usage.Properties.Quantity * meter.MeterRates[0];

    }
}