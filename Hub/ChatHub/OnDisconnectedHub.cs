namespace ChatSystem.Hubs;
public partial class AppHub
{
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        await base.OnDisconnectedAsync(exception);
    }
}