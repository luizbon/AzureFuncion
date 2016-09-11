using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TextReader input, TraceWriter log)
{
    var lines = new List<string>();

    string line;
    while ((line = input.ReadLine()) != null)
    {
        lines.Add(line);
    }

    Random rnd = new Random();

    int r = rnd.Next(lines.Count);
    
    return req.CreateResponse(HttpStatusCode.OK, lines.ToArray()[r]);
}