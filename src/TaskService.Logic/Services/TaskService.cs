using CoreLib.DistributedSync.Abstractions;
using CoreLib.DTOs;
using CoreLib.Entities;
using CoreLib.Interfaces;

namespace TaskService.Logic.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IDistributedSemaphoreFactory _semFactory;

    public TaskService(ITaskRepository taskRepository, IDistributedSemaphoreFactory semFactory)
    {
        _taskRepository = taskRepository;
        _semFactory = semFactory;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        var distributedSemaphore = _semFactory.Create($"project:{dto.ProjectId}:tasks:create:sem", 1, TimeSpan.FromSeconds(30));
        await using (await distributedSemaphore.AcquireAsync(TimeSpan.FromSeconds(2)))
        {
            var task = new TaskEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                AssigneeId = dto.AssigneeId,
                ReporterId = 1, 
                Status = "New",
                Priority = dto.Priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                DueDate = dto.DueDate
            };

            var created = await _taskRepository.AddAsync(task);
            return MapToDto(created);
        }
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task is null ? null : MapToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetTasksAsync(
        int? projectId, int? assigneeId, string? status, string? tag, int page, int pageSize)
    {
        var tasks = await _taskRepository.GetAllAsync(projectId, assigneeId, status, tag, page, pageSize);
        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;

        if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description)) task.Description = dto.Description;
        if (dto.AssigneeId.HasValue) task.AssigneeId = dto.AssigneeId.Value;
        if (dto.DueDate.HasValue) task.DueDate = dto.DueDate.Value;
        if (!string.IsNullOrEmpty(dto.Priority)) task.Priority = dto.Priority;

        await _taskRepository.UpdateAsync(task);
        return MapToDto(task);
    }

    public async Task<TaskDto?> ChangeStatusAsync(int id, ChangeStatusDto dto)
    {
        var distributedSemaphore = _semFactory.Create($"task:{id}:sem", 1, TimeSpan.FromSeconds(30));
        await using (await distributedSemaphore.AcquireAsync(TimeSpan.FromSeconds(2)))
        {
            var task = await _taskRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Task {id} not found.");

            if (task.Status == "Closed" && dto.NewStatus != "Reopened")
            {
                throw new InvalidOperationException($"Task {id} is closed and cannot change status to {dto.NewStatus}.");
            }

            var oldStatus = task.Status;
            task.Status = dto.NewStatus;

            var log = new TaskStatusLog
            {
                TaskId = task.Id,
                OldStatus = oldStatus,
                NewStatus = dto.NewStatus,
                ChangedByUserId = 1, 
                ChangedAt = DateTime.UtcNow
            };

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.AddStatusLogAsync(log);

            return MapToDto(task);
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var distributedSemaphore = _semFactory.Create($"task:{id}:sem", 1, TimeSpan.FromSeconds(30));
        await using (await distributedSemaphore.AcquireAsync(TimeSpan.FromSeconds(2)))
        {
            var task = await _taskRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Task {id} not found.");

            if (task.Status is "InProgress" or "Closed")
            {
                throw new InvalidOperationException($"Task {id} in status '{task.Status}' cannot be deleted.");
            }

            await _taskRepository.DeleteAsync(task);
            return true;
        }
    }

    private static TaskDto MapToDto(TaskEntity task) => new TaskDto
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        ProjectId = task.ProjectId,
        AssigneeId = task.AssigneeId,
        ReporterId = task.ReporterId,
        Status = task.Status,
        Priority = task.Priority,
        CreatedAt = task.CreatedAt,
        DueDate = task.DueDate
    };
}
