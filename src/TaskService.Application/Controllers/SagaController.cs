using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SagaContracts.Choreography;
using SagaContracts.Orchestration;
using Services.Contracts.Dtos.Creations;
using Services.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/saga")]
public class SagaController(ICreationService creationsService, IPublishEndpoint publish, ISendEndpointProvider sender)
    : ControllerBase
{
    [HttpPost("choreography")]
    public async Task<IActionResult> StartChoreography([FromQuery] Guid projectId, [FromQuery] Guid userId)
    {
        var creationId = await creationsService.CreateAsync(new CreationCreateRequest
        {
            ProjectId = projectId,
            ProjectCodeHash = string.Empty,
            IdempotencyKey = Guid.NewGuid().ToString(),
            UserId = userId
        });

        await publish.Publish<CreationInitiated>(new { CreationId = creationId, ProjectId = projectId });
        return Ok(new { CreationId = creationId, Mode = "choreography" });
    }

    [HttpPost("orchestration")]
    public async Task<IActionResult> StartOrchestration([FromQuery] Guid projectId, [FromQuery] Guid userId)
    {
        var creationId = await acreationService.CreateAsync(new CreationCreateRequest
        {
            ProjectId = projectId,
            ProjectCodeHash = string.Empty,
            IdempotencyKey = Guid.NewGuid().ToString(),
            UserId = userId
        });

        var endpoint = await sender.GetSendEndpoint(new Uri("queue:creation-saga"));
        await endpoint.Send<StartCreation>(new { CreationId = creationId, ProjectId = projectId });

        return Ok(new { CreationId = creationId, Mode = "orchestration" });
    }
}