using TaskService.Domain.Entities;

namespace TaskService.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project> AddAsync(Project project);
    Task<Project?> GetByIdAsync(int id);
    Task<IEnumerable<Project>> GetAllAsync(int? ownerId, int page, int pageSize);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}