# Observability — Aspire Dashboard for the alibalib platform

A single standalone **.NET Aspire Dashboard** collects logs, traces, and metrics from
every app. Apps export OpenTelemetry (OTLP) to it privately over the shared docker
network; the web UI is served at **dashboard.alibalib.com**, protected by a login token.

```
 each app ──(OTLP/gRPC :18889, x-otlp-api-key)──► aspire-dashboard ──► dashboard.alibalib.com (nginx + TLS)
            over the awblazor_default network                          (login token)
```

Run in order. `$` = on the droplet.

## 1. DNS (Porkbun)
Add an **A** record — Host `dashboard`, Answer `162.243.174.107`. Verify:
```bash
$ dig +short dashboard.alibalib.com    # → 162.243.174.107
```

## 2. Generate two secrets
```bash
$ openssl rand -base64 32   # use for DASHBOARD_TOKEN
$ openssl rand -base64 32   # use for OTLP_API_KEY
```

## 3. Start the dashboard
```bash
$ cd /opt/kenmandesignstudio && sudo git pull
$ cd deployment/observability
$ cp .env.template .env && sudo chmod 600 .env
$ sudo nano .env          # set DASHBOARD_TOKEN and OTLP_API_KEY from step 2
$ sudo docker compose up -d
$ curl -s -o /dev/null -w "%{http_code}\n" http://127.0.0.1:18888   # 200 (or 302 to login) = up
```

## 4. nginx + TLS for the dashboard
```bash
$ sudo cp /opt/kenmandesignstudio/deployment/nginx-dashboard.conf /etc/nginx/sites-available/dashboard.conf
$ sudo ln -s /etc/nginx/sites-available/dashboard.conf /etc/nginx/sites-enabled/
$ sudo nginx -t && sudo systemctl reload nginx
$ sudo certbot --nginx -d dashboard.alibalib.com
```
Open **https://dashboard.alibalib.com** and paste your `DASHBOARD_TOKEN` to log in.

## 5. Point Kenman Design Studio at the dashboard
```bash
$ sudo nano /opt/kenmandesignstudio/.env
#   OTLP_ENDPOINT=http://aspire-dashboard:18889
#   OTLP_API_KEY=<the same OTLP_API_KEY from step 2>
$ cd /opt/kenmandesignstudio && sudo docker compose up -d app
```
Browse KDS, then refresh the dashboard — you'll see **kenmandesignstudio** under Resources/Structured Logs/Traces/Metrics.

## 6. (Recommended) Do the same for AWBlazor and future apps
Each app needs two things to show up:
1. **Instrumentation** — the OpenTelemetry packages + `AddOpenTelemetry()...UseOtlpExporter()`
   wiring (see KDS `src/KenmanDesignStudio.Web/Program.cs` for the exact pattern).
2. **Config** — on the droplet, set in that app's container env:
   ```
   OTEL_SERVICE_NAME=<app-name>
   OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889
   OTEL_EXPORTER_OTLP_HEADERS=x-otlp-api-key=<OTLP_API_KEY>
   ```
   and make sure the app's stack joins the `awblazor_default` network.

## Notes
- **What it shows:** per-app structured logs, distributed traces, and metrics (incl. live
  request/runtime metrics). An app that stops emitting is a strong "something's wrong" signal.
- **Not a hard uptime monitor.** For true up/down alerting later, add a checker (e.g. Uptime
  Kuma) hitting each app's `/health` endpoint — KDS already exposes `https://kenmandesignstudio.alibalib.com/health`.
- **Retention is in-memory** — the dashboard keeps recent telemetry only; it resets on restart.
  That's expected for this use; add a persistent backend (e.g. an OTLP collector → Loki/Tempo/
  Prometheus) only if you need history.
- **Memory:** the dashboard uses ~150–250 MB — comfortable on the 8 GB droplet.
