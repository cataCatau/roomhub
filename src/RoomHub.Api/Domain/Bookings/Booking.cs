using RoomHub.Api.Common;
using RoomHub.Api.Domain.Rooms;

namespace RoomHub.Api.Domain.Bookings;

public sealed class Booking
{
    public const int MaxBookedByLength = 100;
    public const int MaxActiveBookingsPerDay = 2;
    public const int LargeRoomCapacity = 10;
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);
    public static readonly TimeSpan MaxDurationLargeRoom = TimeSpan.FromHours(2);
    public static readonly TimeSpan MinCancelNotice = TimeSpan.FromMinutes(60);

    private Booking()
    {
    }

    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public string BookedBy { get; private set; } = string.Empty;
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public int Attendees { get; private set; }
    public BookingStatus Status { get; private set; }

    // The caller (the handler) has already checked that the room exists (404 before validation).
    // activeBookingsOfRoom: the active bookings of the same room (R7).
    // activeBookingsOfPersonThatDay: active bookings of the same bookedBy whose start is on the same UTC day (R9).
    public static Result<Booking> Create(
        Room room,
        string? bookedBy,
        DateTime start,
        DateTime end,
        int attendees,
        DateTime nowUtc,
        IEnumerable<Booking> activeBookingsOfRoom,
        int activeBookingsOfPersonThatDay)
    {
        var startUtc = ToUtc(start);
        var endUtc = ToUtc(end);

        // R2
        if (string.IsNullOrWhiteSpace(bookedBy))
            return BookingErrors.InvalidBookedBy;
        if (bookedBy.Trim().Length > MaxBookedByLength)
            return BookingErrors.BookedByTooLong;

        // R3
        if (endUtc <= startUtc)
            return BookingErrors.InvalidRange;

        // R4
        if (attendees <= 0)
            return BookingErrors.InvalidAttendees;
        if (attendees > room.Capacity)
            return BookingErrors.AttendeesExceedCapacity;

        // R5: "now" comes from TimeProvider, passed in by the handler.
        if (startUtc < nowUtc)
            return BookingErrors.StartInPast;

        // R10: rooms with more than 10 seats, at most 2 hours (exactly 2h allowed).
        // R6: otherwise at most 4 hours (exactly 4h allowed).
        var duration = endUtc - startUtc;
        if (room.Capacity > LargeRoomCapacity && duration > MaxDurationLargeRoom)
            return BookingErrors.DurationTooLongForLargeRoom;
        if (duration > MaxDuration)
            return BookingErrors.DurationTooLong;

        // R7: only active bookings count; touching intervals do not overlap.
        if (activeBookingsOfRoom.Any(b => b.IsActive && b.Overlaps(startUtc, endUtc)))
            return BookingErrors.RoomOccupied;

        // R9: at most 2 active bookings per participant per UTC day, across all rooms.
        if (activeBookingsOfPersonThatDay >= MaxActiveBookingsPerDay)
            return BookingErrors.DailyLimitReached;

        return new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BookedBy = bookedBy.Trim(),
            StartUtc = startUtc,
            EndUtc = endUtc,
            Attendees = attendees,
            Status = BookingStatus.Active
        };
    }

    public bool IsActive => Status == BookingStatus.Active;

    // R8: Active -> Cancelled; already cancelled -> 409.
    // R11: only if the booking starts at least 60 minutes from now -> otherwise 409.
    public Result<Unit> Cancel(DateTime nowUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return BookingErrors.AlreadyCancelled;

        if (StartUtc - nowUtc < MinCancelNotice)
            return BookingErrors.TooLateToCancel;

        Status = BookingStatus.Cancelled;
        return Unit.Value;
    }

    // Half-open interval: [StartUtc, EndUtc). 10-12 and 12-14 do not overlap.
    public bool Overlaps(DateTime startUtc, DateTime endUtc) =>
        StartUtc < endUtc && startUtc < EndUtc;

    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
