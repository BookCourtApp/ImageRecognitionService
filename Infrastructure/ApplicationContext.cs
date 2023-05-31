using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationContext : DbContext
{
    public DbSet<BooksImage> BookImages { get; set; }
    public DbSet<Markup> BookMarkups { get; set; }
    public DbSet<Text> BookTexts { get; set; }
    public DbSet<LearnSession> LearnSessions { get; set; }
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options){
        Database.EnsureCreated();
    }
}

public class BooksContext : DbContext
{
    public DbSet<Book> DB { get; set; }
    public BooksContext(DbContextOptions<BooksContext> options) : base(options){
        Database.EnsureCreated();
    }
}