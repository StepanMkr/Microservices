using System;

namespace SagaContracts.Orchestration;

public interface StartCreation
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface DeleteProject
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface ConfirmCreation
{
    Guid CreationId { get; }
    Guid ProjectId { get; }
}

public interface CreateProject
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