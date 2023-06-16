using Microsoft.EntityFrameworkCore;
using Nest;
using Elasticsearch.Net;
using System.Text.RegularExpressions;

using Core.Repository;
using Core.Models;

namespace Infrastructure.DataAccessLayer;
public class BookDto
{
    public string? Name { get; set; }
    public string? Author { get; set; }
};

public class MakrupRepository : IRepository
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    private readonly IDbContextFactory<BooksContext> _contextBooksFactory;
    private readonly IElasticClient _elasticClient;
    public MakrupRepository(
        IDbContextFactory<ApplicationContext> contextFactory, 
        IDbContextFactory<BooksContext> contextBooksFactory, 
        IElasticClient elasticClient)
        {
        _contextFactory = contextFactory;
        _contextBooksFactory = contextBooksFactory;
        _elasticClient = elasticClient;
        }
    public async Task MarkupAddAsync(BooksImage Image){
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            await context.BookImages.AddAsync(Image);
            await context.SaveChangesAsync();
        }
    }
    public async Task TransferData(){
        using (var context = await _contextBooksFactory.CreateDbContextAsync())
        {
            var Books =
                await context.DB
                .Select(b => new BookDto{Name = b.Name, Author = b.Author})
                .ToListAsync();
            foreach(var Book in Books){
                try{
                    var InfoDto = new BookInfo();
                    if(Book.Name == null){
                        Book.Name = "";
                    }
                    if(Book.Author == null){
                        Book.Author = "";
                    }
                    var ClearName = Regex.Replace(Book.Name, @"[\p{P}\p{S}]+", "");
                    var ClearAuthor = Regex.Replace(Book.Author, @"[\p{P}\p{S}]+", "");
                    InfoDto.Info = $"{ClearName} | {ClearAuthor}";
                    _elasticClient.IndexDocument(InfoDto);
                }catch(Exception ex){
                    Console.WriteLine("Something went wrong");
                }
            }
        }

    }
    public async Task<ISearchResponse<BookInfo>>? GetBooksByKeywordAsync(string Keyword)
    {
        var searchRequest = await _elasticClient.SearchAsync<BookInfo>(s => s
            .Query(q => q
                //.Match(m => m
                //    .Field(f => f.Info)
                //    .Query(Keyword)
                //)
                .Fuzzy(m => m
                    .Field(f => f.Info)
                    .Value(Keyword)
                    .Fuzziness(Fuzziness.Auto)
                    .MaxExpansions(3)
                )
            )
            .Size(5000)
        );
        return searchRequest;
    }
    public async Task AddLearnSessionAsync(LearnSession Session){
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            await context.LearnSessions.AddAsync(Session);
            await context.SaveChangesAsync();
        }
    }

    public async Task<LearnSession> GetLatestLearnSession()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var LatestLearnSession = await context.LearnSessions
                .OrderByDescending(p => p.LearnDate)
                .FirstOrDefaultAsync();
            return LatestLearnSession;
        }

    }
    public async Task<LearnSession> GetBestLearnSession()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var LatestLearnSession = await context.LearnSessions
                .OrderByDescending(p => p.Precision)
                .FirstOrDefaultAsync();
            return LatestLearnSession;
        }

    }
    public async Task<LearnSession> GetLearnSessionById(Guid Id)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var LatestLearnSession = await context.LearnSessions
                .FindAsync(Id);
            return LatestLearnSession;
        }

    }
    public async Task<List<LearnSession>> GetAllLearnSessions()
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var LatestLearnSession = await context.LearnSessions
                .ToListAsync();
            return LatestLearnSession;
        }

    }
}

    //public async Task TransferDataTest(){
    //    var myDoc = new Doc{
    //        Id = 1,
    //        Name = "Petya"
    //    };

    //    _elasticClient.IndexDocument(myDoc);
    //}
//public class Doc
//{
//    public int Id {get;set;}
//    public string Name {get;set;}
//}
    /*DEPRECATED*/ //public async Task TestAddRangeAsync(List<Photo> Photos){
    //    using (var context = await _contextFactory.CreateDbContextAsync())
    //    {
    //        await context.BookMarkups.AddRangeAsync(Photos);
    //        await context.SaveChangesAsync();
    //    }
    //}
    //public async Task<List<Photo>> GetAllPhotosAsync(){
    //    using (var context = await _contextFactory.CreateDbContextAsync())
    //    {
    //        var Photos = await context.Photos
    //            .Include(p => p.BookMarkups)
    //                .ThenInclude(t => t.TextMarkups)
    //            .ToListAsync();
    //        return Photos;
    //    }
    //}