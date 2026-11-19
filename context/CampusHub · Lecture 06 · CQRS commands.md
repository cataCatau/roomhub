

## CAM PUSHUB  ·  BUSIN ESS  APPLICATION S  WITH  .N ET  10
## Lecture 06
CQRS commands
Custom handlers, thin endpoints, and the RegisterForEvent use case
Code: src/ CampusHub.Api · tag lecture-06-done

L ecture objectives
## 01
Model c ommands
A command is  a reques t that changes  the s tate of
the s ys tem: a record that implements  ICommand.
## 02
Write handlers
A handler is  a us e cas e: it loads , calls  the domain,
s aves , and returns  a R es ult.
## 03
Keep the endpoint thin
The endpoint only does  reques t, command, handler,
HTTP  res pons e. No bus ines s  rules .
## 04
R egis ter in DI
AddHandlers  s cans  the as s embly once, at s tartup,
without MediatR .
CampusHub · Lecture 06 · CQR S  commands

Recap: lecture 5
Repository.IEventRepository and IParticipantRepository hide EF Core from the rest of the application.
Unit of Work.IUnitOfWork.SaveChangesAsync saves the changes made during a request.
Same DbContext.The repositories and the Unit of Work are Scoped and share the instance from the HTTP request.
Today:the endpoints still call the repository directly. We move their logic into handlers.
CampusHub · Lecture 06 · CQRS commands

The command and the handler contract
Command= a reques t that changes
s tate. It is  a record holding the data
of the us e cas e.
One handler per c ommand.The
contract has  a s ingle method,
## Handle.
R es ult, not ex ceptions .Bus ines s
errors  are returned as  values .
public interfaceICommand<TResult>
## {
## }
public interfaceICommandHandler<inTCommand, TResult>
whereTCommand :  IComman d<TRe sult >
## {
Ta sk <Re sult <TRe sult >>  Han dle(
TCommand command, CancellationToken cancellationToken);
## }
// din PublishEventHandler.cs
public sealed recor dPublishEventCommand(Guid EventId)
:  IComman d<Unit >;
CampusHub · Lecture 06 · CQR S  commandsSimplified excerpt: src/CampusHub.Api/Common/Cqrs/ICommandHandler.cs

Handler: PublishEvent
It orchestrates.It loads the
aggregate, calls the domain, saves.
The rule lives in Event.The handler
calls ev.Publish; it does not change
## Status.
Dependencies via the constructor:
repository, Unit of Work,
TimeProvider.
public sealed classPublishEventHandler(/*  ... * /)
:  IComman dHandler<Publi shEve ntComman d, Unit >
## {
public asyncTask <Re sult <Unit>> Han dle(/* ... */)
## {
varev = awaitevent s.GetByIdA sync(/* ... */);
if(ev is null)
returnEvent Err or s.N otFoun d;
varresult = ev.Publish(time.GetUtcNow().UtcDateTime);
if(result.IsFailure)
returnresult.Error;
awaitunitOfWork .Sa ve ChangesA sync(cancellati on Token );
returnUnit.Value;
## }
## }
CampusHub · Lecture 06 · CQRS commandsSimplified excerpt: src/ CampusHub.Api/ Features/ Events/ PublishEvent/ PublishEventHandler.cs

Thin endpoint
Four s teps :reques t, command,
handler, HTTP  res ponse.
The handler c omes  from DIas
ICommandHandler, not as  a
concrete clas s .
S uc c es s :204 No Content. Failure:
ToP roblem.
public static voidMap(RouteGroupBuilder group) =>
group.MapPost("/{id:guid}/publish", Handle)
.WithName("PublishEvent");
private static async
Task<Results<NoContent, ProblemHttpResult>> Handle(
Guid id,
ICommandHandler<PublishEventCommand, Unit> handler,
CancellationToken ct)
## {
varresult = awaithandl er.Handle(
newPublishEventCommand(id), ct);
if(result.IsFailure)
returnresult.Error.ToProblem();
returnTyped Results.No Content();
## }
CampusHub · Lecture 06 · CQR S  commandsSimplified excerpt: src/CampusHub.Api/F eatures/E vents/P ublishE vent/PublishE ventE ndpoint.cs

From E rror to HTTP  res pons e
Validationbecomes  400, NotFound
becomes  404, Conflic tbecomes
## 409.
One s ingle placefor the mapping,
us ed by all endpoints .
ProblemDetails(R F C 9457), with the
code extens ion.
public staticProblemHttpResult ToProblem(thisError error)
## {
varstatus = error.Type switch
## {
ErrorType.Validation => StatusCodes.Status400BadRequest,
ErrorType.NotFound => StatusCodes.Status404NotFound,
ErrorType.Conflict => StatusCodes.Status409Conflict,
_ => StatusCodes.Status500InternalServerError
## };
returnTypedResults.Problem(
detail: error.Message,
statusCode: status,
title: error.Code,
extensions: /* ... */);
## }
CampusHub · Lecture 06 · CQR S  commandsSimplified excerpt: src/CampusHub.Api/Common/ErrorHttpExtensions.cs

