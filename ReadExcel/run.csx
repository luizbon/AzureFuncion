using System.Net;
using System.Threading.Tasks;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TextReader input, TraceWriter log)
{
    log.Verbose(input.GetType().Name);
    
    var txt = input.ReadToEnd();
    
    return req.CreateResponse(HttpStatusCode.OK, txt);
}