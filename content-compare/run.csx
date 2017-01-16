using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

public static async Task Run(TimerInfo myTimer, TraceWriter log)
{
    var url = "https://prod-06.australiasoutheast.logic.azure.com:443/workflows/904acfab0cc041ea94af1a98867cc9f2/triggers/manual/run?api-version=2015-08-01-preview&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=pYfItovg26uxQXJVXFHEGZcGABDZjKf-xVcVZhvBulY";

    using (var client = new HttpClient())
    {
        var response = await client.PostAsync(url, new StringContent("content"));

        var responseString = await response.Content.ReadAsStringAsync();

        log.Info(responseString);
    }
}