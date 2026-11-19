# Motor Control Dashboard

Single-page React dashboard for the Motor Control System Simulation API. Lists vehicles and motors, creates and finishes trips, sends speed and driving mode commands, recharges parked vehicles and displays live telemetry.

## Run locally

1. Start the ASP.NET Core API on HTTP port 5168.
2. Extract this archive into `front_end/motor_control_system` and run from that folder:

   ```sh
   npm ci
   npm run dev
   ```

3. Open http://localhost:5173. Vite proxies `/api` and `/hubs` to the API configured in `.env.local`.

## Run with Docker Compose

Use this folder as the frontend service build context and publish container port 5173. The frontend Dockerfile starts Vite in `docker` mode. The included `.env.docker` proxies to `http://motor-control-api:5168`, so the backend service must have that name or alias and share the Compose network. From the repository root, use `docker compose up --build`.

If your Compose file already mounts a persistent volume at `/app/node_modules`, update its dependencies after replacing the frontend:

```sh
docker compose run --rm --no-deps motor-control-frontend npm ci
docker compose up --build
```

## Simulation behavior

Both driving controls use `POST /api/associated-motors/{id}/speed`. Changing the mode sends the last displayed speed with the selected mode. The backend can cap that speed and continues to advance the simulation on its own every 2.5 seconds. The current backend does not store a persistent target speed for these commands.

The dashboard subscribes to `MotorStateUpdated` for associated motors on active trips. The fleet list refreshes after successful actions and periodically to pick up trips changed elsewhere.
