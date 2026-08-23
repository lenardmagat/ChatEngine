// using ChatSystem.core;
// using ChatSystem.DataBase;
// using ChatSystem.DTOs.Inventory;
// using ChatSystem.ErrorHandling;
// using ChatSystem.SystemEvents.Inventory;
// using MediatR;

// namespace ChatSystem.EventHandler.Inventory;
// public class UpdateProductStatusHandler : IRequestHandler<UpdateProductStatusCommand, Result>
// {
//     private readonly DbManager _db;
//     private readonly IHasher _hasher;
//     private readonly ILogger<UpdateProductStatusHandler> _logger;
//     public UpdateProductStatusHandler(DbManager db, IHasher hasher, ILogger<UpdateProductStatusHandler> logger)
//     {
//         _db = db;
//         _hasher = hasher;
//         _logger = logger;
//     }
//     public async Task<Result> Handle(UpdateProductStatusCommand details, CancellationToken cancellationToken)
//     {
//         var DecodedProductId = _hasher.DecodeHashids(details.Status.ProductId, HashContext.Product);
//         if (!DecodedProductId.IsSuccess)
//         {
//             return Result.Failure("Tampered or broken Id has been detected", StatusCodes.Status401Unauthorized);
//         }
//         var product = await _db.Products.FindAsync(DecodedProductId.Value);
//         if(product is null)
//         {
//             return Result.Failure("Product is not existing", StatusCodes.Status400BadRequest);
//         }
//         if(!_hasher.VerifyPassword(details.Status.UserPassword, product.Owner.HashedPassword) || product.OwnerUserId != details.UserId)
//         {
//             return Result.Failure("Invalid Credentials", StatusCodes.Status401Unauthorized);
//         }
//         if(details.Status.AddOrRemove == IsAddOrRemove.Remove)
//         {
            
//         }
//     }
// }