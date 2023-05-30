using Newtonsoft.Json;

namespace Core.Models;

public class MarkupDto
{
    //public List<Photo> Photos { get; set; }
    public string Image { get; set; }
    public List<Markup> Markups{ get; set; }
}