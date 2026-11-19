using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RoomHub.Api.Infrastructure.Persistence;

// SQLite stores dates as text; EF reads them back with Kind = Unspecified.
// This converter marks the values read from the database as UTC.
public sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
