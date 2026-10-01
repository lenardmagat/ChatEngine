using Amazon.S3;
using ChatSystem.core.KeyConfiguration;
using ChatSystem.ErrorHandling;
using Microsoft.Extensions.Options;
using Amazon.S3.Model;
namespace ChatSystem.Storage;
public interface IFileStorageService
{
    Task<Result> UploadImageAsync(Stream FileStream, string FileKey, string FileName);
    Task<Result> DeleteImageAsync(string FileKey);
    Task<bool> AllowedImageExtension(string FileName);
    string GetImageUrl(string ImageKey);
}
public class R2FileStorageServices : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;
    private readonly List<string> AllowedExtensionType = [".jpeg", ".jpg", ".png"];
    public R2FileStorageServices(IAmazonS3 s3Client, IOptions<StorageOptions> options)
    {
        _s3Client = s3Client;
        _bucketName = options.Value.Bucket;
        _publicUrl = options.Value.PublicEndpoint;
    }
    public async Task<Result> UploadImageAsync(Stream FileStream, string FileKey, string FileName)
    {
        var streamResponse = await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = FileKey,
                InputStream = FileStream,
                ContentType = GetImageContentType(FileName),
                DisablePayloadSigning = false   
            }
        );
        if(streamResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            return Result.Failure("Failed to upload image", StatusCodes.Status400BadRequest);
        }
        return Result.Success();
    }
    public async Task<Result> DeleteImageAsync(string FileKey)
    {
        var deleteResponse = await _s3Client.DeleteObjectAsync(_bucketName, FileKey);
        if(deleteResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            return Result.Failure("Failed to delete the file.", StatusCodes.Status400BadRequest);
        }
        return Result.Success();
    }
    public async Task<bool> AllowedImageExtension(string FileName)
    {
        string fileExtension = Path.GetExtension(FileName).ToLowerInvariant();
        if (!AllowedExtensionType.Contains(fileExtension))
        {
            return false;
        }
        return true;
    }
    public string GetImageUrl(string ImageKey) => $"{_publicUrl}/{_bucketName}/{ImageKey}";
    private string GetImageContentType(string FileName)
    {
        var extension = Path.GetExtension(FileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}