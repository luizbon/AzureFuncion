using System.Net;
using System.Net.Http;
using AngleSharp;
using AngleSharp.Dom;

public static async Task Run(TimerInfo timerTrigger, TextReader inputFile, TextWriter outputFile, TraceWriter log)
{
    var url = "https://prod-06.australiasoutheast.logic.azure.com:443/workflows/904acfab0cc041ea94af1a98867cc9f2/triggers/manual/run?api-version=2015-08-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pYfItovg26uxQXJVXFHEGZcGABDZjKf-xVcVZhvBulY";

    string input = await inputFile.ReadToEndAsync();

    var config = Configuration.Default.WithDefaultLoader();
    var address = "https://www.jbhifi.com.au/computers-tablets/laptops/microsoft/microsoft-surface-book-i7-1tb-16gb-ram/977137/";
    var document = await BrowsingContext.New(config).OpenAsync(address);

    var price = SelectPrice(document);

    if (price != input)
    {
        input = price;

        var name = SelectName(document);

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(url, new StringContent($"{name} price is ${input}"));

            await response.Content.ReadAsStringAsync();
        }
    }

    await outputFile.WriteAsync(input);
}

public static string SelectPrice(IDocument document)
{
    var priceSelector = "p.price span.amount";
    var priceSpan = document.QuerySelectorAll(priceSelector);
    return priceSpan.First().TextContent.Replace("$", "").Trim();
}

public static string SelectName(IDocument document)
{
    var brandSelector = "div.brand h1";
    var brandH1 = document.QuerySelectorAll(brandSelector);
    return brandH1.First().TextContent.Trim();
}