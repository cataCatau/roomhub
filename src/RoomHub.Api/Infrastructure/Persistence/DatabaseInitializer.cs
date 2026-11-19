namespace RoomHub.Api.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    // Creates the SQLite schema if it does not exist yet. It never deletes data,
    // so rooms and bookings survive an application restart.
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoomHubDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
