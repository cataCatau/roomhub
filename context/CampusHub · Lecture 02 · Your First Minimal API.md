

C A M P U S HU B   ·   B U S IN E S S   A P P L I C A T ION S   WITH  .N E T   1 0
## Lecture 02
Y our F irs t Minimal AP I
Minimal APIs , dependency injection, TypedR esults, your first vertical slice and
your first Postman collection
Code: s rc/Campus Hub.Api ·  tag lecture-02-done

Lecture objectives
## 01
Start a minimal API
Program.cs with WebApplication, a MapGet endpoint
and running on localhost:5080.
## 02
Use DI
Register a service and receive it as a parameter in
the handler.
## 03
Respond with TypedResults
The possible status codes appear in the handler's
return type.
## 04
Organize by slices
One folder per feature and a Postman collection that
calls it.
CampusHub · Lecture 02 · Your  First Minimal API

R ec ap: lec ture 1
Enc aps ulation.A cons tructor that validates , and a private s et.
Inheritanc e and polymorphis m.An abs tract E vent, new types  without modifying the old code.
Interfac es  and compos ition.IP ricingP olicy changes  the price without a new hierarchy.
record.DateR ange, an immutable value object.
Manual DI.INotifier received through the cons tructor. Today the AS P.NE T Core container does  this .
CampusHub ·  Lecture 02 · Your  Firs t Minimal API

Minimal API: Program.cs
Builder.We register services before
## Build().
MapGet.An endpoint is a route and
a handler.
MapEvents().Feature routes are
declared outside Program.cs.
varbuilder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InMemoryEventStore>();
varapp = builder.Build();
app.MapGet("/", () => "CampusHub API is running.");
app.MapEvents();
app.Run();
CampusHub · Lecture 02 · Your  First Minimal APISimplified excerpt: src/CampusHub.Api/Program.cs

DI: InMemor yE ventS tore
AddS ingleton.A s ingle ins tance for
the lifetime of the application.
Injec tion.The handler asks for the
type as a parameter, the container
provides it.
Conc urrentDic tionar y.Concurrent
HTTP reques ts , the s ame object.
public sealed recordEventRecord(
Guid Id, stringTitle, DateTime StartUtc,
DateTime EndUtc, intCapacity);
public sealed classInMemoryEventStore
## {
private readonlyConcurrentDictionary<Guid, EventRecord>
_events = new();
public voidAdd(EventRecord record) =>
_events[record.Id] = record;
publicEventRecord? Find(Guid id) =>
_events.TryGetValue(id, out varrecord) ? record : null;
## }
CampusHub ·  Lecture 02 · Your  Firs t Minimal APISimplified excerpt: Infras tructure/InMemoryE ventStore.cs

TypedResults: CreateEvent
Results< A, B> .The return type lists
the possible responses.
BadRequest.Empty title, invalid
interval or non-positive capacity.
Created.A 201 response with the
address of the new resource.
pri vate st aticResults<Created<Response>,
B adRequest <string>>  Handle(
Request request,
In Me mo ryEvent Sto re sto re)
## {
if(string.IsNullO rWhite Sp ace( request. Title))
retu rnTypedResults.BadRequest(
"Titlul este obligatoriu.");
// ... celel alte  validări,  apoi sto re.Add( reco rd);
retu rnTypedResults.Created(
$"/api/events/{record.Id}", response);
## }
CampusHub · Lecture 02 · Your  First Minimal APISimplified excerpt: Features/Events/CreateEvent/CreateEventEndpoint.cs

R oute with a parameter: GetE ventB yId
{id:guid}.Route constraint, the id
must be a G uid.
Ok or NotFound.B oth cas es  are in
the return type.
WithName.The endpoint name,
used for links and OpenAP I.
public st atic  voidMap(RouteGroupBuilder group) =>
g roup. MapGet("/{id:guid}", Handle)
.WithName("Get EventB yId ");
pri vate st aticResults<Ok<Response>, NotFound> Handle(
Guid id,  In Me mo ryEvent Sto re sto re)
## {
varreco rd = sto re.Find(id);
if(record is null)
returnTypedResults.NotFound();
retu rnTypedResults.Ok(
newResponse(/* ... */));
## }
CampusHub ·  Lecture 02 · Your  Firs t Minimal APISimplified excerpt: Features /E vents /G etE ventById/G etE ventByIdE ndpoint.cs

