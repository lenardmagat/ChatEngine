using ChatSystem.DataBase;
using ChatSystem.Middleware;
using ChatSystem.WebSocketMiddleware;
using Serilog;
var app = Configuration.webApplication();
app.UseSerilogRequestLogging();
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<WebSocketRoutingMiddleware>();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DbManager>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database tables.");
    }
}

app.Run();