# BUGS

The starter contains behaviors that contradict `ASSIGNMENT.md`. Write here every discrepancy you found.
Fill it in yourself, in your own words. There is no required number: what counts is that they are real.

| Which rule from ASSIGNMENT.md is violated (R1–R8) | Where the problem was in the starter (file, approximate line) | How you discovered it (test, reading, AI) | What you changed |
|---|---|---|---|
| R7 | `BookingService.cs`, line 28 | AI | Changed `<=` to `<` in the overlap `if` so back-to-back bookings dont collide |
| R7, R8 | `BookingService.cs`, line 26 | test failed after cancelling a booking | BookingRepository.GetActiveForRoomAsync only loads Active bookings, so cancelled ones no longer block the slot. |
| R8 | `BookingService.cs`, lines 56–59 | reading `Cancel` + test | Booking.Cancel returns BookingErrors.AlreadyCancelled (Conflict, 409) as a Result instead of throwing; status only changes in the domain. |
| R5 | `BookingService.cs`, `Create` (lines 8–32) | AI | Booking.Create takes nowUtc from TimeProvider (via CreateBookingHandler) and returns StartInPast (400). |
| R6 | `BookingService.cs`, `Create` (around line 17) | AI | Booking.Create checks the 4h max duration (DurationTooLong, 400); exactly 4h passes. |
