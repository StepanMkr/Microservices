using CoreLib.DTOs;

namespace CoreLib.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<IEnumerable<TaskDto>> GetTasksAsync(int projectId);
    Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto dto);
    Task DeleteTaskAsync(int id);
    Task<TaskDto> ChangeStatusAsync(int taskId, ChangeStatusDto dto);
}