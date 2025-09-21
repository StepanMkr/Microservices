using CoreLib.DTOs;

namespace CoreLib.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<IEnumerable<ProjectDto>> GetProjectsAsync(int? ownerId, int page, int pageSize);
    Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto);
    Task<bool> DeleteProjectWithTasksAsync(int id);
}