using FunctionApp1.CheckSubscuriptionSpend.Model;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace FunctionApp1.CheckSubscuriptionSpend.ExternalApis
{
    class UsagePayloads : RestResponsePayLoad, IEnumerable<UsagePayload>
    {
        private readonly IEnumerable<UsagePayload> _usagePayloads;

        private UsagePayloads(IEnumerable<UsagePayload> usagePayloads)
        {
            _usagePayloads = usagePayloads;
        }
        public IEnumerator<UsagePayload> GetEnumerator() => _usagePayloads.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _usagePayloads.GetEnumerator();

        public static UsagePayloads GetUsagePayLoads(string token,
            string armBillingService, string subscriptionId, TraceWriter log, int currentMonth)
        {
            var list = new List<UsagePayload>();
            var resObj = GetResponse<UsagePayload>(BuildRquest(armBillingService,
                subscriptionId,
                $"providers/Microsoft.Commerce/UsageAggregates?api-version=2015-06-01-preview&reportedstartTime={CalcStartReportTime(currentMonth)}&reportedEndTime={CalcNowTime()}",
                token), "Usage", log);
            while (!string.IsNullOrEmpty(resObj.NextLink))
            {
                list.Add(resObj);
                resObj = GetResponse<UsagePayload>(BuildRquest(resObj.NextLink, token), "Usage", log);
            }
            list.Add(resObj);
            return new UsagePayloads(list);
        }

        private static object CalcNowTime() => ParseDateString(DateTime.Today);

        private static string CalcStartReportTime(int startReportMonth)
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, startReportMonth, 1);
            if (startDate > now)
                startDate = new DateTime(now.Year - 1, startReportMonth, 1);
            return ParseDateString(startDate);
        }

        private static string ParseDateString(DateTime date) => HttpUtility.UrlEncode(date.ToString("u"));

    }
}
