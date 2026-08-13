# JAVLN Mood Tracker

A web app for tracking daily team mood — built with ASP.NET Core (.NET 8), Angular 18, and MySQL, per the take-home brief.

## Running the app

```
docker compose build
docker compose up
```

- Mood tracker: http://localhost:4200
- Admin view: http://localhost:4200/admin (admin key: `admin123`)
- Backend API/Swagger: http://localhost:8080/swagger

No manual database setup is required — the backend automatically applies EF Core migrations on startup, so the schema is created fresh the first time the app runs.

## Running locally without Docker (for development)

**Backend:**

```
cd InterviewProjectTemplate
dotnet run
```

Requires a MySQL instance reachable per `appsettings.Development.json` (e.g. `docker compose up mysql-db -d`).

**Frontend:**

```
cd Client/web-client
npm install
npm start
```

Runs on http://localhost:4200, pointed at the backend via `src/environments/environment.ts`.

**Tests:**

```
dotnet test
```

(from the project root)

## Architecture & key decisions

**Identifying users without authentication.** The brief requires the once-per-day rule to work without login. I used a browser-generated GUID stored in a long-lived, non-`HttpOnly` cookie, sent with each request. This is simple and effective for the stated scope, but has real tradeoffs worth naming: it resets if cookies are cleared, doesn't follow a user across devices/browsers, and isn't a substitute for genuine identity. An IP-based approach was considered and rejected — it breaks for shared/NAT'd networks and is generally a worse fit than a per-browser identifier.

**Admin page access.** The "no authentication" requirement is stated directly under the once-per-day tracking rule, so I read it as scoped to that flow rather than the whole app. The admin endpoint is gated behind a shared secret key (an `X-Admin-Key` header, checked against a value in configuration) rather than a real login/session system — this isn't authentication in the formal sense (no user accounts, no identity), but it does mean the data isn't openly accessible. I judged this a better fit for the brief's intent than leaving admin data fully public.

**CORS and credentials.** Since the frontend needs to send cookies cross-origin (Angular on one port, the API on another), CORS is configured with an explicit origin allow-list (`WithOrigins(...)`) and `AllowCredentials()`, rather than `AllowAnyOrigin()`, since browsers disallow combining wildcard origins with credentialed requests. In production this origin would need to come from configuration rather than being hardcoded.

**Automatic migrations on startup.** Rather than requiring a manual `dotnet ef database update` step, the backend calls `Database.Migrate()` on startup inside a DI scope. This was necessary to satisfy the brief's constraint that `docker compose build && docker compose up` alone must produce a working app. Combined with a MySQL healthcheck in `docker-compose.yml` and `EnableRetryOnFailure` on the EF Core connection, this also handles the container startup race condition where the app can start before MySQL is fully ready to accept connections.

**Angular version.** The provided template shipped Angular 15.2, while the brief specifies Angular 18. I upgraded through each major version (15→16→17→18) using Angular's official `ng update` migration tooling, verifying the build after each step, rather than skipping directly to 18 (unsupported) or leaving it on 15 (mismatched with the brief).

## Testing

Backend unit tests (`InterviewProjectTemplate.Tests`, xUnit + EF Core's in-memory provider) cover:
- Successful mood submission
- Once-per-day rejection, including correctly simulating the cookie round-trip a real browser performs (this isn't automatic between calls in a test, unlike in a real request/response cycle)
- Admin endpoint access control (missing/invalid key)
- Admin endpoint sorting (most recent first)

## Known limitations / what I'd do with more time

- **Frontend tests.** I focused testing effort on the backend given time constraints; I'd add Angular component/service tests (e.g. with Jasmine/Karma or Jest) next, particularly around the once-per-day error state and admin key handling.
- **Shared type definitions.** The `MoodRating` enum and request/response shapes are currently duplicated by hand between C# and TypeScript. I'd look at generating the TypeScript types from the backend's OpenAPI spec to remove that drift risk.
- **Production CORS origin.** Currently hardcoded to `http://localhost:4200`; should be read from configuration per environment.
- **Admin key management.** A single shared secret in configuration is a reasonable fit for this brief's scope, but wouldn't scale to multiple real admins needing individual accountability — a lightweight login system would be the natural next step if requirements grew.
- **Node/Angular support warning.** Locally I'm running Node 24, which is newer than Angular 18's officially tested range (though everything built and ran correctly) — worth pinning to an LTS version Angular explicitly supports for anything beyond local dev.
