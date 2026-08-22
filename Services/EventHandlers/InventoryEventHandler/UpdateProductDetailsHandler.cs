using System.Net.Mail;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class UpdateProductDetailsHandler : IRequestHandler<UpdateProductCommand, Result>
{
    DbManager _db;
    IHasher _hasher;
    ILogger<UpdateProductDetailsHandler> _logger;
    public UpdateProductDetailsHandler(DbManager db, IHasher hasher, ILogger<UpdateProductDetailsHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try{
            User? owner = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if(owner is null)
            {
                return Result.Failure("Invalid Credentials", StatusCodes.Status400BadRequest);
            }
            if(!_hasher.VerifyPassword(request.Details.UserPassword, owner.HashedPassword))
            {
                return Result.Failure("Wrong Password", StatusCodes.Status400BadRequest);
            }
            var DecodedId = _hasher.DecodeHashids(request.Details.ProductId, HashContext.Product);
            if(!DecodedId.IsSuccess)
            {
                return Result.Failure("Tampered or broken Id detected", StatusCodes.Status401Unauthorized);
            }
            Product? product = await _db.Products.FindAsync(DecodedId.Value);
            if(product is null || product.OwnerUserId != request.UserId)
            {
                return Result.Failure("Invalid Credentials", StatusCodes.Status401Unauthorized);
            }
            product.ProductName = request.Details.NewName;
            product.ProductDescription = request.Details.NewDescription;
            await _db.SaveChangesAsync();
            return Result.Success();
            }
        catch(Exception e){
            _logger.LogCritical(e, $"An critical bug occured while processing update on Product. Details:{request.Details}");
            return Result.Failure("An Unexepected error occcured in the server", StatusCodes.Status500InternalServerError);
        }
    }
}
