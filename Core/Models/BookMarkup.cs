using System.ComponentModel.DataAnnotations;

namespace Core.Models;

public class BookMarkup
{
    public Guid Id { get; set; }
    public List<TextMarkup> TextMarkups { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    
}