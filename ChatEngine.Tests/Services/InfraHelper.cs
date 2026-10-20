using Amazon.S3;
using Amazon.S3.Model;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatEngine.InfrastructureDeletion;

public static class InfraHelper
{
    public static async Task ClearMeiliSearchAsync(HttpClient meiliClient)
    {
        try
        {
            var response = await meiliClient.DeleteAsync($"/indexes/userdocumentation/documents");
            if (response.IsSuccessStatusCode)
            {
                var taskUid = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("taskUid").GetInt32();
                while (true)
                {
                    var task = await meiliClient.GetFromJsonAsync<JsonElement>($"/tasks/{taskUid}");
                    var status = task.GetProperty("status").GetString();
                    if (status == "succeeded" || status == "failed") break;
                    await Task.Delay(50);
                }
            }
        }
        catch
        {
            // Ignore if meilisearch index doesn't exist yet
        }

        try
        {
            var prodResponse = await meiliClient.DeleteAsync($"/indexes/productdocumentation/documents");
            if (prodResponse.IsSuccessStatusCode)
            {
                var prodTaskUid = (await prodResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("taskUid").GetInt32();
                while (true)
                {
                    var task = await meiliClient.GetFromJsonAsync<JsonElement>($"/tasks/{prodTaskUid}");
                    var status = task.GetProperty("status").GetString();
                    if (status == "succeeded" || status == "failed") break;
                    await Task.Delay(50);
                }
            }
        }
        catch
        {
            // Ignore if meilisearch index doesn't exist yet
        }
    }
    
    public static async Task ClearS3BucketAsync(IAmazonS3 s3Client, string bucketName)
    {
        try
        {
            var listResponse = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName
            });
            if (listResponse.S3Objects != null && listResponse.S3Objects.Count > 0)
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = bucketName,
                    Objects = listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
                };
                await s3Client.DeleteObjectsAsync(deleteRequest);
            }
        }
        catch (NoSuchBucketException)
        {
            await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName });
        }
    }
}