using System.Collections.Generic;

namespace FunctionApp1.CheckSubscuriptionSpend.Model
{
    public class UsagePayload
    {
        public List<UsageAggregate> Value { get; set; }
        public string NextLink { get; set; }
    }

    public class UsageAggregate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Properties Properties { get; set; }
    }

    public class InfoFields
    {
        public string MeteredRegion { get; set; }
        public string MeteredService { get; set; }
        public string Project { get; set; }
        public string MeteredServiceType { get; set; }
        public string ServiceInfo1 { get; set; }
    }

    public class Properties
    {
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }

        public string SubscriptionId { get; set; }
        public string UsageStartTime { get; set; }
        public string UsageEndTime { get; set; }
        public string MeterId { get; set; }

        public double Quantity { get; set; }
        public string Unit { get; set; }

        public InfoFields InfoFields { get; set; }

    }

}
