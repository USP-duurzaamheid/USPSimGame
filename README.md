# USPSimGame

## Local development (hot reload)

`docker-compose.dev.yml` layers dev-only behavior on top of the base `docker-compose.yml`: it stops the `web` image build at the SDK stage, mounts your local source into the container, and runs `dotnet watch` instead of the published app. It is never applied automatically — you must pass it explicitly with `-f`.

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

- First run (or after changing the `Dockerfile`/compose files) needs `--build`; after that, plain `up` is enough.
- Edit any `.razor`, `.razor.cs`, or `.razor.css` file and save — `dotnet watch` inside the `web` container picks it up and reloads automatically.
- The app is served at `http://localhost:5261`. Postgres is exposed on `localhost:5432` for tooling (e.g. a DB client) but is not something you open in a browser.
- Logs from both containers are interleaved in the terminal; `EntityFrameworkCore.Database.Command` lines are routine SQL query logging from `GameLoopBackgroundService` polling for active sessions — safe to ignore unless you're debugging DB queries. Run `docker compose logs -f web` to follow just the app container.

## CI/CD

`.github/workflows/ci.yml` runs on every pull request and on every push to `master`:

1. **Build & test** — restore, Release build and all unit tests. Test results (`.trx`) and coverage are uploaded as the `test-results` artifact of the run.
2. **Docker image** — only after tests pass. Builds the multi-stage `Dockerfile` to prove it still builds; pushes nothing.
3. **Publish to GHCR** — only on `master`, and only after a reviewer approves the `production` environment (Actions → the run → *Review deployments* → *Approve and deploy*). Pushes
   `ghcr.io/usp-duurzaamheid/uspsimgame:latest` and `:sha-<commit>`. Approving is effectively deploying, since the server picks up `:latest` by itself.

Nothing in CI needs access to the server: the server pulls new images itself (see below).

## Production / deploy

The server runs `docker-compose.prod.yml`, which uses the prebuilt image from GHCR — no source code, SDK or build on the server. A [watchtower](https://github.com/nicholas-fedor/watchtower) container checks GHCR every 5 minutes and restarts `web` when a new `:latest` appears, so every merge to `master` goes live automatically.

One-time server setup (switching over from the original repo). This repo's migrations are not compatible with the old database, so the old data is dropped and the app starts with a fresh, empty database:

```bash
# in the directory the old app runs from
docker exec uspsimgame-db pg_dump -U postgres uspsimgame_db > backup-before-switch.sql   # optional, just in case
docker compose down -v        # stops the old app and REMOVES its database volume
# place docker-compose.prod.yml and .env.example here, then:
cp .env.example .env          # set a new, strong POSTGRES_PASSWORD
docker compose -f docker-compose.prod.yml up -d
```

- The GHCR package must be **public**, or the server must `docker login ghcr.io` with a token that has `read:packages`.
- Roll back by pinning `web` to an older tag, e.g. `image: ghcr.io/usp-duurzaamheid/uspsimgame:sha-abc1234`, and running `up -d` again.
- Do **not** pass `docker-compose.dev.yml` in production. The base `docker-compose.yml` still works for building locally (`docker compose up -d --build`).
