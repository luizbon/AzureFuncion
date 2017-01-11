using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TextReader inputFile, TraceWriter log)
{
    var lines = new List<string>();

    string line;
    while ((line = await inputFile.ReadLineAsync()) != null)
    {
        lines.Add(line);
    }

    Random rnd = new Random();

    int r = rnd.Next(lines.Count);

    return req.CreateResponse(HttpStatusCode.OK, lines.ToArray()[r]);
}