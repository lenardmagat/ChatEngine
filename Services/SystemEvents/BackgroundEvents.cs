using ChatSystem.ErrorHandling;
using ChatSytstem.Models;
using MediatR;

namespace ChatSystem.SystemEvents.BackgroundEvents;
public record UploadFilesAsyncCommand(UploadEntry UploadEntry) : IRequest<Result>;
