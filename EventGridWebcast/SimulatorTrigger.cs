using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace EventGridWebcast
{
    public static class SimulatorTrigger
    {
        [FunctionName("SimulatorTrigger")]
        public static async Task RunAsync(
            [QueueTrigger("plates")]QueueItem queueItem,
            [Blob("sample/Plate1.jpg", FileAccess.Read)] Stream inBlob,
            [Blob("plates/{index}.jpg", FileAccess.Write)] Stream outBlob,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {queueItem}");
            await inBlob.CopyToAsync(outBlob);
        }

        public class QueueItem
        {
            public int index { get; set; }
        }
    }
}
