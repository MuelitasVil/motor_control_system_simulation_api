using Microsoft.AspNetCore.SignalR;
using MotorControlSystemSimulationApi.Hubs;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

/// <summary>Avanza la simulación y publica telemetría cada 2.5 segundos.</summary>
public sealed class MotorTelemetryWorker(
    IEntityRepository<MotorAssociated> associatedMotorRepository,
    MotorSimulationService simulationService,
    IHubContext<MotorTelemetryHub> hubContext,
    ILogger<MotorTelemetryWorker> logger) : BackgroundService
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(
        (double)(MotorSimulationService.TelemetryIntervalSeconds * 1000m));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(UpdateInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateTravellingMotorsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // El host está apagándose; no es un error de la simulación.
        }
    }

    private async Task UpdateTravellingMotorsAsync(CancellationToken cancellationToken)
    {
        var associatedMotors = await associatedMotorRepository.GetAllAsync(cancellationToken);
        var travellingMotors = associatedMotors
            .Where(item => item.OperationState == MotorOperationState.InTrip)
            .ToList();

        foreach (var associatedMotor in travellingMotors)
        {
            try
            {
                var state = await simulationService.AdvanceSpeedAsync(
                    associatedMotor.AssociatedMotorId,
                    cancellationToken);

                if (state is null)
                    continue;

                await hubContext.Clients
                    .Group(MotorTelemetryHub.GetGroupName(associatedMotor.AssociatedMotorId))
                    .SendAsync("MotorStateUpdated", state, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "No se pudo actualizar la telemetría del motor asociado {AssociatedMotorId}.",
                    associatedMotor.AssociatedMotorId);
            }
        }
    }
}
