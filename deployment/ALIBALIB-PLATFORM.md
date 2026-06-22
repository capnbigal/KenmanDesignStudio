# alibalib.com — Platform Overview

A self-hosted, multi-app platform. Several independent .NET web apps run as Docker containers
behind a single nginx reverse proxy on one DigitalOcean droplet, each on its own subdomain, sharing
one SQL Server and one observability dashboard.

---

## Apps & repositories

| Subdomain | App | Repo | Local folder | Image | Role |
|---|---|---|---|---|---|
| **alibalib.com** | Platform landing | (in the KDS repo) `deployment/landing/index.html` | — | static (nginx) | Hub page listing the apps |
| **awblazor.alibalib.com** | **AWBlazor** | `github.com/capnbigal/AWBlazor` | `AWBlazor` | `ghcr.io/capnbigal/awblazor` | ERP / forecasting demo. **Owns the shared SQL Server.** |
| **kenmandesignstudio.alibalib.com** | **Kenman Design Studio** | `github.com/capnbigal/KenmanDesignStudio` | `KenTheMan` | `ghcr.io/capnbigal/kenmandesignstudio` | Luxury landscape-architecture demo. **Also hosts the platform landing + observability stack.** |
| **stooqexplorer.alibalib.com** | **StooqExplorer** | `github.com/capnbigal/StooqExplorer` | `StooqExplorer` | `ghcr.io/capnbigal/stooqexplorer` | Markets / options research. **Owner-only** (nginx Basic Auth). |
| **nook.alibalib.com** | **Nook** | `github.com/capnbigal/Nook` | `Nook` | `ghcr.io/capnbigal/nook` | Personal productivity / knowledge hub (notes, todos, items, tags). Public. |
| **roofied.alibalib.com** | **Roofied** | `github.com/capnbigal/Roofied` | `Roofied` | `ghcr.io/capnbigal/roofied` | Community safety reporting & awareness platform. Public. |
| **dashboard.alibalib.com** | Aspire Dashboard | (in the KDS repo) `deployment/observability/` | — | `mcr.microsoft.com/dotnet/aspire-dashboard:9.0` | Observability (traces/metrics/logs). |

> The **KenTheMan** local folder is the **KenmanDesignStudio** repo (renamed from an earlier
> "Verdant Atelier"). It doubles as the *platform repo*: the alibalib landing page, the Aspire
> dashboard compose, and the platform ops docs (`deployment/`) live here.

---

## Infrastructure

