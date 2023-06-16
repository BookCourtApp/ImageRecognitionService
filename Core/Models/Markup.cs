
namespace Core.Models;

public class Markup
{
    public Guid Id { get; set; }
    public int x1 { get; set; }
    public int x2 { get; set; }
    public int x3 { get; set; }
    public int x4 { get; set; }
    public int y1 { get; set; }
    public int y2 { get; set; }
    public int y3 { get; set; }
    public int y4 { get; set; }
    public List<Text> Texts { get; set; }
}