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

## Production / deploy

Deploys use the base `docker-compose.yml` only — **never** pass `docker-compose.dev.yml` here. It builds the full multi-stage `Dockerfile` (publish + slim runtime image, no SDK, no source mount, no watcher) and runs the compiled app directly.

```bash
docker compose up -d --build
```

CI (`.github/workflows/ci.yml`) validates this build path via `docker compose build`, which also only reads the base file since the dev file is never auto-merged.
