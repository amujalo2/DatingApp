namespace API.DTOs;

public class AddPhotoDto
{
    public IFormFile File { get; set; } = null!;
    public List<int> TagIds { get; set; } = [];
}