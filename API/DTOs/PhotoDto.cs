namespace API.DTOs;

public class PhotoDto
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public bool IsMain { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; }
}