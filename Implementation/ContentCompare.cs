using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Newtonsoft.Json;

namespace Implementation
{
    public static class ContentCompare
    {
        public static IHttpClient HttpClient { get; set; } = new HttpClient();

        public static async Task Run(TextReader inputFile, TextWriter outputFile)
        {
            var url =
                "https://prod-06.australiasoutheast.logic.azure.com:443/workflows/904acfab0cc041ea94af1a98867cc9f2/triggers/manual/run?api-version=2015-08-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pYfItovg26uxQXJVXFHEGZcGABDZjKf-xVcVZhvBulY";

            var contents = JsonConvert.DeserializeObject<Content[]>(await inputFile.ReadToEndAsync());

            try
            {
                foreach (var content in contents)
                {
                    var config = Configuration.Default.WithDefaultLoader();
                    var address = content.url;
                    var document = await BrowsingContext.New(config).OpenAsync(address);

                    var price = SelectPrice(document, content.selector);

                    if (price != content.content)
                    {
                        var name = SelectName(document, content.titleSelector);

                        await HttpClient.PostStringAsync(url,
                            $"{name} price changed from ${content.content} to ${price}");

                        content.content = price;
                    }
                }
            }
            finally
            {
                await outputFile.WriteAsync(JsonConvert.SerializeObject(contents, Formatting.Indented));
            }
        }

        public static string SelectPrice(IDocument document, string priceSelector)
        {
            var priceSpan = document.QuerySelectorAll(priceSelector);
            return priceSpan.First().TextContent.Replace("$", "").Trim();
        }

        public static string SelectName(IDocument document, string headingSelector)
        {
            var heading = document.QuerySelectorAll(headingSelector);
            return heading.First().TextContent.Trim();
        }
    }

    public class Content
    {
        public string url { get; set; }
        public string content { get; set; }
        public string selector { get; set; }
        public string titleSelector { get; set; }
    }
}