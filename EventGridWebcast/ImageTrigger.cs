using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using System.IO;
using System.Text;
using System;

namespace EventGridWebcast
{
    public static class ImageTrigger
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("ImageTrigger")]
        public static async Task Run(
            [EventGridTrigger]Microsoft.Azure.WebJobs.Extensions.EventGrid.EventGridEvent imageEvent,
            [Blob("{data.url}", FileAccess.Read)] Stream inBlob,
            [Blob("{data.url}-license.txt", FileAccess.Write)] Stream outBlob,
            TraceWriter log
            )
        {
            log.Info($"EventGrid trigger fired: {imageEvent.EventType}");

            var imageData = imageEvent.Data.ToObject<StorageBlobCreatedEventData>();
            log.Info($"Got storage event created: {imageData.Url}");
            var plate = await AnalyzeImageAsync(inBlob);
            log.Info($"Plate is {plate}");
            new MemoryStream(Encoding.UTF8.GetBytes(plate)).CopyTo(outBlob);
        }

        internal static async Task<string> AnalyzeImageAsync(Stream content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, visionApiUrl);
            request.Headers.Add("Ocp-Apim-Subscription-Key", visionApiKey);
            request.Content = new StreamContent(content);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var result = await client.SendAsync(request);
            dynamic ocrObject = await result.Content.ReadAsAsync<object>();
            return ocrObject["regions"][0]["lines"][1]["words"][0]["text"];
        }

        private static string visionApiUrl = Environment.GetEnvironmentVariable("visionApiUrl");
        private static string visionApiKey = Environment.GetEnvironmentVariable("visionApiKey");
    }
}
