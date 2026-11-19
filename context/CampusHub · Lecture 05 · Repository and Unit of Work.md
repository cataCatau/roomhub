

## CAMPUSHUB · BUSINESS APPLICATIONS  W ITH .NET 10
## Lecture 05
Repository and Unit of Work
We take EF Core out of the endpoint and talk honestly about what we gain and
what we pay
Code: src/ CampusHub.Api · tag lecture-05-done

L ecture objectives
## 01
S ee the problem
The CreateE vent endpoint depends  directly on
Campus HubDbContext, that is , on EF  Core.
## 02
Define contracts
Interfaces  in Domain: IE ventR epos itory,
IP articipantR epository and IUnitOfWork.
## 03
Implement and regis ter
Clas s es  in Infras tructure, regis tered as  S coped in
P rogram.cs s o they s hare the s ame DbContext.
## 04
Weigh the cos ts
When an extra layer is  worth it and when we us e
DbContext directly.
CampusHub · Lecture 05 · R epository and Unit of Work

R ecap:  lecture 4
DbContex t.CampusHubDbContext exposes DbSet<E vent> and DbSet<Participant>.
Configurations .One IEntityTypeConfiguration per entity, discovered with ApplyConfigurationsFromAssembly.
S QL ite.The “CampusHub” connection string comes from configuration, and the DbContext is Scoped.
Migrations  and seed.Migrations  are generated live; without them, the s chema comes  from E ns ureCreated. In
Development, tes t events  are s eeded.
Left open.CreateE vent worked directly with db.E vents and db.S aveChangesAsync.
CampusHub · Lecture 05 · R epository and Unit of Work

The problem:  an endpoint w ith DbC ontex t
Dependency on E F  Core.The
endpoint knows  about
Campus HubDbContext.
Hard to test in is olation.Handle
requires  a real
Campus HubDbContext.
Lecture 4 said  so.It was  the s tarting
option.
private static asyncTask<
Results<Created<Response>, ProblemHttpResult>> Handle(
Request request,
CampusHubDbContext db,
TimeProvider time,
CancellationToken ct)
## {
/* ... Event.Create(...) ... */
varev = result.Value;
db.Events.Add(ev);
awaitdb.SaveChangesAsync(ct);
returnTypedResults.Created(/* ... */);
## }
CampusHub · Lecture 05 · R epository and
Unit of Work
S implified excerpt (tag lecture-04-done):
src/CampusHub.Api/F eatures/E vents/CreateE vent/CreateE ventE ndpoint.cs

Repository: the contract in Domain
Lives in Domain.The interface does
not mention EF Core.
Only what we need.A lookup by id
and an add.
Add does not save.Saving stays
with the Unit of Work.
public interf aceIEventRepository
## {
Task<Event?> GetByIdAsync(
Guidid, CancellationT ok en cancellationT ok en);
voidAdd(Event ev);
## }
CampusHub · Lecture 05 · Repository and Unit of WorkSimplified excerpt: src/ CampusHub.Api/ Domain/ Events/ IEventRepository.cs

Implementation:  E ventR epos itor y
In Infras tructure.This is the only
place with F irstOrDefaultAsync.
Receives  the DbContex t.Through
the primary constructor, from DI.
Add only marks .No SQL command
runs until SaveChangesAsync.
public sealed classEventRepository(CampusHubDbContext db)
:  IE ventRepository
## {
publicT ask<E vent?> GetByIdA sync(
Guidid, CancellationToken cancellationToken) =>
db.E vents.F irstO rDef aultA sync(
e => e.Id == id, cancellationToken);
public voidA dd(E vent ev) => db.E vents.A dd(ev);
## }
CampusHub · Lecture 05 · R epository and
Unit of Work
S implified excerpt: src/CampusHub.Api/Infrastructure/P ersistence/R epositories/E ventR epository.cs

Unit of Work: the DbContext, under another name
A single method.
SaveChangesAsync saves
everything that changed.
We build nothing new.
CampusHubDbContext implements
the interface.
Contract in Domain.The endpoint
no longer sees the DbContext class.
//  Domain/ Common/ IU nitOfW ork. cs
public interfaceIU nitOfW ork
## {
T ask<int> SaveChangesA sync(
CancellationToken cancellationToken = def ault);
## }
// Persistence/CampusHubDbContext.cs
public sealed classCampusHubDbContext(
DbContextO ptions< CampusHubDbContext> options)
: DbContext(options), IUnitOfWork
## {
/*  DbSet-uri, O nM odelCreating */
## }
CampusHub · Lecture 05 · Repository and
Unit of Work
Simplified excerpt: src/ CampusHub.Api/ Domain/ Common/ IUnitOfWork.cs,
src/ CampusHub.Api/ Infrastructure/ Persistence/ CampusHubDbContext.cs

