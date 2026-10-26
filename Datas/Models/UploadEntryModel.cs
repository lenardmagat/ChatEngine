using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ChatSytstem.Models;
public enum UploadStatus
{
    Pending,
    Completed,
    Failed
}
public enum FileReference
{
    Profile,
    Product
}
public class UploadEntry
{
    [Key]
    public int UploadEntryId {get; set;}
    public string FileKey {get; set;} = null!;
    public string FileName {get; set;} = null!;
    public  UploadStatus Status {get; set;}
    public int Retries {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public FileReference Reference {get; set;}
    public int ReferenceId {get; set;}
    public DateTime? UpdatedAt {get; set;} = null;
}
    