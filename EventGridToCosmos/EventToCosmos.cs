
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Azure.Documents;

namespace Vehicle.Process
{
    public static class EventToCosmos
    {
        [FunctionName("EventToCosmos")]
        public static void Run(
            [EventGridTrigger]EventGridEvent vehicleEvent,
            [CosmosDB("vehicles", "gate", ConnectionStringSetting = "CosmosDbConnectionString", Id = "{rand-guid}")] out dynamic document,
            TraceWriter log
            )
        {
            log.Info($"EventGrid trigger fired: {vehicleEvent.EventType}");
            document = vehicleEvent.Data;
        }
    }
}
