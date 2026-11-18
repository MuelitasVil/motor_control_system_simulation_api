using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/crew-members")]
public sealed class CrewMemberController(IEntityRepository<CrewMember> repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CrewMemberResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var crewMembers = await repository.GetAllAsync(cancellationToken);
        return Ok(crewMembers.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CrewMemberResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var crewMember = await repository.GetByIdAsync(id, cancellationToken);
        return crewMember is null ? NotFound() : Ok(ToResponse(crewMember));
    }

    private static CrewMemberResponse ToResponse(CrewMember crewMember) => new(
        crewMember.CrewMemberId,
        crewMember.Name,
        crewMember.Phone,
        crewMember.EmergencyPhone,
        crewMember.Weight);
}

public sealed record CrewMemberResponse(
    Guid CrewMemberId,
    string Name,
    string Phone,
    string EmergencyPhone,
    decimal Weight);
