namespace MotorControlSystemSimulationApi.Models.Entities;

public class TripCrewMember
{
    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public Guid CrewMemberId { get; set; }
    public CrewMember? CrewMember { get; set; }
}