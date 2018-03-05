using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;

namespace Vehicle.Display
{
    public static class EventToDisplay
    {
        [FunctionName("EventToDisplay")]
        public static void Run(
            [EventGridTrigger]EventGridEvent vehicleEvent,
             TraceWriter log
            )
        {
            log.Info($"EventGrid trigger fired: {vehicleEvent.EventType}");
        }
    }
}
