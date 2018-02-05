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
            CloudBlobClient serviceClient = account.CreateCloudBlobClient();
            var container = serviceClient.GetContainerReference("plates");

            var images = Directory.GetFiles("images");
            for(int x = 0; x < 100; x++)
            {
                string filename = x + ".jpg";
                var blob = container.GetBlockBlobReference(filename);
                Task.Run(async () => await blob.UploadFromFileAsync(images[0])).Wait();
                Console.WriteLine("uploaded: " + filename);
            }
            Console.ReadLine();
        }

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=eventgridwebcast;AccountKey=H4qeMVTbYJ4LbMlq0x1ACBhaHd39KU4Q7cjpHH1GMwPGor52Y4AyaRDxQbSw72uLNF8iWXB5k5DRzSH/I0Lujg==;BlobEndpoint=https://eventgridwebcast.blob.core.windows.net/;QueueEndpoint=https://eventgridwebcast.queue.core.windows.net/;TableEndpoint=https://eventgridwebcast.table.core.windows.net/;FileEndpoint=https://eventgridwebcast.file.core.windows.net/;";
    }
}
