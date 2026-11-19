using RoomHub.Api.Services;

namespace RoomHub.Api.Endpoints;

// TEMPORARY: the room routes moved to Features/Rooms. Only the old booking-creation
// route is left here; it is replaced by Features/Bookings/CreateBooking in the next step.
public record CreateBookingRequest(string? BookedBy, DateTime StartUtc, DateTime EndUtc, int Attendees);

public record BookingCreatedResponse(Guid Id, Guid RoomId, string Status);

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rooms").WithTags("Rooms");

        group.MapPost("/{roomId:guid}/bookings", (Guid roomId, CreateBookingRequest request, BookingService service) =>
        {
            var booking = service.Create(
                roomId, request.BookedBy, request.StartUtc, request.EndUtc, request.Attendees);

            return Results.Created(
                $"/api/bookings/{booking.Id}",
                new BookingCreatedResponse(booking.Id, booking.RoomId, booking.Status.ToString()));
        });
    }
}
