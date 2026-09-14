using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.OfferMechanisBackgroundWorker;
public class AutoCompleteOfferHander(DbManager db, ILogger<AutoCompleteOfferHander> logger) : IRequestHandler<AutoCompleteCommand, Result<MessageResponseDTO>>
{
    public async Task<Result<MessageResponseDTO>> Handle(AutoCompleteCommand command, CancellationToken cancellation)
    {
        try
        {
        }
    }
}