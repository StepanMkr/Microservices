using MassTransit;

namespace ActivationSagaOrchestrator.Saga;

public class ActivationSagaState : SagaStateMachineInstance
{
    public int CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    public int ProjectId { get; set; }
}