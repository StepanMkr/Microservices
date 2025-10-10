namespace TaskService.Domain.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public bool IsArchived { get; set; }
}