namespace API.Entities;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<PhotoTag> PhotoTags { get; set; } = [];
}
