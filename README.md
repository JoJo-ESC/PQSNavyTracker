# PQS Tracker

An ASP.NET Core Web API for tracking Personnel Qualification Standards (PQS)
sign-offs: a trainee works toward a qualification made up of individually
numbered line items, and a qualified supervisor signs off each one after the
trainee demonstrates it.

PQS content in this repository (the seeded "Reactor Operator" qualification
and its line items) is invented for demonstration purposes and does not
reflect the actual PQS program, terminology, or internal process of any
organization.

## Stack

- ASP.NET Core Web API (.NET 9, controller-based)
- EF Core with the SQLite provider, code-first, via migrations
- xUnit, tested against the service layer with the EF Core in-memory provider
- Swagger/OpenAPI (Swashbuckle) for manual exploration

The project targets net9.0 rather than net8.0 because only the .NET 9 and 10
SDKs were available in the environment this was built in. Nothing about the
design depends on the specific version.

## Running it

```
cd PqsTracker
dotnet run
```

On startup the app applies any pending EF Core migrations and, if the
database is empty, seeds one sample qualification with 15 line items. The
SQLite file (`pqstracker.db`) is created next to the project and is not
checked into source control.

Swagger UI is available at `/swagger` when running in the `Development`
environment.

## Running the tests

```
cd PqsTracker.Tests
dotnet test
```

## Project layout

```
PqsTracker/
  Controllers/   thin — parse the request, call a service, map the result to HTTP
  Services/      all business rules live here
  Data/          DbContext, seed data
  Models/        entities
  Dtos/          request and response shapes
  Program.cs
PqsTracker.Tests/
  Services/      unit tests against the service layer
```

## Data model

- **Qualification** — a named standard (e.g. "Reactor Operator") made up of
  `LineItem`s grouped into three sections: Fundamentals, Systems,
  Watchstations.
- **Trainee** — a person. Also the entity used for qualifiers: anyone who
  holds a qualification can sign others off on it.
- **SignOff** — records that a trainee was signed off on a line item by a
  qualifier. Never hard-deleted; "removing" one sets `RevokedAt` and
  `RevocationReason` instead, so the original record stays queryable.

## Business rules

Enforced in the service layer, not the controllers:

1. A qualifier cannot sign off their own line item.
2. A qualifier must already hold the qualification the line item belongs to.
3. A trainee cannot have two active (non-revoked) sign-offs for the same
   line item at once — though a new one is allowed after the prior one is
   revoked.
4. Sign-offs are append-only: revoking sets a timestamp and reason rather
   than deleting the row.
5. Completion and progress percentage are computed from the current set of
   active sign-offs on every request, never stored — so a revoked sign-off
   is reflected immediately with no stale cached state.

## API

```
GET    /api/qualifications
GET    /api/qualifications/{id}
POST   /api/qualifications
PUT    /api/qualifications/{id}
DELETE /api/qualifications/{id}

GET    /api/trainees
GET    /api/trainees/{id}
POST   /api/trainees

POST   /api/signoffs
POST   /api/signoffs/{id}/revoke

GET    /api/trainees/{traineeId}/progress/{qualificationId}
```

Rule violations return `400 Bad Request`. Missing resources return `404`.

## Out of scope

No authentication, no frontend, no PostgreSQL/Docker/deployment, no roles or
permissions beyond the qualifier check. This is a focused demonstration of
the data model and business rules, not a production system.
