
namespace Core.Models;

public class TextMarkup
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Text { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}