using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EventGridWebcast
{
    public static class ImageTrigger
    {
        private static StorageBlobCreatedEventData eventData;

        [FunctionName("ImageTrigger")]
        public static async Task Run(
            [EventGridTrigger]Microsoft.Azure.WebJobs.Extensions.EventGrid.EventGridEvent imageEvent,
            TraceWriter log
            )
        {
            log.Info($"EventGrid trigger fired: {imageEvent.EventType}");
            eventData = imageEvent.Data.ToObject<StorageBlobCreatedEventData>();
            var imageStream = await StreamImageContent(eventData.Url);

            
            // Analyze the plate data
            var plate = await AnalyzeImageAsync(imageStream, log);

            if(!String.IsNullOrEmpty(plate)) {
            // Emit a "Vehicle Entered" alert
                await EmitEventAsync(
                    type: "VehicleEntered/Recognized", 
                    subject: $"Garage{garageId}",
                    data: new 
                        {   plateNumber = plate,
                            garageId = garageId,
                            gateNumber = gateNumber
                        });
            }
        }

        internal static async Task<string> AnalyzeImageAsync(Stream content, TraceWriter log)
        {
            // Call the Computer Vision API
            var result = await client.SendAsync(AnalyzeImageRequest(content));

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    // Try to pull out detected plate
                    dynamic ocrObject = await result.Content.ReadAsAsync<object>();
                    string plate = ocrObject["regions"][0]["lines"][1]["words"][0]["text"];
                    return plate;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // If it can't find it, send event to review
                    await EmitEventAsync(
                        type: "VehicleEntered/Unrecognized", 
                        subject: new Uri(eventData.Url).PathAndQuery, 
                        data: new { blob = eventData.Url });
                    return null;
                }
            }
            else { return "Got status code " + result.StatusCode; }
        }



        private static async Task EmitEventAsync(string type, string subject, dynamic data)
        {
            var events = new List<Microsoft.Azure.EventGrid.Models.EventGridEvent>
            {
                new Microsoft.Azure.EventGrid.Models.EventGridEvent
                {
                    EventType = type,
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Subject = subject,
                    Data = data
                }
            };


            var request = new HttpRequestMessage(HttpMethod.Post, eventGridUrl);
            request.Headers.Add("aeg-sas-key", eventGridKey);
            request.Content = new ObjectContent<List<Microsoft.Azure.EventGrid.Models.EventGridEvent>>(
                    events,
                    formatter: new System.Net.Http.Formatting.JsonMediaTypeFormatter(),
                    mediaType: "application/json");
            var result = await client.SendAsync(request);
        }
        private static HttpRequestMessage AnalyzeImageRequest(Stream content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, visionApiUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", visionApiKey);
            request.Content = new StreamContent(content);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            return request;
        }

        private static async Task StoreLicensePlateData(string uri, string plate)
        {
            var cb = new CloudBlob(new Uri(uri.Replace(".jpg", "-result.txt")));
            CloudBlockBlob blob = blobClient.
                GetContainerReference(cb.Container.Name).
                GetBlockBlobReference(cb.Name);
            await blob.UploadTextAsync(plate);
            
        }

        private static async Task<Stream> StreamImageContent(string uri)
        {
            var blob = await blobClient.GetBlobReferenceFromServerAsync(new Uri(uri));
            return await blob.OpenReadAsync(null, null, null);
        }

        private static HttpClient client = new HttpClient();
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        private static string visionApiUrl = Environment.GetEnvironmentVariable("visionApiUrl");
        private static string visionApiKey = Environment.GetEnvironmentVariable("visionApiKey");
        private static string eventGridUrl = Environment.GetEnvironmentVariable("eventGridUrl");
        private static string eventGridKey = Environment.GetEnvironmentVariable("eventGridKey");
        private static string garageId = "2";
        private static string gateNumber = "1";
    }
}
