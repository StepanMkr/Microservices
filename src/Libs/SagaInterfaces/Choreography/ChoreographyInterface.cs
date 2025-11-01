using System;

namespace SagaContracts.Choreography;

public interface CreationInitiated
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface ProjectDeleted
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface CreationConfirmed
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface ProjectCreated
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}