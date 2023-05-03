
namespace Core.Models;

public class RecognizedImage{
    public string Image { get; set; } = default!;
    public int Height { get; set; }
    public int Width { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public List<string> PossibleNames { get; set; } = default!;
}