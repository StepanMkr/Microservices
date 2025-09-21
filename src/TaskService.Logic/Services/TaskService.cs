using CoreLib.DTOs;
using CoreLib.Entities;
using CoreLib.Interfaces;

namespace TaskService.Logic.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        var task = new TaskEntity
        {
            Title = dto.Title,
            Description = dto.Description,
            ProjectId = dto.ProjectId,
            AssigneeId = dto.AssigneeId,
            ReporterId = 1, // TODO: получать из контекста пользователя
            Status = "New",
            Priority = dto.Priority ?? "Medium",
            CreatedAt = DateTime.UtcNow,
            DueDate = dto.DueDate
        };

        var created = await _taskRepository.AddAsync(task);
        return MapToDto(created);
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
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;

        var oldStatus = task.Status;
        task.Status = dto.NewStatus;

        // логируем изменение статуса
        var log = new TaskStatusLog
        {
            TaskId = task.Id,
            OldStatus = oldStatus,
            NewStatus = dto.NewStatus,
            ChangedByUserId = 1, // TODO: брать текущего пользователя
            ChangedAt = DateTime.UtcNow
        };

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.AddStatusLogAsync(log);

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        await _taskRepository.DeleteAsync(task);
        return true;
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
