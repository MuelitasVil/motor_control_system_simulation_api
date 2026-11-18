namespace MotorControlSystemSimulationApi.Models.Entities;

public class MotorStateHistory
{
    public Guid MotorStateId { get; set; }
    public Guid AssociatedMotorId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Speed { get; set; }
    public decimal Temperature { get; set; }
    public int Rpm { get; set; }
    public decimal Power { get; set; }
    public decimal BatteryLevel { get; set; }
}
