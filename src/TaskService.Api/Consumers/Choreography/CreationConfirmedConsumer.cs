using Logic.Managers.Interfaces;
using MassTransit;
using SagaContracts.Choreography;

namespace Api.Messages.Consumers.Choreography;

public class CreationConfirmedConsumer : IConsumer<CreationConfirmed>
{
    private readonly IProjectsManager _projectsManager;

    public CreationConfirmedConsumer(IProjectsManager projectsManager) => _projectsManager = projectsManager;

    public async Task Consume(ConsumeContext<CreationConfirmed> context)
    {
        await _projectManager.CreateAsync(context.Message.ProjectId);
    }
}