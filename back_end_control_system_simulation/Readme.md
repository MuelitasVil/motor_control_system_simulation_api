# Motor Control System Simulation API

An ASP.NET Core REST API that simulates an electric vehicle motor control system. It provides HTTP endpoints for managing vehicles, motors, motor-vehicle associations, trips, and maintenance. SignalR streams motor telemetry to connected clients while a vehicle is travelling.

The application uses in-memory repositories with sample data. Data is reset when the API process restarts. The initial data includes three vehicles and three motors; one motor is not installed, one installed motor is stopped and has no trips, and another installed motor already has an active trip.

## Main features

- Create and list vehicles and motors.
- Associate an existing motor with a vehicle.
- Start trips with or without crew members and finish trips.
- Change speed only while the associated motor is in a trip.
- Recharge a vehicle only when it is not travelling.
- Automatically update travelling motor telemetry every 2.5 seconds and stream it over SignalR.

## Main endpoints

| Method and route | Description |
|---|---|
| `GET /api/vehicles` | List vehicles. |
| `POST /api/vehicles` | Create a vehicle. |
| `GET /api/motors` | List motors. |
| `POST /api/motors` | Create a motor without installing it. |
| `GET /api/associated-motors` | List motor-vehicle associations and their state (`Stopped` or `InTrip`). |
| `POST /api/associated-motors` | Associate an existing motor with an existing vehicle. |
| `GET /api/trips` | List trips. |
| `POST /api/trips` | Start a trip; `crewMemberIds` is optional. |
| `POST /api/trips/{id}/finish` | Finish a trip and set the associated motor state to `Stopped`. |
| `GET /api/crew-members` | List crew members available for trips. |
| `POST /api/vehicles/{id}/maintenance` | Recharge the vehicle when it is not travelling. |
| `POST /api/associated-motors/{id}/speed` | Request a speed change during a trip. |

POST request bodies use DTOs. Services generate identifiers and timestamps.

## Real-time telemetry

The SignalR hub endpoint is `/hubs/motor-telemetry`. A client connects to the hub and invokes `SubscribeToMotor(associatedMotorId)`. The API's `MotorTelemetryWorker` checks travelling motors every 2.5 seconds and asks `MotorSimulationService` to advance each simulation cycle. The service updates the current motor state and stores a history record. The worker then sends the `MotorStateUpdated` event to clients subscribed to that motor.

The automatic simulation increases speed by up to 10 km/h per cycle, subject to the motor's maximum speed and driving mode. If the battery is depleted, speed decreases by 10 km/h per cycle until the motor stops. Automatic telemetry cycles do not create manual command records.

## Sample data for trying trips

The associated motor `99999999-9999-9999-9999-999999999999` is stopped and has no trips; use it to start a test trip. The associated motor `33333333-3333-3333-3333-333333333333` already has a trip in progress.

## Example requests

Create a vehicle (`POST /api/vehicles`):

```json
{
  "identifier": "EV-004",
  "color": "Gray",
  "maximumWeight": 1750,
  "maximumOccupants": 5
}
```

Create an uninstalled motor (`POST /api/motors`):

```json
{
  "model": "Urban-Electric",
  "maximumSpeed": 150,
  "maximumTemperature": 140,
  "batteryCapacityKwh": 60,
  "maximumPower": 180
}
```

Associate an existing vehicle and motor (`POST /api/associated-motors`):

```json
{
  "vehicleId": "EXISTING-VEHICLE-ID",
  "motorId": "EXISTING-MOTOR-ID"
}
```

Start a trip without crew members (`POST /api/trips`):

```json
{
  "associatedMotorId": "99999999-9999-9999-9999-999999999999",
  "startPoint": "Bogota",
  "endPoint": "Tunja",
  "totalDistance": 145,
  "cargoWeight": 200
}
```

To include crew, add IDs returned by `GET /api/crew-members`:

```json
{
  "associatedMotorId": "99999999-9999-9999-9999-999999999999",
  "startPoint": "Bogota",
  "endPoint": "Tunja",
  "totalDistance": 145,
  "cargoWeight": 200,
  "crewMemberIds": ["CREW-MEMBER-ID"]
}
```

Maintenance can specify a target battery level from the current level up to 100. If omitted, the service uses 100:

```json
{
  "targetBatteryLevel": 100
}
```

## Requirements

- .NET 11 SDK
- Git (optional, for cloning the repository)

## Run locally

From the repository root, the directory containing `MotorControlSystemSimulation.sln`, restore and run the API:

```powershell
dotnet restore .\MotorControlSystemSimulation.sln
dotnet run --project .\motor_control_system_simulation_api\motor_control_system_simulation_api.csproj
```

The API uses the local address shown in the terminal. With the current launch settings, the endpoints are:

- Swagger UI: <http://localhost:5168/swagger>
- OpenAPI document: <http://localhost:5168/openapi/v1.json>

Use the port printed in the terminal if it differs from `5168`.

## Run unit tests

From the repository root:

```powershell
dotnet test .\motor_control_system_simulation_api.Tests\motor_control_system_simulation_api.Tests.csproj
```

The unit tests focus on `MotorSimulationService` using mocked repositories.

## Run the SignalR integration client

The integration client is a console application. It queries vehicles and associated motors, connects to SignalR, subscribes to a stopped motor, starts a trip, and prints incoming telemetry. Run the API first in one terminal. In a second terminal, from the repository root, run:

```powershell
dotnet run --project .\motor_control_system_simulation_api.IntegrationTest\motor_control_system_simulation_api.IntegrationTest.csproj -- http://localhost:5168/
```

The URL argument is optional; if omitted, the client uses `http://localhost:5168/`. Press `Ctrl+C` to stop the client. It attempts to finish the trip so that the associated motor returns to the `Stopped` state.

## Build the solution

From the repository root:

```powershell
dotnet build .\MotorControlSystemSimulation.sln
```

## Technologies

- C#
- .NET 11
- ASP.NET Core Web API
- SignalR
- OpenAPI and Swagger UI
- xUnit and Moq
