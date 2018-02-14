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

        private static string connectionString = "{Your connection string here}";
        
    }
}
