namespace ChatSystem.Infrastructure;
public class DirectoryHelper
{
    private static readonly string _stagingDirectoryPath =
        Path.Combine(AppContext.BaseDirectory, "Uploads", "staging");
    public static void EnsureUploadDirectory()
    {
        Directory.CreateDirectory(_stagingDirectoryPath);
    }

    public static async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        string fileKey = Guid.NewGuid().ToString("N");
        string filePath = Path.Combine(_stagingDirectoryPath, fileKey);

        await using (var output = new FileStream(
            filePath, FileMode.CreateNew,FileAccess.Write 
        ))
        {
            await file.CopyToAsync(output, cancellationToken);
        }
        return fileKey;
    }
    public static FileStream OpenRead(string fileKey)
    {
        string path = Path.Combine(_stagingDirectoryPath, fileKey);
        return File.OpenRead(path);
    }
    public static void DeleteAsync(string fileName)
    {
        string path = Path.Combine(_stagingDirectoryPath, fileName);
        
        File.Delete(path);
        return;
    }
}
