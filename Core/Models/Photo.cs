
namespace Core.Models;

public class Photo
{
    public Guid Id { get; set; }
    public string Image { get; set; }
    public List<BookMarkup> BookMarkups { get; set; }
}