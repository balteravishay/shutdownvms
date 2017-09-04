using FunctionApp1.CheckSubscuriptionSpend.Model;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;

namespace FunctionApp1.CheckSubscuriptionSpend.ExternalApis
{
    class RateCardPayload: RestResponsePayLoad
    {
        private RateCardPayload()
        {

        }

        public List<object> OfferTerms { get; set; }
        public List<Resource> Meters { get; set; }
        public string Currency { get; set; }
        public string Locale { get; set; }
        public string RatingDate { get; set; }
        public bool IsTaxIncluded { get; set; }

        public static RateCardPayload GetRateCardPayLoad(string token,
            string armBillingService, string subscriptionId, string subscriptionType, TraceWriter log) =>
            GetResponse<RateCardPayload>(BuildRquest(armBillingService,
                subscriptionId,
                $"providers/Microsoft.Commerce/RateCard?api-version=2016-08-31-preview&$filter=OfferDurableId eq '{subscriptionType}' and Currency eq 'USD' and Locale eq 'en-US' and RegionInfo eq 'US'",
                token), "RateCard", log);
    }
}
