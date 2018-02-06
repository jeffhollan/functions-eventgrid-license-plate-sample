using System;
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
            
            Parallel.For(0, 1000, 
                async s =>
            {
                await queue.AddMessageAsync(
                    new Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage(
                        "{ \"index\": " + s + " }"
                        ));
            });
            Console.ReadLine();
        }

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=eventgridwebcast;AccountKey=H4qeMVTbYJ4LbMlq0x1ACBhaHd39KU4Q7cjpHH1GMwPGor52Y4AyaRDxQbSw72uLNF8iWXB5k5DRzSH/I0Lujg==;BlobEndpoint=https://eventgridwebcast.blob.core.windows.net/;QueueEndpoint=https://eventgridwebcast.queue.core.windows.net/;TableEndpoint=https://eventgridwebcast.table.core.windows.net/;FileEndpoint=https://eventgridwebcast.file.core.windows.net/;";
        
    }
}
