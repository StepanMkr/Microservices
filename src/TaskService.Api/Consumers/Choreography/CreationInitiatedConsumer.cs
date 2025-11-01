using Logic.Managers.Interfaces;
using MassTransit;
using SagaContracts.Choreography;

namespace Api.Messages.Consumers.Choreography;

public class CreationInitiatedConsumer : IConsumer<CreationInitiated>
{
    private readonly IProjectsManager _projectsManager;
    private readonly IPublishEndpoint _publish;

    public CreationInitiatedConsumer(IProjectsManager projectsManager, IPublishEndpoint publish)
    {
        _projectsManager = projectsManager;
        _publish = publish;
    }

    public async Task Consume(ConsumeContext<CreationInitiated> context)
    {
        await _projectsManager.DeleteAsync(context.Message.ProjectId);
        await _publish.Publish<ProjectDeleted>(new { context.Message.CreationId, context.Message.ProjectId });
    }
}