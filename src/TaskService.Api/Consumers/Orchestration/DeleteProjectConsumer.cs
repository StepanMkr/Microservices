using Logic.Managers.Interfaces;
using MassTransit;
using SagaContracts.Orchestration;

namespace Api.Messages.Consumers.Orchestration;

public class DeleteProjectConsumer : IConsumer<DeleteProject>
{
    private readonly IProjectsManager _projectsManager;
    private readonly IPublishEndpoint _publish;

    public DeleteProjectConsumer(IProjectsManager projectsManager, IPublishEndpoint publish)
    {
        _projectsManager = projectsManager;
        _publish = publish;
    }

    public async Task Consume(ConsumeContext<DeleteProject> context)
    {
        await _projectsManager.DeleteAsync(context.Message.ProjectId);
        await _publish.Publish<ProjectDeleted>(new { context.Message.CreationId, context.Message.ProjectId });
    }
}