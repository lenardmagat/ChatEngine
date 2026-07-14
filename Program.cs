using ChatSystem.DataBase;
using ChatSystem.Hubs;
using ChatSystem.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
var app = Configuration.webApplication();
app.UseSerilogRequestLogging();
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AppHub>("/ChatHub");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DbManager>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database tables.");
    }
}

app.Run();