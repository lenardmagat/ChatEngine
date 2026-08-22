using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;

namespace ChatSystem.Services.Interfaces.OfferInterfaces;
public interface IOfferSaleStrategy
{
    public SaleOfferStatus Target {get;}
    Task<Result<OfferResponse>> OfferStrategy();
}