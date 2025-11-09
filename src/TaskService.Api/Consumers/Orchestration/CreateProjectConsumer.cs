using Logic.Managers.Interfaces;
using MassTransit;
using SagaContracts.Orchestration;

namespace Api.Messages.Consumers.Orchestration;

public class CreateProjectConsumer : IConsumer<CreateProject>
{
    private readonly IProjectsManager _projectsManager;
    private readonly IPublishEndpoint _publish;

    public CreateProjectConsumer(IProjectsManager projectsManager, IPublishEndpoint publish)
    {
        _projectsManager = projectsManager;
        _publish = publish;
    }

    public async Task Consume(ConsumeContext<CreateProject> context)
    {
        await _projectsManager.CreateAsync(context.Message.ProjectId);
        await _publish.Publish<ProjectCreated>(new { context.Message.CreationId, context.Message.ProjectId });
    }
}