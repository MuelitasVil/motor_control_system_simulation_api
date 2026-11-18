using Microsoft.AspNetCore.SignalR;

namespace MotorControlSystemSimulationApi.Hubs;

/// <summary>Hub al que los clientes se suscriben para recibir telemetría de un motor asociado.</summary>
public sealed class MotorTelemetryHub : Hub
{
    public Task SubscribeToMotor(Guid associatedMotorId) =>
        Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(associatedMotorId));

    public Task UnsubscribeFromMotor(Guid associatedMotorId) =>
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(associatedMotorId));

    public static string GetGroupName(Guid associatedMotorId) =>
        $"associated-motor:{associatedMotorId}";
}
