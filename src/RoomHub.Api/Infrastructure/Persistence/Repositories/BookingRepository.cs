using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.Bookings;

namespace RoomHub.Api.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetActiveForRoomAsync(
        Guid roomId, CancellationToken cancellationToken) =>
        await db.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveForPersonOnDayAsync(
        string bookedBy, DateTime dayStartUtc, CancellationToken cancellationToken)
    {
        var dayEndUtc = dayStartUtc.AddDays(1);
        return db.Bookings.CountAsync(
            b => b.BookedBy == bookedBy
                 && b.Status == BookingStatus.Active
                 && b.StartUtc >= dayStartUtc
                 && b.StartUtc < dayEndUtc,
            cancellationToken);
    }

    // Add only marks the entity; IUnitOfWork.SaveChangesAsync writes it.
    public void Add(Booking booking) => db.Bookings.Add(booking);
}
