FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY ["MotorControlSystemSimulation.sln", "./"]
COPY ["motor_control_system_simulation_api/motor_control_system_simulation_api.csproj", "motor_control_system_simulation_api/"]
COPY ["motor_control_system_simulation_api.Tests/motor_control_system_simulation_api.Tests.csproj", "motor_control_system_simulation_api.Tests/"]

RUN dotnet restore "MotorControlSystemSimulation.sln"

COPY . .

RUN dotnet test "MotorControlSystemSimulation.sln" \
    --configuration Release

RUN dotnet publish "motor_control_system_simulation_api/motor_control_system_simulation_api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5168
EXPOSE 5168

ENTRYPOINT ["dotnet", "motor_control_system_simulation_api.dll"]