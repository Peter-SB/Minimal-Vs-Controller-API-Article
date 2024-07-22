namespace Shared.Data;

public class Playlist
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required List<int> Songs { get; set; } = new List<int>();
}