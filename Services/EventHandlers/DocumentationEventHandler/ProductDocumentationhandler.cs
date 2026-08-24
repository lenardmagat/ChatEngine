using ChatSystem.DataBase;
using ChatSystem.DTOs.Documentation;
using ChatSystem.Services.Interfaces;
using MediatR;

namespace ChatSystem.EventHandler.Documentation;
public class ProductDocumentationStrategy(DbManager db, IDynamicSearchService searchService) : IDocumentStrategy
{
    public DocumentTarget Target =>  DocumentTarget.Product;
    public async Task DocumentAsync(DocumentRequest request, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync(new object[]{int.Parse(request.DocumentId), cancellationToken});
        if(product == null )
        {
            await searchService.DeleteFromIndexAsync<ProductDocumentation>(request.DocumentId);
        }
        else
        {
            await searchService.IndexAsync<ProductDocumentation>(
                new ProductDocumentation(
                    product.Id.ToString(),
                    product.ProductName,
                    product.ProductDescription,
                    product.BasePrice.ToString(),
                    product.ProductAvailable.ToString(),
                    product.Mode.ToString(),
                    product.IsAvailable.ToString(),
                    product.IsActive.ToString()
                ),
                cancellationToken
            );
        }

    }
}