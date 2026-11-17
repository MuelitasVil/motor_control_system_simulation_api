namespace MotorControlSystemSimulationApi.Models.Entities;

public class CrewMember
{
    public Guid CrewMemberId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public decimal Weight { get; set; }

    public ICollection<TripCrewMember> Trips { get; set; }
        = new List<TripCrewMember>();
}