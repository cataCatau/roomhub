

## CAMPUSHUB · BUSINESS APPLICATIONS  W ITH .NET 10
## Lecture 04
EF Core + SQLite
DbContext, IEntityTypeConfiguration, migrations and seed: events no longer
disappear when the application restarts
Code:  src/ CampusHub.Api · branch lecture-04 · tag lecture-04-done

L ecture objectives
## 01
You configure E F  Core
Campus HubDbContext with DbS ets  and S QLite,
regis tered in DI as  S coped.
## 02
You map the domain
IE ntityTypeConfiguration: keys , lengths , enum as
text, DateR ange in the Events  table.
## 03
You generate the schema
Migrations  with dotnet ef and tes t data (s eed) in
## Development.
## 04
You save and read
S aveChanges Async when writing, As NoTracking
when reading without changes .
CampusHub · Lecture 04 · E F  Core +  S QLite

R ecap:  lecture 3
Event.Create
the only way to a valid Event: no public constructor, no public setters
Result<T>
business failures are values (E rror), not exceptions
DateRange
value object for an interval, created via Create, with Overlaps
TimeProvider
time comes from DI, not from DateTime.Now
InMemoryEventStore
in-memory data, temporary: gone today
CampusHub · Lecture 04 · E F  Core +  S QLite

DbContext: the entry point
DbSetper entity: Events and
## Participants.
No configuration here.Each entity
has its own class.
ApplyConfigurationsFromAssembly
finds them automatically.
public sealed classCampusHubDbContext(
Db Conte xtOp ti ons< Ca mpus Hub Db Conte xt>  op ti ons)
: DbContext(options)
## {
publicDbSet<Event> Events => Set<Event>();
publicDb Se t<Par ticipa nt> Par ticipa nts  =>
Set<Participant>();
protected override voidOnM odel Crea ti ng (
ModelBuilder modelBuilder) =>
// Găsește au toma t  toa te clasele
// I Enti tyType Configura ti on<T> di n aces t asse mbly.
modelBuilder.ApplyConfigurationsFromAssembly(
typeof(Ca mpus Hub Db Context). Asse mbly) ;
## }
CampusHub · Lecture 04 · EF Core +  SQLiteSimplified excerpt: Infrastructure/ Persistence/ CampusHubDbContext.cs

IE ntityTypeC onfiguration:  E vent
One class per entity.Mappings do
not pile up in the DbContext.
ValueGeneratedNever.The Id
comes  from the domain.
E num as tex t.The database stays
readable.
public sealed classEventConfiguration
: IEntityTypeConfiguration<Event>
## {
public voidConfigure(EntityTypeBuilder<Event> builder)
## {
builder.ToTable("Events");
builder.HasKey(e => e.Id);
// Id-ul se generează în domeniu, nu în baza de date.
builder.Property(e => e.Id).ValueGeneratedNever();
builder.Property(e => e.Title).IsRequired()
.HasMaxLength(Event.MaxTitleLength);
// Enum-urile se salvează ca text.
builder.Property(e => e.Type)
.HasConversion<string>().HasMaxLength(20);
/* ... Status la fel, apoi Range ... */
## }
## }
CampusHub · Lecture 04 · E F  Core +  S QLiteS implified excerpt: Infrastructure/Persistence/Configurations/E ventConfiguration.cs

