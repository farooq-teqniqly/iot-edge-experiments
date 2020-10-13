using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SqlServerHealthModule
{
    public class MessageFactory
    {
        public Message CreateMessage(HealthStatus healthStatus)
        {
            var messageBody = new MessageBody {HealthStatus = healthStatus, TimeCreated = DateTime.UtcNow};
            var messageJson = JsonConvert.SerializeObject(messageBody);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var message = new Message(messageBytes) {ContentEncoding = "UTF-8", ContentType = "application/json"};

            return message;
        }
    }
}
