using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Common.Cqrs;
using RoomHub.Api.Domain.Bookings;
using RoomHub.Api.Domain.Common;
using RoomHub.Api.Domain.Rooms;
using RoomHub.Api.Endpoints;
using RoomHub.Api.Features.Bookings;
using RoomHub.Api.Features.Rooms;
using RoomHub.Api.Infrastructure.Persistence;
using RoomHub.Api.Middleware;
using RoomHub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton(TimeProvider.System);

// DbContext is Scoped: one instance per HTTP request.
builder.Services.AddDbContext<RoomHubDbContext>((serviceProvider, options) =>
    options.UseSqlite(serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("RoomHub")));

// The repositories and the Unit of Work share the same DbContext instance.
builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
    serviceProvider.GetRequiredService<RoomHubDbContext>());
builder.Services.AddScoped<IRoomRepository, RoomHub.Api.Infrastructure.Persistence.Repositories.RoomRepository>();
builder.Services.AddScoped<IBookingRepository, RoomHub.Api.Infrastructure.Persistence.Repositories.BookingRepository>();

builder.Services.AddHandlers(typeof(Program).Assembly);

// TEMPORARY: old starter booking flow, removed in the next steps.
builder.Services.AddSingleton<RoomHub.Api.Repositories.RoomRepository>();
builder.Services.AddSingleton<RoomHub.Api.Repositories.BookingRepository>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseMiddleware<ExceptionMiddleware>();

app.MapGet("/", () => "RoomHub API is running.");
app.MapRooms();
app.MapBookings();
app.MapBookingEndpoints();

app.Run();