- **Host:** DigitalOcean droplet, IP **162.243.174.107**, **8 GB RAM / 4 vCPU**, ~154 GB disk, Ubuntu.
- **Reverse proxy:** **nginx** — one server block per subdomain; proxies to a container on `127.0.0.1:<port>`.
  Blazor needs the WebSocket `Upgrade` headers for its SignalR circuit (the `map $http_upgrade …`
  is defined **once** in the awblazor site conf — don't redefine it).
- **Containers:** **Docker Compose**, one stack per app under `/opt/<app>/`. All app + infra containers
  join the external Docker network **`awblazor_default`** (created by the AWBlazor stack) so they can
  reach each other by container name.
- **DNS:** **Porkbun** — one A record per subdomain (Host = the label only, e.g. `awblazor`).
- **TLS:** **certbot --nginx** (Let's Encrypt), one cert per subdomain.
- **Images / CI:** **GitHub Container Registry (GHCR)**. **GitHub Actions** builds & pushes on push to
  `main` (`.github/workflows/build-and-push-image.yml`); the droplet pulls. Packages must be **Public**
  (or `docker login` on the droplet).
- **Access:** SSH/uploads use a **PuTTY** key (`C:\Users\capnb\.ssh\droplet.ppk`); the DO web console
  mangles long pasted lines — use PuTTY. File copy: `pscp -i droplet.ppk` (OpenSSH `scp` fails publickey).

### Container/port map

| Container | Host port | Network | Notes |
|---|---|---|---|
| `awblazor` | 127.0.0.1:8080 | awblazor_default | the app |
| `awblazor-sqlserver` | (internal) | awblazor_default | **shared SQL Server 2022** |
| `kenmandesignstudio` | 127.0.0.1:8081 | awblazor_default | |
| `stooqexplorer` (`stooq-app`) | 127.0.0.1:8082 | awblazor_default | Basic-Auth gated |
| `nook-app` | 127.0.0.1:8083 | awblazor_default | DB `Nook` |
| `roofied-app` | 127.0.0.1:8084 | awblazor_default | DB `Roofied`; needs `IP_HASH_SALT` |
| `aspire-dashboard` | 127.0.0.1:18888 (UI), :18889 (OTLP, internal) | awblazor_default | |
| `thetaterminal` | (internal :25503) | awblazor_default | StooqExplorer live-data feed |

---

## Shared data tier

One **SQL Server 2022** container (`awblazor-sqlserver`, owned by the AWBlazor stack) hosts every app's
database; apps reach it as host `awblazor-sqlserver` on `awblazor_default`:

- **AdventureWorks2022** — AWBlazor
- **KenmanDesignStudio** — Kenman Design Studio
- **StooqExplorer** — StooqExplorer price history (~17.5 GB, restored from a `.bak`)
- **ThetaSpy** — StooqExplorer options data (full set is ~150 GB and does **not** fit; only the **schema**
  + a rolling recent window is kept; see the StooqExplorer hybrid-sourcing work)
- **Nook** — Nook (notes / items / tags); auto-migrated + seeded on first start
- **Roofied** — Roofied (reports / channels / resources / Identity); auto-migrated + seeded on first start

`SA_PASSWORD` is shared across all stacks' `.env` (never committed). SQL memory cap = 4096 MB
(`MSSQL_MEMORY_LIMIT_MB`). Note: SQL **error 18456 state 38** ("Login failed for 'sa'") actually means
the *database in the connection string doesn't exist*.

---

## Observability

A standalone **.NET Aspire Dashboard** at **dashboard.alibalib.com** aggregates OpenTelemetry
(traces / metrics / logs). Apps export **opt-in via `.env`**: `OTLP_ENDPOINT=http://aspire-dashboard:18889`
+ a shared `OTLP_API_KEY` (same value across the dashboard and every app). UI auth = BrowserToken,
OTLP auth = ApiKey. KDS uses the default OTel logging provider; AWBlazor uses the **Serilog OTLP sink**;
StooqExplorer uses default OTel.

---

## Tech stack

**Common to all apps:** **.NET 10**, **Blazor Server** (global **Interactive Server** render mode),
**MudBlazor** component library, **EF Core 10** + **SQL Server**, containerized with a multi-stage
Dockerfile.

| App | Notable additions |
|---|---|
| **AWBlazor** | **Serilog** (+ OTLP sink) for logging; forecasting/ERP domain. `AllowedHosts="*"` (pinned). |
| **Kenman Design Studio** | **ApexCharts** (namespace-aliased); EF Core seeding (`DataSeeder`); antler-themed brand mark + favicon. |
| **StooqExplorer** | **Dapper** (hot-path queries) alongside EF Core; **Plotly.Blazor** charts; **SignalR** (`TickHub`) for the live-tick stream; **Sep** CSV parsing (ETL); a typed **`IThetaTerminalClient`** over the ThetaData Terminal REST API. Two console tools in-repo: `StooqExplorer.Theta` (ETL into ThetaSpy) and `StooqExplorer.Etl`. |

---

## Conventions

**Add-a-new-app (repeatable pattern):**
1. Porkbun A record for the new subdomain.
2. New Compose stack under `/opt/<app>/` on a new `127.0.0.1` port, joining the external `awblazor_default` network.
3. nginx server block (include the Blazor WebSocket `Upgrade` headers).
4. `certbot --nginx -d <sub>.alibalib.com`.
5. Add the OTLP `.env` vars to report into the dashboard.
6. Add a card to the platform landing page (`deployment/landing/`).
7. GitHub Actions builds the image on push to `main`; pull + `docker compose up -d` on the droplet.

**Deploy an update:** merge to `main` → CI builds & pushes the image → on the droplet
`cd /opt/<app> && sudo docker compose pull app && sudo docker compose up -d app`.

See also: `deployment/DEPLOY.md`, `deployment/OBSERVABILITY.md`, and each repo's own deployment docs.
