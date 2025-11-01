using ActivationSagaOrchestrator.Saga;
using MassTransit;
using SagaContracts.Orchestration;

public class ActivationSagaStateMachine : MassTransitStateMachine<ActivationSagaState>
{
    public State WaitingForProjectDelete { get; private set; } = null!;
    public State WaitingForConfirmation { get; private set; } = null!;
    public State WaitingForActivation { get; private set; } = null!;

    public Event<StartActivation> StartActivationEvent { get; private set; } = null!;
    public Event<ProjectDeleted> ProjectDeletedEvent { get; private set; } = null!;
    public Event<ActivationConfirmed> ActivationConfirmedEvent { get; private set; } = null!;
    public Event<ProjectCreated> ProjectCreatedEvent { get; private set; } = null!;

    private const string DeleteProjectQueue = "queue:delete-project";
    private const string ConfirmActivationQueue = "queue:confirm-activation";
    private const string CreateProjectQueue = "queue:create-project";

    public ActivationSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        ConfigureEvents();
        ConfigureStateMachine();
        
        SetCompletedWhenFinalized();
    }

    private void ConfigureEvents()
    {
        Event(() => StartActivationEvent, cfg =>
        {
            cfg.CorrelateById(x => x.Message.ActivationId);
            cfg.InsertOnInitial = true;

            cfg.SetSagaFactory(ctx => new ActivationSagaState
            {
                CorrelationId = ctx.Message.ActivationId,
                ProjectId = ctx.Message.ProjectId,
                CurrentState = nameof(Initial)
            });
        });

        Event(() => ProjectDeletedEvent, x => x.CorrelateById(m => m.Message.ActivationId));
        Event(() => ActivationConfirmedEvent, x => x.CorrelateById(m => m.Message.ActivationId));
        Event(() => ProjectCreatedEvent, x => x.CorrelateById(m => m.Message.ActivationId));
    }

    private void ConfigureStateMachine()
    {
        Initially(
            When(StartActivationEvent)
                .SendAsync(ctx => new Uri(DeleteProjectQueue),
                    ctx => ctx.Init<DeleteProject>(new { ctx.Message.ActivationId, ctx.Saga.ProjectId }))
                .TransitionTo(WaitingForProjectDelete)
        );

        During(WaitingForProjectDelete,
            When(ProjectDeletedEvent)
                .SendAsync(ctx => new Uri(ConfirmActivationQueue),
                    ctx => ctx.Init<ConfirmActivation>(new { ctx.Message.ActivationId, ctx.Saga.ProjectId }))
                .TransitionTo(WaitingForConfirmation)
        );

        During(WaitingForConfirmation,
            When(ActivationConfirmedEvent)
                .SendAsync(ctx => new Uri(CreateProjectQueue),
                    ctx => ctx.Init<CreateCard>(new { ctx.Message.ActivationId, ctx.Saga.ProjectId }))
                .TransitionTo(WaitingForActivation)
        );

        During(WaitingForActivation,
            When(ProjectCreatedEvent)
                .Finalize()
        );
    }
}
