

C A M P U S HU B   ·   B U S IN E S S   A P P L I C A T ION S   WITH  .N E T   1 0
## Lecture 03
R ic h Domain and R es ult
E vent.Create, R esult<T> and Error, DateR ange and TimeP rovider: business rules
live in the domain
Code: s rc/Campus Hub.Api/Domain ·  tag lecture-03-done

Lecture objectives
## 01
Rich domain
Event with no public constructor and no public
setters: the only way in is Event.Create.
## 02
Result and Error
Business failures are values returned from a method,
not exceptions thrown.
## 03
Value object
DateRange can only be created valid, and it can tell
whether it overlaps another range.
## 04
Controllable time
The current time comes from TimeProvider, so the
"not in the past" rule can be tested.
CampusHub · Lecture 03 · Rich Domain and Result

R ec ap: lec ture 2
Minimal API.E ndpoints  mapped with MapP os t and MapG et, returning TypedR es ults .
DI.InMemoryE ventS tore regis tered as  a S ingleton and received as  a parameter in Handle.
Vertic al  slic e.An endpoint's  R eques t, R es pons e and logic live in the s ame folder.
Pos tman.The firs t collection: create an event and read it by id.
Left open.Validation was  temporary, written directly in the endpoint. Today we move it into the domain.
CampusHub · Lecture 03 · Rich Domain and Res ult

Rich domain: Event
No public  setters.State changes
only from inside the class.
Private constructor.You cannot call
new Event from outside.
Event.Createreturns
Result< Event> : either a valid Event
or an error.
public sealed classEvent
## {
privateEvent()
## {
## }
publicGuid Id { get; private set; }
public stringTitle { get; private set; } = string.Empty;
publicEventStatus Status { get; private set; }
public intCapacity { get; private set; }
public staticResult<Event> Create(
stringtitle,
## /* ... */
intcapacity,
DateTime nowUtc)
CampusHub · Lecture 03 · Rich Domain and ResultSimplified excerpt: src/CampusHub.Api/Domain/Events/Event.cs

E vent.C reate: validation with early returns
R ange firs t.If DateR ange.Create
fails, we return its error.
Then Validate.It returns  the firs t
error found, or null.
A new E ventalways  starts in the
Draft state.
varrangeResult = DateRange.Create(startUtc, endUtc);
if(rangeResult.IsFailure)
returnrangeResult.Error;
varerror = Validate(title, description, capacity,
rangeResult.Value, nowUtc);
if(error is not null)
returnerror;
return newEvent
## {
Id = Guid.NewGuid(),
Title = title.Trim(),
## /* ... */
Status = EventStatus.Draft,
Range = rangeResult.Value,
Capacity = capacity
## };
CampusHub ·  Lecture 03 · Rich Domain and Res ultSimplified excerpt: s rc/CampusHub.Api/Domain/E vents /E vent.cs

Result<T>: success or error
Exceptionsfor unexpected
situations, Resultfor business
failures.
Private constructor.Instances are
created only through Success or
## Failure.
Implicit conversions:return ev; or
return error; straight from the
method.
public sealed classResult<T>
## {
priv at e readon lyT? _value;
private readonlyE rro r? _erro r;
public boo lIsSuccess => _error is n ull;
public boolIsF ailure = > ! IsSuccess;
public st at icResult<T> Success(T value) => new(value, null);
public staticResult <T > F ailure( Erro r erro r)  = >
new(def ault, error);
public static implicit operatorResult <T >(T  v alue)  = >
## Success(value);
public static implicit operatorResult <T >(E rro r erro r)  = >
## Failure(error);
## }
CampusHub · Lecture 03 · Rich Domain and ResultSimplified excerpt: src/CampusHub.Api/Domain/Common/Result.cs

E rror: a bus ines s  error as  a value
A rec ordwith three fields: Code,
Message and Type.
Stable Code.Clients  can rely on it;
the mes s age can be reworded.
Typeis translated into an HTTP
status later.
public enumErrorType
## {
## Validation,
NotFound,
## Conflict
## }
public sealed recordError(
stringCode, stringMessage, ErrorType Type)
## {
public staticError Validation(
stringcode, stringmessage) =>
new(code, message, ErrorType.Validation);
/* ... NotFound, Conflict ... */
## }
CampusHub ·  Lecture 03 · Rich Domain and Res ultSimplified excerpt: s rc/Campus Hub.Api/Domain/Common/E rror.cs

Value object: DateRange
A recorddefined by its value, with
no identity of its own.
Create validates:the end must be
after the start.
Overlapsis half-open: one range
ending at 12:00 and another starting
at 12:00 do not overlap.
public sealed recordDateRange
## {
publicDateTime StartUtc { get; private set; }
publicDateTime EndUtc { get; private set; }
public staticResult<DateRange> Create(
DateTime start, DateTime end)
## {
varstartUtc = ToUtc(start);
varendUtc = ToUtc(end);
if(endUtc <= startUtc)
returnDateRangeErrors.EndBeforeStart;
## /* ... */
## }
public boolOverlaps(DateRange other) =>
StartUtc < other.EndUtc && other.StartUtc < EndUtc;
## }
CampusHub · Lecture 03 · Rich Domain and ResultSimplified excerpt: src/CampusHub.Api/Domain/Events/DateRange.cs

