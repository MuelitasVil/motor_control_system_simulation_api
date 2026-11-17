using MotorControlSystemSimulationApi.Models.Enums;

namespace MotorControlSystemSimulationApi.Models.Entities;

public class Command
{
    public Guid CommandId { get; set; } = Guid.NewGuid();

    public Guid AssociatedMotorId { get; set; }
    public MotorAssociated? AssociatedMotor { get; set; }

    public CommandType Type { get; set; }
    public string Parameters { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}