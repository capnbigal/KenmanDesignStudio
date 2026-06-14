# Deploying Kenman Design Studio to kenmandesignstudio.alibalib.com

Goal end-state on the droplet (162.243.174.107):

| URL | Serves |
|---|---|
| `alibalib.com` / `www` | static **platform landing page** (lists the apps) |
| `awblazor.alibalib.com` | AWBlazor (existing app, moved off the root) |
| `kenmandesignstudio.alibalib.com` | **Kenman Design Studio** (new) |

KDS shares the existing **`awblazor-sqlserver`** container (new database `KenmanDesignStudio`)
and runs as its own app container on `127.0.0.1:8081`. Nginx reverse-proxies each subdomain.

Run the steps in order. `$` = on the droplet over SSH.

---

## 0. (Optional) Add swap — safety net

On the upgraded 8 GB / 4 vCPU droplet there's ample headroom (SQL ~2–4 GB + two app
containers ~0.5 GB each + OS leaves plenty free), so swap is no longer required. A small
swapfile is still cheap insurance against deploy-time spikes:

```bash
$ sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile
$ sudo mkswap /swapfile && sudo swapon /swapfile
$ echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
$ free -h
```

Optional, while you're at it — give SQL Server more buffer now that RAM is plentiful. In
`/opt/awblazor/docker-compose.yml` raise `MSSQL_MEMORY_LIMIT_MB` from `2048` to e.g. `4096`,
then `cd /opt/awblazor && sudo docker compose up -d sqlserver`.

## 1. DNS at Porkbun

Porkbun → **Domain Management → alibalib.com → DNS → Edit**. Add two **A** records
(TTL 600). `alibalib.com` and `www` already resolve — leave them.

| Type | Host | Answer |
|---|---|---|
| A | `awblazor` | `162.243.174.107` |
| A | `kenmandesignstudio` | `162.243.174.107` |

Verify before continuing (must return the IP):
```bash
$ dig +short awblazor.alibalib.com kenmandesignstudio.alibalib.com
```

## 2. Push the KDS image to GHCR (from your machine, not the droplet)

Commit the repo (now contains `Dockerfile`, `.dockerignore`, `docker-compose.yml`,
`.github/workflows/build-and-push-image.yml`) and push to `main`. GitHub Actions builds
and publishes `ghcr.io/capnbigal/kenmandesignstudio:latest`.

Then make the package pullable by the droplet — either:
- GitHub → your profile → **Packages → kenmandesignstudio → Package settings → Change visibility → Public**, **or**
- keep it private and `docker login ghcr.io` on the droplet with a PAT (same as AWBlazor).

## 3. Put the deploy files on the droplet

```bash
$ sudo mkdir -p /opt/kenmandesignstudio
$ sudo git clone https://github.com/capnbigal/KenmanDesignStudio.git /opt/kenmandesignstudio
# (or scp just docker-compose.yml + deployment/ if you prefer)
$ cd /opt/kenmandesignstudio
$ cp deployment/.env.template .env && sudo chmod 600 .env
$ sudo nano .env     # set SA_PASSWORD to the SAME value as /opt/awblazor/.env
```

Confirm the shared SQL password matches:
```bash
$ grep SA_PASSWORD /opt/awblazor/.env /opt/kenmandesignstudio/.env
```

## 4. Start the KDS container

```bash
$ cd /opt/kenmandesignstudio
$ sudo docker compose pull app
$ sudo docker compose up -d app
$ sudo docker compose logs -f app   # watch: "Applying database migrations" -> "Now listening on: http://[::]:8080"
```

On first start it creates the `KenmanDesignStudio` database on the shared SQL Server and seeds it.
Smoke test locally on the box:
```bash
$ curl -I http://127.0.0.1:8081      # expect HTTP/1.1 200
```

## 5. Nginx — move AWBlazor to its subdomain, add landing + KDS

**a) Move AWBlazor off the root.** Edit its existing config and change BOTH `server_name`
lines from `alibalib.com www.alibalib.com` to `awblazor.alibalib.com`:
```bash
$ sudo sed -i 's/server_name alibalib.com www.alibalib.com;/server_name awblazor.alibalib.com;/g' \
      /etc/nginx/sites-available/awblazor.conf
```
(Leave the `map $http_upgrade $connection_upgrade {…}` block in this file — it is shared by all sites.)

**b) Add the landing page + its site:**
```bash
$ sudo mkdir -p /var/www/alibalib
$ sudo cp /opt/kenmandesignstudio/deployment/landing/index.html /var/www/alibalib/
$ sudo cp /opt/kenmandesignstudio/deployment/nginx-alibalib-landing.conf /etc/nginx/sites-available/alibalib-landing.conf
$ sudo ln -s /etc/nginx/sites-available/alibalib-landing.conf /etc/nginx/sites-enabled/
```

**c) Add the KDS site:**
```bash
$ sudo cp /opt/kenmandesignstudio/deployment/nginx-kenmandesignstudio.conf /etc/nginx/sites-available/kenmandesignstudio.conf
$ sudo ln -s /etc/nginx/sites-available/kenmandesignstudio.conf /etc/nginx/sites-enabled/
$ sudo nginx -t && sudo systemctl reload nginx
```

## 6. TLS for the two new subdomains

The landing reuses the existing `alibalib.com` cert. Issue certs for the subdomains
(certbot edits the configs to add `listen 443 ssl` + the redirect automatically):
```bash
$ sudo certbot --nginx -d awblazor.alibalib.com
$ sudo certbot --nginx -d kenmandesignstudio.alibalib.com
$ sudo nginx -t && sudo systemctl reload nginx
$ sudo certbot certificates   # confirm 3 certs now
```

## 7. Verify

```bash
$ curl -sI https://alibalib.com | head -1                       # 200 (landing)
$ curl -sI https://awblazor.alibalib.com | head -1              # 200
$ curl -sI https://kenmandesignstudio.alibalib.com | head -1    # 200
$ free -h                                                       # check headroom + swap in use
```
Open each in a browser; on KDS confirm the antler logo, real project photos, and that the
interactive circuit connects (no "reconnecting" overlay — that means the WebSocket proxy works).

---

## Routine updates later

Push to `main` → Actions rebuilds the image → on the droplet:
```bash
$ cd /opt/kenmandesignstudio && sudo docker compose pull app && sudo docker compose up -d app
```
Rollback: `APP_TAG=<short-sha> sudo docker compose up -d app`.

## Notes / gotchas
- **Memory:** on the 8 GB / 4 vCPU droplet there's comfortable headroom for SQL + both app
  containers. Sanity-check with `docker stats --no-stream` and `free -h` after KDS is up.
- **Backups:** AWBlazor's `deployment/backup-sqlserver.sh` backs up the whole instance, so the
  new `KenmanDesignStudio` DB is included automatically once it exists.
- **Connection string** is injected by compose (env), overriding `appsettings.json`. The app's
  baked-in `Data Source=ELITE` is irrelevant in the container.
