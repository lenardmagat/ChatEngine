using ChatSystem.DataBase;
using ChatSystem.DTOs.Documentation;
using ChatSystem.Services.Interfaces;

namespace ChatSystem.EventHandler.Documentation;
public class UserDocumentationStrategy(IDynamicSearchService Searchservice, DbManager db) : IDocumentStrategy
{
    public DocumentTarget Target => DocumentTarget.User;
    public async Task DocumentAsync(DocumentRequest request, CancellationToken cancellation = default)
    {
        var user = await db.Users.FindAsync(new object[]{int.Parse(request.DocumentId)}, cancellation);
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
                    user.Status
                    ), 
                cancellation
                );
        }
    }
}