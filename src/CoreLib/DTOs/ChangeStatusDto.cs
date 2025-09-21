namespace CoreLib.DTOs;

public class ChangeStatusDto
{
    public string NewStatus { get; set; } = null!; // можно использовать Enum
    public string? Comment { get; set; }
}