# DECISIONS

Answer **yourself**, in your own words, in 3–5 sentences per question.
Refer to concrete files and classes from your own repo (for example `Features/Bookings/CreateBooking/...`).
An unanswered question gives 0 points.

## 1. A rule you moved
R7, was in Services/BookingService.cs, now is in Domain/Bookings/Booking.cs in Create și Overlaps
_(your answer here)_

## 2. Result instead of an exception
Why: an occupied room is a normal business outcome, not an unexpected failure. The method signature shows it can fail.
Flow: Booking.Create returns BookingErrors.RoomOccupied. CreateBookingHandler returns it. CreateBookingEndpoint calls ToProblem() from Common/ErrorHttpExtensions.cs. ErrorType.Conflict becomes 409 ProblemDetails.

## 3. Who knows about EF Core?
- Knows: Infrastructure/Persistence/ (RoomHubDbContext, RoomConfiguration, BookingConfiguration, RoomRepository, BookingRepository) and Program.cs.
- Doesn't know: Domain/ (IRoomRepository, IBookingRepository, IUnitOfWork) and Features/.
- Gain: handlers testable with fakes, domain independent of the database.
- Cost: extra interfaces and classes.
_(your answer here)_

## 4. The flow of a request
POST /api/rooms/{roomId}/bookings
-> CreateBookingEndpoint -> CreateBookingCommand -> CreateBookingHandler
-> IRoomRepository.GetByIdAsync (404 if missing)
-> IBookingRepository.GetActiveForRoomAsync / CountActiveForPersonOnDayAsync
-> Booking.Create (rules) -> IBookingRepository.Add
-> IUnitOfWork.SaveChangesAsync (RoomHubDbContext) -> SQLite roomhub.db

## 5. What you rejected from what the AI gave you
- AI tried EF Core 11.0.0, which doesn't support net10, so you switched to 10.x.
- You rejected the first batch of files and asked for task-by-task work.

## 6. If you had 30 more minutes
What would you improve and what risk did you leave in the project?

- Improve: automated tests.
    - Risks: cancel wasn't tested by request, two concurrent requests can both pass the overlap check, EnsureCreated is used instead of migrations.
