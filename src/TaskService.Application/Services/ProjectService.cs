using TaskService.Application.Interfaces;
using TaskService.Domain.DTOs;
using TaskService.Domain.Entities;
using TaskService.Domain.Interfaces;
using CoreLib.HttpService.Services.Interfaces;
using CoreLib.HttpService.Services.Models;
using CoreLib.HttpService.Exceptions;

namespace TaskService.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpRequestService _http;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);
    private const string ClientName = "project-catalog";

    public ProjectService(IProjectRepository projectRepository, IHttpRequestService http)
    {
        _projectRepository = projectRepository;
        _http = http;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project is null) return null;

        var meta = await GetExternalMetaAsync(id);

        var dto = MapToDto(project);
        dto.Meta = meta;
        return dto;
    }

    private async Task<ProjectMetaDto?> GetExternalMetaAsync(int projectId)
    {
        var request = new HttpRequestData
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"/api/projects/{projectId}/meta", UriKind.Relative)
        };

        var connection = new HttpConnectionData
        {
            ClientName = ClientName,
            Timeout = DefaultTimeout
        };

        var response = await _http.SendRequestAsync<ProjectMetaDto>(request, connection);

        if (response.Body is null)
            throw new HttpRequestException('Empty body');

        return response.Body;
    }

    private static ProjectDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        OwnerId = project.OwnerId,
        IsArchived = project.IsArchived
    };
}
