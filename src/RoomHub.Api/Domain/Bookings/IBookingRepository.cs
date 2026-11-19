namespace RoomHub.Api.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(Guid roomId, CancellationToken cancellationToken);

    // R9: active bookings of bookedBy that start in [dayStartUtc, dayStartUtc + 1 day).
    Task<int> CountActiveForPersonOnDayAsync(
        string bookedBy, DateTime dayStartUtc, CancellationToken cancellationToken);

    void Add(Booking booking);
}
