# Motor Control System Simulation API

API REST construida con ASP.NET Core para simular la comunicación con un motor de vehículo eléctrico. El proyecto incluye controladores HTTP, documentación OpenAPI y Swagger UI para explorar y probar los endpoints durante el desarrollo.

La API utiliza repositorios simulados en memoria. Incluye datos iniciales con tres vehículos y tres motores: uno de los motores todavía no está instalado, y un motor instalado está detenido y sin viajes.

## Endpoints principales

| Método y ruta | Función |
|---|---|
| `GET /api/vehicles` | Lista vehículos. `POST /api/vehicles` crea uno. |
| `GET /api/motors` | Lista motores. `POST /api/motors` crea uno sin instalarlo. |
| `GET /api/associated-motors` | Lista instalaciones y su estado (`Stopped` o `InTrip`). `POST /api/associated-motors` relaciona un motor y un vehículo. |
| `GET /api/trips` | Lista viajes. `POST /api/trips` inicia uno, con `crewMemberIds` opcional o vacío. |
| `POST /api/trips/{id}/finish` | Finaliza un viaje y deja detenido el motor asociado. |
| `POST /api/vehicles/{id}/maintenance` | Recarga las baterías del vehículo, solo cuando no está en viaje. |
| `POST /api/associated-motors/{id}/speed` | Cambia velocidad solo durante un viaje. |

Los cuerpos de los POST son DTOs. Los servicios generan los identificadores y las fechas. Para usar tripulantes existentes, consulta `GET /api/crew-members`.

El motor asociado `99999999-9999-9999-9999-999999999999` está detenido y no tiene viajes, por lo que sirve para probar la creación de viajes. El motor asociado inicial `33333333-3333-3333-3333-333333333333` ya tiene un viaje en curso.

Ejemplo para crear un vehículo (`POST /api/vehicles`):

```json
{
  "identifier": "EV-004",
  "color": "Gris",
  "maximumWeight": 1750,
  "maximumOccupants": 5
}
```

Ejemplo para crear un motor sin asociarlo (`POST /api/motors`):

```json
{
  "model": "Urban-Electric",
  "maximumSpeed": 150,
  "maximumTemperature": 140,
  "batteryCapacityKwh": 60,
  "maximumPower": 180
}
```

Para instalarlo, envía los identificadores existentes a `POST /api/associated-motors`:

```json
{
  "vehicleId": "ID-DEL-VEHICULO",
  "motorId": "ID-DEL-MOTOR"
}
```

Ejemplo para crear un viaje sin tripulantes:

```json
{
  "associatedMotorId": "99999999-9999-9999-9999-999999999999",
  "startPoint": "Bogotá",
  "endPoint": "Tunja",
  "totalDistance": 145,
  "cargoWeight": 200
}
```

Este POST cambia el estado de la instalación a `InTrip`. Al finalizar el viaje con `POST /api/trips/{id}/finish`, el estado pasa a `Stopped`.

Para crear un viaje con tripulantes, incluye sus identificadores en el mismo DTO:

```json
{
  "associatedMotorId": "99999999-9999-9999-9999-999999999999",
  "startPoint": "Bogotá",
  "endPoint": "Tunja",
  "totalDistance": 145,
  "cargoWeight": 200,
  "crewMemberIds": ["ID-OBTENIDO-DE-GET-API-CREW-MEMBERS"]
}
```

Una solicitud de mantenimiento puede indicar el nivel final deseado, entre el nivel actual y 100. Si se omite, se usa 100:

```json
{ "targetBatteryLevel": 100 }
```

## Requisitos

- .NET 11 SDK
- Git (opcional)

## Ejecutar localmente

Clona el repositorio y entra a la carpeta del proyecto:

```bash
git clone <URL_DEL_REPOSITORIO>
cd motor_control_system_simulation_api
```

## Restaura dependencias y ejecuta la API:

```bash
dotnet restore
dotnet run
```

La terminal indicará las direcciones locales donde se inició la aplicación. Con la configuración actual, puedes probar:
- Swagger UI: http://localhost:5168/swagger
- Documento OpenAPI: http://localhost:5168/openapi/v1.json

Usa el puerto que aparezca en la terminal si es diferente de 5168.

Compilar
```bash
dotnet build
```

Desde la carpeta raíz que contiene la solución, ejecutar las pruebas unitarias de `MotorSimulationService`:

```bash
dotnet test MotorControlSystemSimulation.sln
```

## Tecnologías
- C#
- .NET 11
- ASP.NET Core Web API
- OpenAPI
- Swagger UI