TimeProvider: the time comes from outside
The domain does not read the
clock.Event.Create receives nowUtc
as a parameter.
The endpoint asks for it through
DI:a TimeProvider parameter.
In Program.cswe register
TimeProvider.System, the real clock.
//  P rog ram. cs
builder.Services.ConfigureHttpJsonOptions(options =>
o pt ion s. Serializ erOpt ion s.Convert ers.A dd(
newJsonStringEnumConverter()));
builder. Serv ices.A ddSing leton(T imeP rov ider. Sy st em) ;
// CreateEventEndpoint.cs, în apelul Event.Create(...)
tim e. GetUt cNow().Ut cD at eT im e) ;
CampusHub · Lecture 03 · Rich Domain and ResultSimplified excerpt: src/CampusHub.Api/Program.cs

Thin endpoint: it only trans lates
No bus ines s  rules .It calls
Event.Create and nothing more.
F ailure:result.E rror.ToProblem()
becomes a ProblemDetails
response.
Suc c es s :the event goes into the
store and the response is 201
## Created.
priv at e st at icResults<Created<Response>, ProblemHttpResult>
Han dle(
Request request,
InM emo ryEv ent Sto re sto re,
TimeProvider time)
## {
varresult = Event.Create(
request.T it le,
## /*  ...  */
time.GetUtcNow().UtcDateTime);
if( result. IsF ailure)
ret urnresult.Error.ToProblem();
varev  =  result.V alue;
store.Add(ev);
## /* ... */
CampusHub ·  Lecture 03 · Rich Domain and Res ult
Simplified excerpt:
s rc/Campus Hub.Api/F eatures /E vents /CreateE vent/CreateE ventE ndpoint.cs

The flow: from reques t to res pons e
StepWhereWhat happens
- RequestCreateE ventE ndpointThe JSON becomes a Request; TimeP rovider supplies
nowUtc
- RangeDateR ange.C reateend ≤ start: EndBeforeStart
- RulesE vent.Validatetitle, description, capacity, start in the past
- ResultRes ult<Event>An E vent in the Draft state, or an Error
- HTTPError.ToProblem()Validation: 400, NotFound: 404, Conflict: 409
G ET  /api/events/{id}: 200 OK with the event, or 404 with EventE rrors.NotFound.
CampusHub ·  Lecture 03 · Rich Domain and Res ultSimplified excerpt: s rc/CampusHub.Api/Common/E rrorHttpE xtensions.cs

Live coding: the steps
1.Domain/ Common: Error,  Result<T> , Unit
2.DateRange as a record, with Create and Overlaps
3.EventErrors and Event.Create with Validate
4.ErrorHttpExtensions: ToProblem
5.CreateEventEndpoint: Event.Create instead of ifs
6.GetEventByIdEndpoint: EventErrors.NotFound.ToProblem()
7.Program.cs: JsonStringEnumConverter and
TimeProvider.System
8.Postman: Create event,  Get event by id, Get event -not found
In the repo
Branch lecture-03: the starting point.
Tag lecture-03-done: the finished code, with
the exercise solved.
The Create event request now also sends
description, type.
CampusHub · Lecture 03 · Rich Domain and Result

Common mis takes
MistakeWhy it hurtsIn CampusHub
Exceptions for normal business flowsthe method signature does not show that it
can fail
Res ult<Event> and Error
DateTime.UtcNow called in the domainthe rule cannot be tested with a chosen timenowUtc as a parameter,  from TimeP rovider
Rules written in the endpointthey scatter and get duplicated across
endpoints
E vent.Create and Validate in the domain
CampusHub ·  Lecture 03 · Rich Domain and Res ult

Exercise: "must not start in the past"
An event cannot  start in the past. Add the rule in the
domain, without touching the endpoint.
The current time comes from the nowUtc parameter,
not from DateTime.UtcNow.
## Criteria
•the rule is added in Event.Validate
•new error: EventErrors.StartInPast
•it is a Validation-type error
•CreateEventEndpoint stays unchanged
Solution: src/CampusHub.Api/Domain/Events/Event.cs (Validate) and EventErrors.cs · tag lecture-03-done

R es ourc es
Anemic  Domain Model (Martin F owler)
martinfowler.com/bliki/AnemicDomainModel.html
TimeP rovider (Mic ros oft L earn)
learn.micros oft.com/en-us /dotnet/s tandard/datetime/timeprovider-overview
R ec ord types
learn.micros oft.com/en-us /dotnet/cs harp/language-reference/builtin-types /record
R es pons es  in Minimal AP Is  (TypedR es ults )
learn.micros oft.com/en-us /as pnet/core/fundamentals /minimal-apis /res pons es
R F C 9457: P roblem Details  for HTTP  AP Is
rfc-editor.org/rfc/rfc9457.html
Lecture code: s rc/CampusHub.Api · branch lecture-03 · tag lecture-03-done