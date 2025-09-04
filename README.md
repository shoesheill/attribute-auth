# Dynamic Hierarchical Authorization (ASP.NET Core + PostgreSQL)

A starter kit demonstrating **dynamic role hierarchy** with **location/region scoping** and **resource-based authorization** (read everyone in scope; upsert/delete only own).

## Features
- PostgreSQL schema for **roles with inheritance** and **region/location scopes**
- RBAC (role → permission) + ABAC (ownership & scope)
- ASP.NET Core **minimal API** with `IAuthorizationRequirement` + `AuthorizationHandler<T, TResource>`
- JWT-based auth (demo-only secret – change it!)
- Postman collection to try endpoints

---

## 1) Prerequisites
- .NET 9 SDK
- PostgreSQL 17.6
- (optional) Docker for running Postgres locally
- Postman

## 2) Setup Database
```bash
psql "postgres://postgres:postgres@localhost:5432/postgres" -f sql/schema.sql
psql "postgres://postgres:postgres@localhost:5432/postgres" -f sql/seed.sql
```
> Adjust the connection string to your environment.

## 3) Configure App
Create `appsettings.json` in `src/` (or set environment variables):

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=postgres;Username=postgres;Password=postgres"
  }
}
```

> JWT key is hardcoded in `Program.cs` for demo. Replace `"super-secret-key-please-change"` with a strong secret and move it to `appsettings.json`.

## 4) Run the API
```bash
dotnet run --project src/Program.cs
```
Open Swagger at `http://localhost:5000/swagger` (or the console output URL).

## 5) Auth Flow
1. **Login** to get a JWT:
   - POST `/auth/login` with one of the seeded users:
     - `nepal_head` / `pass`
     - `regional_head` / `pass`
     - `local_head_ktm` / `pass`
2. Use the Bearer token for the resource endpoints.

## 6) Authorization Rules
- **NepalHead**: full access everywhere.
- **RegionalHead**: can **read** all resources in their region; can **upsert/delete** only resources **they own** in their region.
- **LocalHead**: can **read** all resources in their location; can **upsert/delete** only resources **they own** in their location.
- Multiple users in the same scope **share read** but **write their own only**.

All enforced by `ResourceAccessHandler` using `PermissionService` with both RBAC (permissions) and ABAC (scope + ownership).

## 7) Postman Collection
Import `postman/DynamicAuth.postman_collection.json` and follow the folder order:
- Auth → Login
- Resources → Get / Create / Update / Delete

Set a Postman **Environment** variable `baseUrl` (default `http://localhost:5000`). Then the requests use `{{baseUrl}}`.

## 8) Extend
- Add more actions/resources in the `permission` table.
- Add new roles, chain them via `parent_role_id` to inherit permissions.
- Assign roles to users with scope in `user_role_scope` (global/region/location).
- Tighten JWT, add password hashing (e.g., PBKDF2/BCrypt), and refresh tokens.
- Replace naive permission queries with cached materialized views for performance.

## 9) Notes
- EF Core models are minimal and not 1:1 with SQL constraints in this starter; for production, generate EF migrations from the SQL or model-first approach.
- The view `v_user_effective_permissions` is provided as a convenience if you want to precompute permissions.
- This is a **starter**. Adapt for your domain (VoIP, tickets, etc.).
