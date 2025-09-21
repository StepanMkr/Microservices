using CoreLib.Entities;

namespace CoreLib.Interfaces;

public interface ITaskRepository
{
    Task<Task> CreateTaskAsync(Task task);
    Task<Task?> GetTaskByIdAsync(int id);
    Task<IEnumerable<Task>> GetTasksAsync(int projectId);
    Task<Task> UpdateTaskAsync(Task task);
    Task DeleteTaskAsync(int id);
}