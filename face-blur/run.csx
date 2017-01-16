#r "Newtonsoft.Json"
#r "System.Drawing"
#r "System.IO"

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
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
        byte[] imageBytes;
        using (HttpClient client = new HttpClient())
        {
            string url = data.image.ToString();
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            using (MemoryStream memStream = new MemoryStream())
            {
                await streamToReadFrom.CopyToAsync(memStream);
                memStream.Position = 0;
                imageBytes = memStream.ToArray();
            }
        }

        Face[] faces;

        using (MemoryStream faceStream = new MemoryStream(imageBytes))
        {
            var faceServiceClient = new FaceServiceClient("f38bd900f0314eef9b2566352645036e");

            faces = await faceServiceClient.DetectAsync(faceStream, false, true);
        }

        foreach (var face in faces)
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
                    imageBytes = outStream.ToArray();
                }
            }
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