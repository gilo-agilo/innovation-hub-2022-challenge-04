using System.Net;
using Newtonsoft.Json;

namespace OpenSearchServerless.Services;

public static class ImageRecognitionClient
{
    private const string ServiceUrl = "http://k8s-default-ingressi-e7a5128b21-1907316656.us-east-1.elb.amazonaws.com/ImageVector";

    public static async Task<decimal[]?> GetImageVector(string imageName, string imageUrl)
    {
        var imageData = await GetMedia(imageUrl);
        var imageVector = SendImageVectorRequest(imageName, imageData);

        return imageVector;
    }
    
    private static decimal[]? SendImageVectorRequest(string fileName, byte[] data)
    {
        var boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
        // The first boundary
        var boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
        // The last boundary
        var trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
        // The first time it iterates, we need to make sure it doesn't put too many new paragraphs down or it completely messes up poor webbrick
        System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
        
        // Create the request and set parameters
        var request = (HttpWebRequest) WebRequest.Create(ServiceUrl);
        request.ContentType = "multipart/form-data; boundary=" + boundary;
        request.Method = "POST";
        request.KeepAlive = true;
        request.Credentials = CredentialCache.DefaultCredentials;

        // Get request stream
        var requestStream = request.GetRequestStream();

        var formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(
            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
            "image-file", fileName));
        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
        requestStream.Write(formItemBytes, 0, formItemBytes.Length);
        requestStream.Write(data, 0, data.Length);

        // Write trailer and close stream
        requestStream.Write(trailer, 0, trailer.Length);
        requestStream.Close();

        using var reader = new StreamReader(request.GetResponse().GetResponseStream());
        var result = reader.ReadToEnd();
        var vectorObject = JsonConvert.DeserializeObject<VectorResponse>(result);

        return vectorObject?.Vector;
    }

    private static async Task<byte[]> GetMedia(string mediaUrl)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(mediaUrl, HttpCompletionOption.ResponseHeadersRead);
        
        return await response.Content.ReadAsByteArrayAsync();
    }
}
    
    