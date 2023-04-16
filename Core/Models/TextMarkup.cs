
namespace Core.Models;

public class TextMarkup
{
    public Guid Id { get; set; }
    public BookMarkup BookMarkup { get; set; }
    public string Type { get; set; }
    public string Text { get; set; }
    public int x0 { get; set; }
    public int x1 { get; set; }
    public int x2 { get; set; }
    public int x3 { get; set; }
    public int y0 { get; set; }
    public int y1 { get; set; }
    public int y2 { get; set; }
    public int y3 { get; set; }
}