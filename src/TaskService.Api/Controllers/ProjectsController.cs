using Microsoft.AspNetCore.Mvc;
using TaskService.Application.Interfaces;
using TaskService.Domain.DTOs;

namespace TaskService.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        var project = await _projectService.CreateProjectAsync(dto);
        return CreatedAtAction(nameof(GetProject), new { projectId = project.Id }, project);
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProject(int projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] int? ownerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var projects = await _projectService.GetProjectsAsync(ownerId, page, pageSize);
        return Ok(projects);
    }

    [HttpPatch("{projectId}")]
    public async Task<IActionResult> UpdateProject(int projectId, [FromBody] UpdateProjectDto dto)
    {
        var project = await _projectService.UpdateProjectAsync(projectId, dto);
        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(int projectId)
    {
        var success = await _projectService.DeleteProjectWithTasksAsync(projectId);
        if (!success) return NotFound();
        return NoContent();
    }
}