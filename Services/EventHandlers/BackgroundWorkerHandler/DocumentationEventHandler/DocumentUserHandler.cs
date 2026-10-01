using ChatSystem.DataBase;
using ChatSystem.DTOs.Documentation;
using ChatSystem.Services.Interfaces;
using ChatSystem.Storage;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Documentation;
public class UserDocumentationStrategy(IDynamicSearchService Searchservice, DbManager db, IFileStorageService fileStorage) : IDocumentStrategy
{
    public DocumentTarget Target => DocumentTarget.User;
    public async Task DocumentAsync(DocumentRequest request, CancellationToken cancellation = default)
    {
        var user = await db.Users.Include(u => u.UserProfilePicture).AsNoTracking().Where(u => u.UserId == int.Parse(request.DocumentId)).FirstOrDefaultAsync(cancellation);
        if(user == null)
        {
            await Searchservice.DeleteFromIndexAsync<UserDocumentation>(request.DocumentId, cancellation);
        }
        else
        {
            await Searchservice.IndexAsync<UserDocumentation>(
                new UserDocumentation(
                    user.UserId.ToString(), 
                    user.Username, 
                    user.Role.ToString(), 
                    user.Status,
                    user.UserProfilePicture != null ? fileStorage.GetImageUrl(user.UserProfilePicture!.PhotoKey) : null
                    ), 
                cancellation
                );
        }
    }
}