using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

var baseAddress = args.FirstOrDefault() ?? "http://localhost:5168/";
if (!baseAddress.EndsWith('/'))
    baseAddress += "/";

var baseUri = new Uri(baseAddress);
using var httpClient = new HttpClient { BaseAddress = baseUri };
using var cancellationSource = new CancellationTokenSource();
Guid? activeTripId = null;

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

try
{
    var vehicles = await GetRequiredAsync<List<VehicleResponse>>(
        httpClient,
        "api/vehicles",
        cancellationSource.Token);

    Console.WriteLine("Vehículos disponibles:");
    foreach (var vehicle in vehicles)
        Console.WriteLine($"- {vehicle.Identifier} | {vehicle.VehicleId}");

    var associatedMotors = await GetRequiredAsync<List<AssociatedMotorResponse>>(
        httpClient,
        "api/associated-motors",
        cancellationSource.Token);

    var availableMotor = associatedMotors.FirstOrDefault(
        item => string.Equals(item.OperationState, "Stopped", StringComparison.OrdinalIgnoreCase));

    if (availableMotor is null)
    {
        Console.WriteLine("No hay motores detenidos disponibles para iniciar un viaje.");
        return;
    }

    Console.WriteLine($"\nMotor asociado seleccionado: {availableMotor.AssociatedMotorId}");
    Console.WriteLine("Conectando a SignalR...");

    await using var hubConnection = new HubConnectionBuilder()
        .WithUrl(new Uri(baseUri, "hubs/motor-telemetry"))
        .WithAutomaticReconnect()
        .Build();

    hubConnection.On<MotorStateResponse>("MotorStateUpdated", state =>
    {
        Console.WriteLine(
            $"[{state.UpdatedAt:HH:mm:ss}] " +
            $"Velocidad: {state.AppliedSpeed,6:0.##} km/h | " +
            $"Batería: {state.BatteryLevel,6:0.##}% | " +
            $"Temperatura: {state.Temperature,6:0.##} °C | " +
            $"RPM: {state.Rpm,5} | Potencia: {state.Power,6:0.##} kW");
    });

    await hubConnection.StartAsync(cancellationSource.Token);
    await hubConnection.InvokeAsync(
        "SubscribeToMotor",
        availableMotor.AssociatedMotorId,
        cancellationSource.Token);

    var createTrip = new CreateTripRequest(
        availableMotor.AssociatedMotorId,
        "Bogotá",
        "Tunja",
        TotalDistance: 145m,
        CargoWeight: 100m);

    using var response = await httpClient.PostAsJsonAsync(
        "api/trips",
        createTrip,
        cancellationSource.Token);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync(cancellationSource.Token);
        Console.WriteLine($"No se pudo iniciar el viaje ({(int)response.StatusCode}): {error}");
        return;
    }

    var trip = await response.Content.ReadFromJsonAsync<TripResponse>(
        cancellationToken: cancellationSource.Token);
    activeTripId = trip?.TripId;

    Console.WriteLine($"Viaje {trip?.TripId} iniciado. Escuchando actualizaciones; presiona Ctrl+C para salir.\n");
    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationSource.Token);
}
catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
{
    Console.WriteLine("Cliente de integración detenido.");
}
catch (HttpRequestException exception)
{
    Console.WriteLine($"No se pudo conectar con la API en {baseUri}: {exception.Message}");
    Console.WriteLine("Inicia primero la API y vuelve a ejecutar este cliente.");
}
finally
{
    if (activeTripId is Guid tripId)
    {
        try
        {
            using var finishResponse = await httpClient.PostAsync(
                $"api/trips/{tripId}/finish",
                content: null,
                cancellationToken: CancellationToken.None);
            Console.WriteLine(finishResponse.IsSuccessStatusCode
                ? $"Viaje {tripId} finalizado."
                : $"No se pudo finalizar automáticamente el viaje {tripId}.");
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine($"No se pudo finalizar el viaje: {exception.Message}");
        }
    }

    cancellationSource.Dispose();
}

static async Task<T> GetRequiredAsync<T>(
    HttpClient client,
    string route,
    CancellationToken cancellationToken)
{
    using var response = await client.GetAsync(route, cancellationToken);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadFromJsonAsync<T>(
        cancellationToken: cancellationToken)
        ?? throw new InvalidOperationException($"La ruta {route} devolvió una respuesta vacía.");
}

internal sealed record VehicleResponse(Guid VehicleId, string Identifier);

internal sealed record AssociatedMotorResponse(Guid AssociatedMotorId, string OperationState);

internal sealed record CreateTripRequest(
    Guid AssociatedMotorId,
    string StartPoint,
    string EndPoint,
    decimal TotalDistance,
    decimal CargoWeight);

internal sealed record TripResponse(Guid TripId);

internal sealed record MotorStateResponse(
    Guid AssociatedMotorId,
    decimal RequestedSpeed,
    decimal AppliedSpeed,
    decimal SpeedLimit,
    decimal Temperature,
    int Rpm,
    decimal Power,
    decimal BatteryLevel,
    string DrivingMode,
    DateTimeOffset UpdatedAt);
