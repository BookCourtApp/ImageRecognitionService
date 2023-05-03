using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationContext : DbContext
{
    public DbSet<Photo> Photos { get; set; }
    public DbSet<BookMarkup> BookMarkups { get; set; }
    public DbSet<TextMarkup> TextMarkups { get; set; }
    public DbSet<LearnSession> LearnSessions { get; set; }
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options){
        Database.EnsureCreated();
    }
}
