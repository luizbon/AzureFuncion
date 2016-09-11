using System.Net;
using System.Threading.Tasks;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TextReader input, TraceWriter log)
{
    var lines = new List<string>();

    string line;
    while ((line = input.ReadLine()) != null)
    {
        lines.Add(line);
    }
    
    return req.CreateResponse(HttpStatusCode.OK, new JArray(lines.ToArray()));
}