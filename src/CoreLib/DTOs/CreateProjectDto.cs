namespace CoreLib.DTOs;

public class CreateProjectDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
}