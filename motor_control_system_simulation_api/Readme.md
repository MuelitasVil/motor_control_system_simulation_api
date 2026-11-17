# Motor Control System Simulation API

API REST construida con ASP.NET Core para simular la comunicación con un motor de vehículo eléctrico. El proyecto incluye controladores HTTP, documentación OpenAPI y Swagger UI para explorar y probar los endpoints durante el desarrollo.

La solución se ampliará con modelos y repositorios simulados en memoria para representar vehículos, motores asociados, viajes, telemetría y comandos.

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

Ejecutar pruebas
Cuando se agregue el proyecto de pruebas a la solución:
```bash
dotnet test
```

## Tecnologías
- C#
- .NET 11
- ASP.NET Core Web API
- OpenAPI
- Swagger UI