Vertical slice: Features/Events
One folder per feature.
CreateEvent and GetEventById.
The slice declares its routes.The
Map method receives the group.
No shared types.Each slice has its
own Request and Response.
public static classEventsEndpoints
## {
public staticIEndpointRouteBuilder MapEvents(
thisIEndpointRouteBuilder app)
## {
vargroup = app.MapGroup("/api/events")
.WithTags("Events");
CreateEventEndpoint.Map(group);
GetEventByIdEndpoint.Map(group);
returnapp;
## }
## }
CampusHub · Lecture 02 · Your  First Minimal APISimplified excerpt: Features/Events/EventsEndpoints.cs

What the AP I returns
Reques tRes ult in codeStatus
POS T /api/events, valid dataCreated with the address /api/events/{id}201
POS T /api/events, empty title, invalid interval or
non-positive capacity
BadRequest with the error mess age400
GET /api/events/{id}, existing idOk with Res ponse200
GET /api/events/{id}, nonexistent idNotFound404
CampusHub · Lecture 02 · Your  Firs t Minimal API

The P os tman c ollec tion
1 . Create event.P OS T /api/events ,
s aves  eventId (expected 201).
- Get event by id.GE T
/api/events /{{eventId}} (expected
## 200).
- Get event -not found.GE T with
a new {{$guid}} (expected 404).
POST{{baseUrl}}/api/events
Content-Type: application/json
## {
"title": "Introducere in Git",
"startUtc": "{{startUtc}}",
"endUtc": "{{endUtc}}",
## "capacity": 2
## }
// baseUrl = http://localhost:5080
CampusHub ·  Lecture 02 · Your  Firs t Minimal APISimplified excerpt: pos tman/CampusHub.pos tman_ collection.js on

L ive c oding: the s teps
1.Web project (Web SDK), net10.0, in src/CampusHub.Api
2.P rogram.cs: MapGet on "/" and running on localhos t:5080
3.InMemoryEventS tore and AddSingleton
4.CreateE ventE ndpoint: R eques t, R es pons e, Handle
5.Events E ndpoints : MapGroup and MapEvents
6.P os tman collection: Create event, then Get event by id
7.The exercise: GetEventById with 404
In the repo
Branch lecture-02: the starting point.
Tag lecture-02-done: the finished code,
including the exercise.
Run: dotnet run --project
src/CampusHub.Api
CampusHub ·  Lecture 02 · Your  Firs t Minimal API

Common mis takes
Mistak eWhy it hurtsIn Campus Hub
Plain Dictionary in a Singletonconcurrent requests can corrupt the dataConcurrentDictionary in
InMemoryEventStore
Res ponse shared between featuresa change in one place breaks anothereach slice  has its own Response
Ok(null) when the resource is missingthe client cannot tell "does not exist" from
## "exists"
NotFound, i.e. 404
CampusHub ·  Lecture 02 · Your  Firs t Minimal API

E x erc is e: GE T /api/events /{id}
Add the endpoint that returns  an event by id, with 404
when it does  not exis t.
A new s lice, with its  own R es pons e, regis tered in
E vents E ndpoints.
## Criteria
•folder Features /E vents /GetE ventById
•R es ults <Ok<R es pons e>, NotF ound>
•a nonexis tent id returns 404
•route regis tered in MapE vents
•new reques t in Pos tman
Solution: s rc/CampusHub.Api/Features /E vents /G etE ventById/ ·  tag lecture-02-done

R es ourc es
Minimal APIs , overview (Micros oft Learn)
learn.micros oft.com/en-us /as pnet/core/fundamentals /minimal-apis /overview
R oute handlers  in Minimal AP Is
learn.micros oft.com/en-us /as pnet/core/fundamentals /minimal-apis /route-handlers
R espons es  and TypedR es ults
learn.micros oft.com/en-us /as pnet/core/fundamentals /minimal-apis /res pons es
Dependenc y injec tion in .NE T
learn.micros oft.com/en-us /dotnet/core/extensions /dependency-injection
Vertic al S lic e Arc hitec ture (J immy B ogard)
jimmybogard.com/vertical-s lice-architecture/
Collec tion runs  in P os tman
learning.pos tman.com/v11/docs /tes ts-and-s cripts /running-collections /intro-to-collection-runs
Lecture code: tag lecture-02-done ·  paths on the slides  are relative to src/CampusHub.Api