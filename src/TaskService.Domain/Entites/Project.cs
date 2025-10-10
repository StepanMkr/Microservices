namespace TaskService.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public bool IsArchived { get; set; } = false;

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}