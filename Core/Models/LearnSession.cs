
namespace Core.Models;

public class LearnSession
{
    public Guid Id { get; set; }
    public string Version { get; set; } = default!;
    public float LearnTime { get; set; } = default!;
    public float Precision { get; set; } = default!;
    public DateTime LearnDate { get; set; } = default!;
    public string Weights { get; set; } = default!;
}