namespace CoreLib.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int? AssigneeId { get; set; }
    public int ReporterId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
}