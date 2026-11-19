

## CAMPUSHUB · BUSINESS APPLICATIONS WITH .NET 10
## Lecture 01
OOP in C#
Encapsulation, inheritance, polymorphism, interfaces, records and composition,
using campus events as the model
Code: samples/CampusHub.Oop · tag lecture-01-done

Lecture objectives
## 01
Model the domain
Classes for participant, event and time range, named
in the language of the business.
## 02
Protect invariants
An object's state cannot become invalid from the
outside: a validating constructor, private set.
## 03
Extend without modifying
Add a new event type without touching existing code
(the Open/Closed principle).
## 04
Inheritance or composition
Inheritance for the “is a” relationship, composition
for behavior that can change.
CampusHub · Lecture 01 · OOP in C#

The CampusHub domain
TypeRole in the applicationConcept
Participantthe registered student, with a valid name and
email
encapsulation
Event (abstract)the part shared by all eventsinheritance
Workshop, Hackathon, Lecture, Webinarthe concrete event typespolymorphism
DateRangethe time span of an event, immutablerecord
IPricingPolicythe price calculation strategycomposition
INotifier, RegistrationServicenotifications and the registration flowinterfaces, manual DI
CampusHub · Lecture 01 · OOP in C#

## Encapsulation: Participant
The constructor validates. An
invalid Participant cannot exist.
private set. State changes only
through methods of the class.
ChangeEmail goes through the
same validation as creation.
public sealed class Participant
## {
public string Email { get; private set; }

public Participant(string fullName, string email)
## {
if (string.IsNullOrWhiteSpace(fullName))
throw new ArgumentException(
"Numele este obligatoriu.", nameof(fullName));
## </ <<.
Email = NormalizeEmail(email);
## }

public void ChangeEmail(string newEmail) <>
Email = NormalizeEmail(newEmail);
## }
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: samples/CampusHub.Oop/Participant.cs

Inheritance and abstraction: Event
abstract. You cannot write new
Event(...): an event “in general” does
not exist.
The shared part (title, range,
capacity, registrations) lives in the
base class.
Each concrete type adds only what
sets it apart, for example TeamSize.
public abstract class Event
## {
protected Event(string title, DateRange range,
int capacity, IPricingPolicy pricing) { <* <<. </ }

public abstract string Category { get; }
public virtual string Describe() { <* <<. </ }
## }

public sealed class Hackathon(string title, DateRange range,
int capacity, IPricingPolicy pricing, int teamSize)
: Event(title, range, capacity, pricing)
## {
public int TeamSize { get; } = teamSize;
public override string Category <> "Hackathon";
## }
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: Event.cs, EventTypes.cs

## Polymorphism: Describe()
Same call, different behavior. The
loop does not know the concrete
type of each event.
virtual in the base class, override in
derived classes, with
base.Describe() for the shared part.
New code without ifs on type: a
new type goes into the list and just
works.
## </ Hackathon
public override string Describe() <>
$"{base.Describe()} | echipe de {TeamSize}";

## </ Program.cs
var events = new List<Event> { <* Workshop, Hackathon, <<. </ };

foreach (var ev in events)
## {
Console.WriteLine(ev.Describe());
## }
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: EventTypes.cs, Program.cs

Interfaces and manual DI: INotifier
We depend on an abstraction, not
on Console. Tomorrow it could be
email or SMS.
The dependency comes in through
the constructor. No container, no
magic.
Lecture 2: the DI container in
ASP.NET Core takes over the same
role.
public interface INotifier
## {
void Notify(Participant recipient, string message);
## }

public sealed class RegistrationService(INotifier notifier)
## {
public bool Register(Event ev, Participant participant)
{ <* TryRegister + notifier.Notify(<<.) </ }
## }

## </ Program.cs
var service = new RegistrationService(new ConsoleNotifier());
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: INotifier.cs, RegistrationService.cs

Records and immutability: DateRange
Value equality. Two ranges with the
same endpoints are equal.
Immutable. Get-only properties,
validated once in the constructor.
Overlaps uses half-open ranges:
10–12 and 12–14 do not overlap;
10–12 and 11–13 do.
public sealed record DateRange
## {
public DateTime Start { get; }
public DateTime End { get; }

public DateRange(DateTime start, DateTime end)
## {
if (end <= start)
throw new ArgumentException(<* <<. </);
Start = start;
End = end;
## }

public bool Overlaps(DateRange other) <>
Start < other.End <& other.Start < End;
## }
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: samples/CampusHub.Oop/DateRange.cs

Composition over inheritance: pricing
Pricing is not a hierarchy.
FreeWorkshop, PaidWorkshop,
DiscountedWorkshop... would
multiply the classes.
The strategy plugs in to the event
through the constructor.
Early-bird Hackathon: 40 RON with
a 25% discount means 30.00 RON
until the deadline, 40.00 RON after.
public interface IPricingPolicy
## {
decimal PriceFor(
Participant participant, DateTime registeredAt);
## }

public sealed class EarlyBirdPricing(
decimal amount, decimal discountPercent, DateTime deadline)
: IPricingPolicy
## {
public decimal PriceFor(
Participant participant, DateTime registeredAt) <>
registeredAt <= deadline
? Math.Round(amount * (1 - discountPercent / 100m), 2)
: amount;
## }
CampusHub · Lecture 01 · OOP in C#Simplified excerpt: samples/CampusHub.Oop/IPricingPolicy.cs

Live coding: the steps
1.Console project: dotnet new console -n CampusHub.Oop
2.Participant: validating constructor, private set
3.DateRange as a record, with Overlaps
4.Abstract Event, then Workshop, Hackathon, Lecture
5.IPricingPolicy and the three pricing policies
6.INotifier and RegistrationService
7.Program.cs: events, registrations, prices
In the repo
Branch lecture-01: the starting point.
Tag lecture-01-done: the solved code, with
the exercise.
Run: dotnet run in the
samples/CampusHub.Oop folder.
CampusHub · Lecture 01 · OOP in C#

Common mistakes
MistakeWhy it hurtsIn CampusHub
Public setter on every propertyanyone can break the invariant from
outside
private set and methods like ChangeEmail
Class hierarchy for pricing variantsthe number of classes grows
combinatorially
IPricingPolicy, through composition
Internal collection exposed as a Listcallers add items without following the
rules
IReadOnlyList and TryRegister
CampusHub · Lecture 01 · OOP in C#

Exercise: the Webinar type
Add an online event with an access link, without
modifying Event, Workshop, Hackathon or Lecture.
The Open/Closed principle: we extend by adding, not
by modifying.
## Criteria
## •inherits Event
•has a Uri JoinUrl property
•Category returns “Webinar”
•Describe() adds the link
•add it to the list in Program.cs
Solution: samples/CampusHub.Oop/Webinar.cs · tag lecture-01-done

## Resources
Object-oriented programming in C# (Microsoft Learn)
learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/
Polymorphism in C#
learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/polymorphism
Record types
learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record
Dependency injection in .NET (preparation for lecture 2)
learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
JetBrains Rider documentation
jetbrains.com/help/rider/Introduction.html
Lecture code: samples/CampusHub.Oop · tag lecture-01-done