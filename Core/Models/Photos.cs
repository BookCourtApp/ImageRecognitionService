
namespace Core.Models;

public class Photo
{
    public Guid Id { get; set; }
    public byte[] Image { get; set; }
    public ICollection<BookMarkup> BookMarkups { get; set; }
}