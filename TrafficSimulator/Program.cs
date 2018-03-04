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

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=eventgridwebcast2;AccountKey=YH60RxZOcZs2tmgMpD02fikPQHCWSUtQ89iBHkmsx7HneoZf8SPfun2XE85yS98WPxEaeqpzHXc488x0WFa9Bg==;BlobEndpoint=https://eventgridwebcast2.blob.core.windows.net/;QueueEndpoint=https://eventgridwebcast2.queue.core.windows.net/;TableEndpoint=https://eventgridwebcast2.table.core.windows.net/;FileEndpoint=https://eventgridwebcast2.file.core.windows.net/;";
        
    }
}
