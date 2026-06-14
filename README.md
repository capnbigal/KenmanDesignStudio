# Kenman Design Studio — Landscape Architecture Platform (Demo)

A polished, data-rich demonstration platform for an elite landscape architecture, design &
development firm. It pairs a **cinematic public showcase** with a **premium internal operator
platform** (command-center dashboard, Client 360, project pipeline, leads, campaigns, requests,
notifications, analytics).

Built with **.NET 10 · Blazor (Interactive Server) · MudBlazor · EF Core / SQL Server · ApexCharts**.

---

## What's inside

### Public showcase (anonymous, marketing-facing)
| Route | Page |
|-------|------|
| `/` | Cinematic hero landing with brand, stats, disciplines, featured work, testimonial |
| `/portfolio` | Filterable **project gallery** — the visual centerpiece (filter by discipline, `?category=#`) |
| `/portfolio/{id}` | Project detail with multi-image gallery + full project facts |
| `/studio` | The studio / philosophy / disciplines |
| `/testimonials` | Prestige-client testimonials |
| `/consultation` | **Request a Consultation** form → creates a `Request` **and** an attributed `Lead` |

### Internal platform (the operator's app)
| Route | Page |
|-------|------|
| `/admin` | Command-center dashboard — KPI tiles + revenue trend, pipeline, leads-by-source, revenue-by-category charts |
| `/admin/clients` | Clients `MudDataGrid` with tier summary + search + CRUD |
| `/admin/clients/{id}` | **Client 360** — lifetime value, tier, insights, full project history, contacts, value-by-discipline |
| `/admin/projects` | Project **pipeline board** (Lead → Proposed → Won → In Design → In Construction → Complete) + list view, category filter, CRUD |
| `/admin/leads` | Leads with source attribution, inline status changes, CRUD |
| `/admin/campaigns` | Campaign ROI tracker (spend, leads, conversions, ROI) with charts |
| `/admin/requests` | Inbound consultation-request inbox with status workflow |
| `/admin/notifications` | Activity feed |
| `/admin/analytics` | Revenue by category, value by region, win rate by source, lead volume, open pipeline |

Real CRUD is wired on **Clients, Projects and Leads**; the rest read/display over the seeded data.

---

## Prerequisites

- **.NET 10 SDK** (`dotnet --version` ≥ 10.0)
- **SQL Server** (Express/Developer/LocalDB) reachable via a trusted connection
- *(optional)* EF Core tools for manual migrations: `dotnet tool install --global dotnet-ef`

---

## Database connection

The connection string lives in [`src/KenmanDesignStudio.Web/appsettings.json`](src/KenmanDesignStudio.Web/appsettings.json):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=KenmanDesignStudio;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

> **Assumption / note:** the brief specified a SQL Server instance named **`.\ELITE`**. The build
> machine had no `ELITE` instance, so the default instance **`Server=.`** is used so the demo runs
> out of the box. To target the named instance, change `Server=.` to `Server=.\ELITE`
> (or any instance/LocalDB, e.g. `Server=(localdb)\MSSQLLocalDB`).

On **first startup the app automatically**:
1. Applies EF Core migrations (creates the `KenmanDesignStudio` database), then
2. Seeds a large, coherent demo dataset **if the database is empty**, and
3. Generates on-brand SVG project plates into `wwwroot/images/projects/{category}/`.

Both steps are toggleable in `appsettings.json`:

```json
"Seed": { "AutoMigrate": true, "AutoSeed": true }
```

### Manual migration (optional)
```bash
dotnet ef database update \
  -p src/KenmanDesignStudio.Infrastructure/KenmanDesignStudio.Infrastructure.csproj \
  -s src/KenmanDesignStudio.Infrastructure/KenmanDesignStudio.Infrastructure.csproj
```
The EF tooling reads the connection from the `VERDANT_CONNECTION` environment variable if set,
otherwise the same `Server=.` default.

---

## Run it

```bash
dotnet run --project src/KenmanDesignStudio.Web
```

- HTTP profile: **http://localhost:5093**
- HTTPS profile: `dotnet run --project src/KenmanDesignStudio.Web --launch-profile https` → **https://localhost:7074**

The browser opens automatically. First run takes a few extra seconds while the DB is created,
seeded, and ~700 image plates are written.

---

## Re-seeding

The seeder only runs when the database is **empty**, and it is deterministic (fixed RNG seed), so
re-seeding reproduces the same world. To regenerate everything from scratch:

```sql
-- in SSMS / sqlcmd
DROP DATABASE KenmanDesignStudio;
```
…then run the app again. (Or set `"AutoSeed": false` to start empty.)

---

## Dropping in real project photos

The gallery ships with elegant **generated SVG plates** so it looks great offline. To use real
photography, drop image files into the matching category folder:

```
src/KenmanDesignStudio.Web/wwwroot/images/projects/
  ├── rooftop-gardens/
  ├── airports/
  ├── resorts/
  ├── estates/
  ├── golf-courses/
  └── corporate/
```

Each folder has a `README.txt` with guidance. **The app automatically prefers a real raster image
over the generated `.svg` of the same base name.** For example, to replace the plate
`airports/va-25062-1.svg`, drop in `airports/va-25062-1.jpg` (`.jpg/.jpeg/.webp/.png/.avif`
supported) — it appears instantly, no code changes. Recommended: landscape orientation, ≥1600×1200.

---

## Architecture

```
KenmanDesignStudio.sln
├── src/KenmanDesignStudio.Core            # Domain: entities, enums, value helpers
│   ├── Entities/                      # Client, Contact, Project, ProjectMedia, Lead,
│   │                                  #   Campaign, Request, Notification, Testimonial
│   ├── Enums/                         # ProjectCategory, ProjectStatus, ClientTier, LeadSource, …
│   └── Common/                        # CategoryCatalog, tier calculator, display helpers
├── src/KenmanDesignStudio.Infrastructure  # EF Core
│   ├── Data/AppDbContext.cs           # IDbContextFactory, soft-delete filters, audit timestamps
│   ├── Configurations/                # IEntityTypeConfiguration per entity
│   ├── Data/Migrations/               # InitialCreate
│   └── Seeding/                       # DataSeeder, SeedPools, PlaceholderArt (SVG generator)
└── src/KenmanDesignStudio.Web             # Blazor Interactive Server + MudBlazor
    ├── Components/Layout/             # PublicLayout, AdminLayout shells
    ├── Components/Pages/Public|Admin/ # the pages above
    ├── Components/Shared/             # KpiTile, ProjectCard, ChartCard, Pill, dialogs, …
    └── Services/                      # BrandTheme, data services, ImageResolver, ChartTheme
```

**Conventions:** `IDbContextFactory<AppDbContext>` for Blazor Server safety · soft deletes +
`CreatedAt/UpdatedAt` audit fields on every core entity · global query filter hides soft-deleted
rows · a re-themable `MudTheme` (deep botanical green / charcoal / brass) with light + dark toggle.

---

## Demo data at a glance

~53 prestige clients · ~165 projects across all six disciplines and every pipeline stage
(dated over the last ~3 years) · ~730 project images · ~52 attributed leads · 6 campaigns ·
consultation requests · testimonials · a seeded notification feed. KPI tiles and charts reconcile
with the underlying rows.

> This is a **demonstration** build — no authentication, multi-tenancy, or external integrations.
