#r "Newtonsoft.Json"
#r "System.Drawing"
#r "System.IO"

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Configuration;
using Newtonsoft.Json;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    if (data?.image == null)
    {
        log.Info("No image provided");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a image in the request body");
    }

    try
    {
        ISupportedImageFormat format = new PngFormat();
        byte[] imageBytes = await DownloadImage(data.image.ToString());

        Face[] faces = await DetectFaces(req, imageBytes);

        foreach (var face in faces)
        {
            imageBytes = BlurFace(imageBytes, face, format);
        }

        var res = req.CreateResponse(HttpStatusCode.OK);
        res.Content = new ByteArrayContent(imageBytes);
        res.Content.Headers.ContentType = new MediaTypeHeaderValue(format.MimeType);
        return res;
    }
    catch (Exception ex)
    {
        log.Error("Error processing the image", ex);
        return req.CreateResponse(HttpStatusCode.BadRequest, "Error processing the image");
    }
}

public static async Task<byte[]> DownloadImage(string url)
{
    using (HttpClient client = new HttpClient())
    {
        using (HttpResponseMessage response = await client.GetAsync(url))
        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
        using (MemoryStream memStream = new MemoryStream())
        {
            await streamToReadFrom.CopyToAsync(memStream);
            memStream.Position = 0;
            return memStream.ToArray();
        }
    }
}

public static async Task<Face[]> DetectFaces(HttpRequestMessage req, byte[] imageBytes)
{
    using (MemoryStream faceStream = new MemoryStream(imageBytes))
    {
        var faceKey = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "key", true) == 0)
        .Value;

        faceKey = faceKey ?? ConfigurationManager.AppSettings["FaceAPI"];
        var faceServiceClient = new FaceServiceClient(faceKey);

        return await faceServiceClient.DetectAsync(faceStream, false, true);
    }
}

public static byte[] BlurFace(byte[] imageBytes, Face face, ISupportedImageFormat format)
{
    using (MemoryStream inStream = new MemoryStream(imageBytes))
    using (MemoryStream outStream = new MemoryStream())
    {
        using (ImageFactory imageFactory = new ImageFactory())
        {
            imageFactory.Load(inStream)
                        .Crop(new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height))
                        .GaussianBlur(20)
                        .Save(outStream);
        }

        using (ImageFactory imageFactory = new ImageFactory())
        {
            imageFactory.Load(inStream)
                        .Overlay(new ImageLayer
                        {
                            Image = Image.FromStream(outStream),
                            Position = new Point(face.FaceRectangle.Left, face.FaceRectangle.Top)
                        })
                        .Format(format)
                        .Save(outStream);
            return outStream.ToArray();
        }
    }
}