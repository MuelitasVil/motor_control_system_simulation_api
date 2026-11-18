namespace MotorControlSystemSimulationApi.Models.Entities;

public class TripHistory
{
    public Guid TripHistoryId { get; set; } = Guid.NewGuid();

    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
}