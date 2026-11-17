FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

COPY ["motor_control_system_simulation_api.csproj", "./"]
RUN dotnet restore "motor_control_system_simulation_api.csproj"

COPY . .
RUN dotnet publish "motor_control_system_simulation_api.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5168
EXPOSE 5168

ENTRYPOINT ["dotnet", "motor_control_system_simulation_api.dll"]