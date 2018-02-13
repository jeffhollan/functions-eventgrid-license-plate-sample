using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TrafficSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            var serviceClient = account.CreateCloudQueueClient();
            var queue = serviceClient.GetQueueReference("plates");
            List<Task> tasks = new List<Task>();

            Console.WriteLine("Simulating cars....");
            for(int x = 0; x < 1000; x++)
            {
                tasks.Add(queue.AddMessageAsync(
                    new Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage(
                        "{ \"index\": " + x + " }"
                        )));
            }
            Task.WhenAll(tasks).Wait();
            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=eventgridwebcast;AccountKey=H4qeMVTbYJ4LbMlq0x1ACBhaHd39KU4Q7cjpHH1GMwPGor52Y4AyaRDxQbSw72uLNF8iWXB5k5DRzSH/I0Lujg==;BlobEndpoint=https://eventgridwebcast.blob.core.windows.net/;QueueEndpoint=https://eventgridwebcast.queue.core.windows.net/;TableEndpoint=https://eventgridwebcast.table.core.windows.net/;FileEndpoint=https://eventgridwebcast.file.core.windows.net/;";
        
    }
}
