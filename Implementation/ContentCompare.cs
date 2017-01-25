using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace Implementation
{
    public static class ContentCompare
    {
        public static IHttpClient HttpClient { get; set; } = new HttpClient();

        public static async Task Run(TextReader inputFile, TextWriter outputFile, TraceWriter log)
        {
            var contents = JsonConvert.DeserializeObject<Content[]>(await inputFile.ReadToEndAsync());

            try
            {
                foreach (var content in contents)
                {
                    try
                    {
                        content.content = await ProcessContent(content);
                    }
                    catch (Exception e)
                    {
                        log.Error("Fail to process content", e);
                    }
                }
            }
            finally
            {
                await outputFile.WriteAsync(JsonConvert.SerializeObject(contents, Formatting.Indented));
            }
        }

        private static async Task<string> ProcessContent(Content content)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = content.url;
            var document = await BrowsingContext.New(config).OpenAsync(address);

            var selectedContent = SelectContent(document, content.selector);

            if (selectedContent == content.content) return content.content;

            var name = SelectName(document, content.titleSelector);

            await SendNotifications(content, name, selectedContent);
            
            return selectedContent;
        }

        private static async Task SendNotifications(Content content, string name, string selectedContent)
        {
            foreach (var url in content.push)
            {
                await HttpClient.PostStringAsync(url,
                    $"{name} content changed from {content.content} to {selectedContent}");
            }
        }

        public static string SelectContent(IDocument document, string priceSelector)
        {
            var priceSpan = document.QuerySelectorAll(priceSelector);
            return priceSpan.First().TextContent.Trim();
        }

        public static string SelectName(IDocument document, string titleSelector)
        {
            var heading = document.QuerySelectorAll(titleSelector);
            return heading.First().TextContent.Trim();
        }
    }

    public class Content
    {
        public string url { get; set; }
        public string content { get; set; }
        public string selector { get; set; }
        public string titleSelector { get; set; }
        public string[] push { get; set; }
    }
}