DI regis tration:  ever ything S coped
S coped.One instance per HTTP
request, like the DbContext.
The s ame DbContex t.The
repositories and IUnitOfWork share
the instance.
IUnitOfWork via lambda.It asks DI
for CampusHubDbContext itself.
builder. Services.A ddDbContext< CampusHubDbContext>(/* ... */);
builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
serviceProvider. GetReq uiredService< CampusHubDbContext>());
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder. Services.A ddScoped< IParticipantRepository,
ParticipantRepository>();
CampusHub · Lecture 05 · R epository and Unit of WorkS implified excerpt: src/CampusHub.Api/P rogram.cs

The endpoint, after
Two abstractions.IEventRepository
and IUnitOfWork instead of db.
Parameters from DI.Minimal API
injects them into Handle.
The rest is unchanged.
Event.Create and the Created
response stay the same.
private static asyncTask<
Results< Created< Response>, ProblemHttpResult>> Handle(
Request request,
IE ventRepository events,
IUnitOfWork unitOfWork,
T imeProvider time,
CancellationToken ct)
## {
/*  ...  E vent. Create(... ) ...  */
varev = result.Value;
events.A dd(ev);
aw aitunitOfWork.SaveChangesAsync(ct);
returnT ypedResults. Created(/* ... */);
## }
CampusHub · Lecture 05 · Repository and
Unit of Work
Simplified excerpt: src/ CampusHub.Api/ Features/ Events/ CreateEvent/ CreateEventEndpoint.cs

Direction of dependencies
F eatures
CreateEventEndpoint
depends on
## Domain
IEventRepository
IUnitOfWork
implements
Infras tructure
EventRepository
CampusHubDbContext
The interfaces in Domain do not mention EF  Core. Only Infrastructure knows about it.
CampusHub · Lecture 05 · R epository and Unit of Work

Discussion: gain and cost
What we gain
Handlers do not depend on EF
Core, so they can be tested with
a hand-written fake.
All data access for commands
(writes) goes through the
repository.
What we pay
Extra code: interfaces and
classes.
The risk of a generic repository
that merely forwards calls.
Our decision
Repository on the write side
## (commands).
On the read side (queries,
lecture 7): DbContext directly,
with projections.
CampusHub · Lecture 05 · Repository and
Unit of Work
Simplified excerpt: src/ CampusHub.Api/ Infrastructure/ Persistence/ Repositories/ EventRepository.cs

Live coding: the steps
1.IUnitOfWork in Domain/Common
2.IEventRepository in Domain/ Events
3.EventRepository in Persistence/ Repositories
4.CampusHubDbContext implements IUnitOfWork
5.Scoped registrations in Program.cs
6.CreateEventEndpoint: repository and unit of work instead of db
7.Discussion of costs (about 10 minutes)
In the repo
Branch lecture-05: the starting point.
Tag lecture-05-done: the solved code, with
the exercise.
The difference: git diff lecture-04-done
lecture-05-done --stat
CampusHub · Lecture 05 · Repository and Unit of Work

Common mis tak es
Mistak eWhy it hurtsIn CampusHub
A generic repository that merely forwards
calls
extra code, with no rule addedonly the methods we need: GetByIdAsync
and Add
SaveChanges inside the repositoryyou can no longer save several changes
together
Add only marks, IUnitOfWork saves
Repository and Unit of Work with
different DbContexts
the changes are in one context, the save in
another
everything Scoped, the same instance per
request
CampusHub · Lecture 05 · R epository and Unit of Work

Exercise: IParticipantRepository
Write the contract for Participant, modeled on
IEventRepository, and its implementation.
Think about what a Participant needs: lookup by id,
email check, add.
## Criteria
•the interface lives in Domain/Participants
•Add does not save
•the implementation class lives in Repositories
•Scoped registration in Program.cs
Solution: src/ CampusHub.Api/ Domain/ Participants/ IParticipantRepository.cs · tag lecture-05-done

R es ources
R epos itor y (Martin F owler)
martinfowler.com/eaaCatalog/repository.html
Unit of Work (Martin F owler)
martinfowler.com/eaaCatalog/unitOfWork.html
Dependency injection in .NE T (lifetimes , S coped)
learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
E F  Core:  tracking and no-trac king queries
learn.microsoft.com/en-us/ef/core/querying/tracking
R oute handlers  in Minimal AP Is  (injected parameters )
learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers
Lecture code: branch lecture-05 (start) · tag lecture-05-done (solved)