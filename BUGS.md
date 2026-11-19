# BUGS

The starter contains behaviors that contradict `ASSIGNMENT.md`. Write here every discrepancy you found.
Fill it in yourself, in your own words. There is no required number: what counts is that they are real.

| Which rule from ASSIGNMENT.md is violated (R1–R8) | Where the problem was in the starter (file, approximate line) | How you discovered it (test, reading, AI) | What you changed |
|---|---|---|---|
| R7 | `BookingService.cs`, line 28 | AI | Changed `<=` to `<` in the overlap `if` so back-to-back bookings dont collide |
| R7, R8 | `BookingService.cs`, line 26 | test failed after cancelling a booking | Ignored cancelled bookings in the overlap loop (only checking `Status == BookingStatus.Active`) |
| R8 | `BookingService.cs`, lines 56–59 | reading `Cancel` + test | Added a check if booking is already `Cancelled` and threw a conflict exception (409) instead of overwriting it again |
| R5 | `BookingService.cs`, `Create` (lines 8–32) | AI | Injected `TimeProvider` and rejected bookings where `startUtc` is in the past |
| R6 | `BookingService.cs`, `Create` (around line 17) | AI | Added duration check to reject bookings longer than 4 hours (`endUtc - startUtc > TimeSpan.FromHours(4)`) |
