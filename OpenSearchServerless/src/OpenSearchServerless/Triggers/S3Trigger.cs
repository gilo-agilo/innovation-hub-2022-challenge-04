using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Nest;
using Newtonsoft.Json;
using OpenSearchServerless.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OpenSearchServerless.Triggers;

public class S3Trigger
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public S3Trigger()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client"></param>
    public S3Trigger(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string?> FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var s3Event = evnt.Records?[0].S3;
        if (s3Event == null)
        {
            return null;
        }

        try
        {
            var bucketName = s3Event.Bucket.Name;
            var objectKey = s3Event.Object.Key;
            context.Logger.LogInformation($"Reading and indexing data from {objectKey} file in {bucketName} bucket");

            using (var resp = await this.S3Client.GetObjectAsync(bucketName, objectKey))
            using (var respStream = resp.ResponseStream)
            using (var reader = new StreamReader(respStream))
            {
                var currLine = 1;
                while (reader.Peek() >= 0)
                {
                    var str = await reader.ReadLineAsync();

                    if (str == null)
                    {
                        context.Logger.LogInformation("Skipping empty line");
                        continue;
                    }
                    
                    context.Logger.LogInformation($"Going to deserealize line: ${str}");
                    var book = JsonConvert.DeserializeObject<Book>(str);
                    if (book == null)
                    {
                        context.Logger.LogInformation("Skipping empty item");
                        continue;
                    }
                    var idxResp = await ElasticSearchClient.Instance.IndexDocumentAsync(book);
                    
                    context.Logger.LogInformation(idxResp.IsValid
                        ? $"Indexing of {currLine} item is succeeded. Item id = {book.Isbn}"
                        : $"Indexing of {currLine} item is failed. Item id = {book.Isbn}");
                    
                    currLine++;
                }

                context.Logger.LogInformation($"Index completed");
                return "ok";
            }
        }
        catch (Exception e)
        {
            context.Logger.LogInformation(
                $"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
            context.Logger.LogInformation(e.Message);
            context.Logger.LogInformation(e.StackTrace);
            throw;
        }
    }
}