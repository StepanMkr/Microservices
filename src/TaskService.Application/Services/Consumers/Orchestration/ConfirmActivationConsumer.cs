using MassTransit;
using Microsoft.Extensions.Logging;
using SagaContracts.Orchestration;
using Services.Contracts.Dtos.Creations;
using Services.Interfaces;

namespace Services.Messages.Consumers.Orchestration;

public class ConfirmCreationConsumer(
    ICreationsService creationsService,
    IPublishEndpoint publish,
    ILogger<ConfirmCreationConsumer> logger)
    : IConsumer<ConfirmCreation>
{
    public async Task Consume(ConsumeContext<ConfirmCreation> context)
    {
        logger.LogInformation("Orchestration: подтверждение создания {CreationId}", context.Message.CreationId);
        await creationsService.ConfirmAsync(context.Message.CreationId, new CreationConfirmRequest { UserId = Guid.Empty });
        await publish.Publish<CreationConfirmed>(new { context.Message.CreationId, context.Message.ProjectId });
    }
}