Automatic  handler regis tration
S c an at s tartup.E very clas s  that
implements  ICommandHandler is
added to DI.
S c oped,like the DbContext they us e.
In Program.c s :
builder.Services .AddHandlers (typeof(
P rogram).As s embly);
public staticIServiceCollection AddHandlers(
thisIServic eCollection servic es, Assembly assembly)
## {
Type[] ha ndl erCont racts = [typeof(IComma ndHa ndl er<,>)];
varimplementations = assembly.GetTypes()
.W here(type =>
type is{ IsClass: tru e, IsAbstract: false});
foreach(varimplem entation inimplem entations)
## {
varcont racts = implem entation.Get Int erfac es ()
.Where(/* ... */);
foreach(varcont ract incont racts)
services.AddScoped(contract, implementation);
## }
retu rnservices;
## }
CampusHub · Lecture 06 · CQR S  commandsSimplified excerpt: src/CampusHub.Api/Common/Cqrs/HandlerR egistrationExtensions.cs

RegisterForEvent (the simple version)
Two aggregates loaded:Event and
Participant, both with NotFound.
The rules in Event.Register:
published, not yet started, no double
registration, a free spot.
Waitlistonly comes in lecture 9.
varev = awaitevents.GetByIdAsync(/* ... */);
if(ev is null)
returnEventErrors.NotFound;
varparticipant = awaitparticipants.GetByIdAsync(/* ... */);
if(participant is null)
returnParticipantErrors.NotFound;
varresult = ev.Register(
participant.Id, time.GetUtcNow().UtcDateTime);
if(result.IsFailure)
returnresult.Error;
awaitunitOfWork.SaveChangesAsync(cancellationToken);
return newRegisterForEventResult(
result.Value.Id, result.Value.Status);
CampusHub · Lecture 06 · CQRS commands
Simplified excerpt:
src/ CampusHub.Api/ Features/ Registrations/ RegisterForEvent/ RegisterForEventHandler.cs

P os tman:  the reques ts  in the collection
R eques tMethod and routeS tatus
Create eventPOST /api/events201
Publish eventPOST /api/events/{{eventId}}/publish204
Create participantPOST /api/participants201
R egister for eventPOST /api/events/{{eventId}}/registrations201
R egister again -conflictPOST /api/events/{{eventId}}/registrations409
Cancel eventPOST /api/events/{{eventId2}}/cancel204
Order matters : the event is  publis hed before regis tering, otherwis e R egis ter returns  NotP ublished (409).
CampusHub · Lecture 06 · CQR S  commandsSimplified excerpt: postman/CampusHub.postman_ collection.json

L ive coding:  the s teps
1.ICommand and ICommandHandler in Common/Cqrs
2.AddHandlers  and the call in P rogram.cs
3.CreateE ventHandler and the CreateE vent endpoint, now thin
4.E vent.Cancel and CancelE vent (handler + endpoint)
5.R egis tration, R egis trationConfiguration and the new migration
6.E vent.R egis ter and Regis terF orE vent
7.The P os tman collection: publis h, regis ter, conflict
In the repo
Branch lecture-06: the s tarting point.
Tag lecture-06-done: the s olved code,
including the exercis e.
CampusHub · Lecture 06 · CQR S  commands

Common mis takes
MistakeWhy it hurtsIn Campus Hub
Busines s rule in the endpointyou cannot reuse it and it is hard to testthin endpoint, the rule in the handler and in
E vent
Exception for a “full event”the normal flow becomes an error flowR esult with R egistrationErrors.E ventFull,
then 409
The handler changes Status directlythe rule gets duplicated and can be
bypassed
ev.P ublish, ev.Cancel, ev.R egister
CampusHub · Lecture 06 · CQR S  commands

E x ercis e:  P ublis hE vent
Write the us e cas e that moves  an event from Draft to
P ublis hed, in the s tyle of the lecture: c ommand,
handler, thin endpoint.
The publis hing rule lives  in E vent, not in the handler.
## Criteria
•P ublis hE ventCommand(Guid E ventId)
•E vent.P ublis h(nowUtc) returning a R es ult
•P OST /api/events /{id}/publis h
•204 on s ucces s , 404 if it is  mis s ing
•409 if it is  already publis hed or cancelled
Solution: Features/E vents/P ublishE vent/ · tag lecture-06-done

R es ources
CQR S  (Martin Fowler)
martinfowler.com/bliki/CQR S .html
Vertic al s lice architec ture (J immy B ogard)
jimmybogard.com/vertical-slice-architecture/
R es pons es  in Minimal AP Is  (TypedR es ults )
learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses
Dependenc y injec tion in .NE T
learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
RFC 9457:  Problem Details  for HTTP  AP Is
rfc-editor.org/rfc/rfc9457.html
Lecture code: src/CampusHub.Api · tag lecture-06-done