DateRange in the Events table
ComplexProperty.No separate
table for DateRange.
SQLite stores text.EF would read
DateTimeKind.Unspecified.
Convertorulmarks the values it
reads as UTC.
// EventConfiguration.cs
builder.ComplexProperty(e => e.Range, range =>
## {
range.Property(r => r.StartUtc)
.HasColumnName("StartUtc")
.HasConversion<UtcDateTimeConverter>();
range.Property(r => r.EndUtc)
.HasColumnName("EndUtc")
.HasConversion<UtcDateTimeConverter>();
## });
// UtcDateTimeConverter.cs
public sealed classUtcDateTimeConverter()
: ValueConverter<DateTime, DateTime>(
value => value,
value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
CampusHub · Lecture 04 · EF Core +  SQLiteSimplified excerpt: Infrastructure/ Persistence/ Configurations/ EventConfiguration.cs

R egistration in Program.cs
S coped.A new instance for each
HTTP request.
Connection stringfrom
apps ettings.js on, key Campus Hub.
R ead on first use.Tests can override
it (lecture 1 1 ).
builder.Services.AddSingleton(TimeProvider.System);
// DbContext-ul este Scoped: o instanță nouă
// pentru fiecare cerere HTTP.
builder.Services.AddDbContext<CampusHubDbContext>(
(serviceProvider, options) =>
options.UseSqlite(serviceProvider
.GetRequiredService<IConfiguration>()
.GetConnectionString("CampusHub")));
varapp = builder.Build();
awaitapp.InitializeDatabaseAsync();
CampusHub · Lecture 04 · E F  Core +  S QLiteS implified excerpt: Program.cs

Writing:  C reateE vent
DbContextcomes  through DI, as  a
parameter of the endpoint.
Addonly marks  the entity.
S aveChangesAs yncsends the
changes to the database.
pri va te s ta tic asy ncTask<Results<Created<Response>,
Pr oble mHttpResul t>>  Ha ndle (
Request request,
Ca mpus Hub Db Conte xt db,
TimeProvider time,
Ca ncella ti onT oke n c t)
## {
/* ... Event.Create; on failure: result.Error.ToProblem() */
varev  = resul t.Value ;
db.Events.Add(ev);
awaitdb.Sa ve Cha nges Asy nc (c t) ;
retur nTypedResults.Created(
$"/api/events/{ev.Id}",
ne wResponse(ev.Id, ev.Title, ev.Status));
## }
CampusHub · Lecture 04 · E F  Core +  S QLiteS implified excerpt: F eatures/E vents/CreateE vent/CreateE ventE ndpoint.cs

Reading: GetEventById
AsNoTracking.We only read, so EF
does not track the entity.
FirstOrDefaultAsyncreturns null if it
does not exist.
null becomes 404via
EventErrors.NotFound.
private static asyncTask<Results<Ok<Response>,
ProblemHttpResult>> Handle(
Guid id,
CampusHubDbContext db,
CancellationToken ct)
## {
// AsNoTracking: doar citim, EF nu trebuie să
// urmărească entitatea.
varev = awaitdb.Events
.AsNoTracking()
.FirstOrDefaultAsync(e => e.Id == id, ct);
if(ev is null)
returnEventErrors.NotFound.ToProblem();
/* ... TypedResults.Ok(new Response(...)) */
## }
CampusHub · Lecture 04 · EF Core +  SQLiteSimplified excerpt: Features/ Events/ GetEventById/ GetEventByIdEndpoint.cs

Migrations: from model to schema
Generated live.Migrations are not
committed to the repo.
With migrations:MigrateAsync.
Without migrations:
EnsureCreatedAsync, so the cloned
repo works right away.
#  ter mi nal  (R EADM E. md)
dotnet tool install --global dotnet-ef
dotne t ef  migra ti ons add I ni tial Crea te \
--project src/CampusHub.Api \
--s tar tup-pr ojec t src /Ca mpus Hub. Api \
--output-dir Infrastructure/Persistence/Migrations
dotne t ef da tabase upda te \
--project src/CampusHub.Api \
--s tar tup-pr ojec t src /Ca mpus Hub. Api
// DatabaseInitializer.cs
if(db.Database.GetMigrations().Any())
awaitdb.Da tabase.Migra te Asy nc() ;
else
awaitdb.Da tabase. Ensure Crea tedAsy nc () ;
CampusHub · Lecture 04 · EF Core +  SQLiteSimplified excerpt:  README.md, Infrastructure/ Persistence/ DatabaseInitializer.cs

S eed:  test data
Development only.Checked with
IsDevelopment().
Idempotent.If events  exis t, it adds
nothing.
Via E vent.Create.Only succes sful
results are saved.
if(app.Environment.IsDevelopment())
## {
vartime = scope.ServiceProvider
.GetRequiredService<TimeProvider>();
awaitSeedAsync(db, time);
## }
private static asyncTask SeedAsync(
CampusHubDbContext db, TimeProvider time)
## {
if(awaitdb.Events.AnyAsync())
return;
/* ... samples: 3 x Event.Create ... */
foreach(varsample insamples.Where(s => s.IsSuccess))
db.Events.Add(sample.Value);
awaitdb.SaveChangesAsync();
## }
CampusHub · Lecture 04 · E F  Core +  S QLiteS implified excerpt: Infrastructure/Persistence/Databas eInitializer.cs

Live coding: the steps
1.The EF Core Sqlite and Design packages in the .csproj
2.CampusHubDbContext with DbSet< Event>  and
DbSet< Participant>
3.EventConfiguration, with ComplexProperty for DateRange
4.UtcDateTimeConverter for dates read from SQLite
5.Program.cs: AddDbContext and ConnectionStrings
6.CreateEvent and GetEventById on the DbContext; delete
InMemoryEventStore
7.dotnet ef migrations add InitialCreate, then database update
8.DatabaseInitializer: MigrateAsync or EnsureCreatedAsync, seed
In the repo
Branch lecture-04: the starting point.
Tag lecture-04-done: the finished code, with
the exercise.
Run: dotnet run --project
src/CampusHub.Api
CampusHub · Lecture 04 · EF Core +  SQLite

Common mistakes
MistakeWhy it hurtsIn CampusHub
DbContext registered as a Singletonit is not designed for concurrent requestsAddDbContext: Scoped, one instance per
request
Add without SaveChangesAs yncAdd only marks the entity, it does not write
to the database
db.Events.Add(ev), then await
db.SaveChangesAs ync(ct)
SQLite DateTime used as isEF reads it with Kind UnspecifiedUtcDateTimeConverter on StartUtc and
EndUtc
CampusHub · Lecture 04 · E F  Core +  S QLite

E x ercise:  ParticipantConfiguration
Write the E F Core configuration for Participant,
following the model of EventConfiguration.
The DbContext finds  it on its  own, via
ApplyConfigurationsFromAssembly.
## Criteria
•the Participants table
•Id key, ValueGeneratedNever
•FullName required, with a maximum length
•E mail required, 254 characters
•unique index on Email
S olution: Infrastructure/Persistence/Configurations/ParticipantConfiguration.cs · tag lecture-04-done

R esources
Migrations in EF  Core (Microsoft Learn)
learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
Complex  types (DateR ange as a value object)
learn.microsoft.com/en-us/ef/core/modeling/complex-types
Tracking and no-tracking queries
learn.microsoft.com/en-us/ef/core/querying/tracking
S QL ite provider limitations
learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
Dependency injection in .NE T (lifetimes)
learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
Lecture code: branch lecture-04 · tag lecture-04-done