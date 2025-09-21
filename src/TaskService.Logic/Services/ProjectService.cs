using CoreLib.DTOs;
using CoreLib.Entities;
using CoreLib.Interfaces;

namespace TaskService.Logic.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto)
    {
        var project = new ProjectEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = dto.OwnerId,
            IsArchived = false
        };

        var created = await _projectRepository.AddAsync(project);
        return MapToDto(created);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        return project is null ? null : MapToDto(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(int? ownerId, int page, int pageSize)
    {
        var projects = await _projectRepository.GetAllAsync(ownerId, page, pageSize);
        return projects.Select(MapToDto);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        if (!string.IsNullOrEmpty(dto.Name)) project.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Description)) project.Description = dto.Description;
        if (dto.IsArchived.HasValue) project.IsArchived = dto.IsArchived.Value;

        await _projectRepository.UpdateAsync(project);
        return MapToDto(project);
    }

    public async Task<bool> DeleteProjectWithTasksAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return false;

        await _projectRepository.DeleteAsync(project);
        return true;
    }

    private static ProjectDto MapToDto(ProjectEntity project) => new ProjectDto
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        OwnerId = project.OwnerId,
        IsArchived = project.IsArchived
    };
}
