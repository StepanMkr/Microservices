using CoreLib.Entities;

namespace CoreLib.Interfaces;

public interface IProjectRepository
{
    Task<ProjectEntity> AddAsync(ProjectEntity project);
    Task<ProjectEntity?> GetByIdAsync(int id);
    Task<IEnumerable<ProjectEntity>> GetAllAsync(int? ownerId, int page, int pageSize);
    Task UpdateAsync(ProjectEntity project);
    Task DeleteAsync(ProjectEntity project);
}