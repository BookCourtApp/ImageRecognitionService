using System.ComponentModel.DataAnnotations;

namespace Core.Models;

public class Book
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? SourceName {get;set;}
    public string? Image { get; set; }
    public string? Genre { get; set; }
    public string? NumberOfPages { get; set; }
    public string? ISBN { get; set; }
    public string? ParsingDate { get; set; }
    public string? PublisherYear { get; set; }
    public string? SiteBookId { get; set; }
    public string? Breadcrqmbs { get; set; }
    public string? SourceUrl { get; set; }
}