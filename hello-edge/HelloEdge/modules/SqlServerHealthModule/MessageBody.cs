using System;
using Newtonsoft.Json;

namespace SqlServerHealthModule
{
    public class MessageBody
    {
        [JsonProperty("healthStatus")]
        public HealthStatus HealthStatus { get; set; }

        [JsonProperty("timeCreated")]
        public DateTime TimeCreated { get; set; }
    }

    public enum HealthStatus
    {
        Healthy = 0,
        Unhealthy
    }
}
