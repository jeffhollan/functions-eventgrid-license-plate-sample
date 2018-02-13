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

namespace EventGridWebcast
{
    public static class ImageTrigger
    {
        private static StorageBlobCreatedEventData imageData;
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
            imageData = imageEvent.Data.ToObject<StorageBlobCreatedEventData>();

            // Analyze the plate data
            var plate = await AnalyzeImageAsync(inBlob, log);

            // Create output blob with results
            new MemoryStream(Encoding.UTF8.GetBytes(plate)).CopyTo(outBlob);
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
                    await EmitEventAsync(imageData.Url);
                    return "Manual review initiated";
                }
            }
            else { return "Got status code " + result.StatusCode; }
        }



        private static async Task EmitEventAsync(string url)
        {
            var events = new List<Microsoft.Azure.EventGrid.Models.EventGridEvent>
            {
                new Microsoft.Azure.EventGrid.Models.EventGridEvent
                {
                    EventType = "Plates/Unrecognized",
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString(),
                    Subject = new Uri(url).PathAndQuery,
                    Data = new { blob = url }
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

        private static string visionApiUrl = Environment.GetEnvironmentVariable("visionApiUrl");
        private static string visionApiKey = Environment.GetEnvironmentVariable("visionApiKey");
        private static string eventGridUrl = Environment.GetEnvironmentVariable("eventGridUrl");
        private static string eventGridKey = Environment.GetEnvironmentVariable("eventGridKey");
    }
}
