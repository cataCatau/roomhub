using RoomHub.Api.Services;

namespace RoomHub.Api.Endpoints;

// TEMPORARY: only the old cancel route is left; it moves to Features/Bookings/CancelBooking next.
public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/{id:guid}/cancel", (Guid id, BookingService service) =>
        {
            service.Cancel(id);
            return Results.NoContent();
        });
    }
}
