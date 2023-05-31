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

public class BookInfo
{
    public string Info { get; set; }
}

public class ElasticResponse
{
    public string BookInfo { get; set; }
    public int EntryPoints { get; set; }
}

public class MakrupRepository : IRepository
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    private readonly IDbContextFactory<BooksContext> _contextBooksFactory;
    private readonly IElasticClient _elasticClient;
    public MakrupRepository(IDbContextFactory<ApplicationContext> contextFactory, IDbContextFactory<BooksContext> contextBooksFactory, IElasticClient elasticClient){
        _contextFactory = contextFactory;
        _contextBooksFactory = contextBooksFactory;
        _elasticClient = elasticClient;
    }
    /*DEPRECATED*/ //public async Task TestAddRangeAsync(List<Photo> Photos){
    //    using (var context = await _contextFactory.CreateDbContextAsync())
    //    {
    //        await context.BookMarkups.AddRangeAsync(Photos);
    //        await context.SaveChangesAsync();
    //    }
    //}
    public async Task MarkupAddAsync(BooksImage Image){
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            await context.BookImages.AddAsync(Image);
            await context.SaveChangesAsync();
        }
    }
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
    public async Task TransferData(){
        using (var context = await _contextBooksFactory.CreateDbContextAsync())
        {
            var Books =
                await context.DB
                .Select(b => new BookDto{Name = b.Name, Author = b.Author})
                .ToListAsync();
            foreach(var Book in Books){
                try{
                    if(Book.Name == null){
                        Book.Name = "";
                    }
                    if(Book.Author == null){
                        Book.Author = "";
                    }
                    var ClearName = Regex.Replace(Book.Name, @"[\p{P}\p{S}]+", "");
                    var ClearAuthor = Regex.Replace(Book.Author, @"[\p{P}\p{S}]+", "");
                    _elasticClient.IndexDocument($"{ClearName} | {ClearAuthor}");
                }catch(Exception ex){
                    Console.WriteLine("Something went wrong");
                }
            }
        }

    }
    public async Task<List<string>> GetPossibleBooksAsync(List<string> Keywords){
        using (var context = await _contextBooksFactory.CreateDbContextAsync())
        {
            List<List<ElasticResponse>> Responses = new List<List<ElasticResponse>>();
            foreach(var keyword in Keywords){
                var searchRequest = _elasticClient.Search<BookInfo>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Info)
                            .Query(keyword)
                        )
                    )
                );

                var Response = new List<ElasticResponse>();
                foreach (var hit in searchRequest.Hits)
                {
                    //Console.WriteLine(hit.Source.Info);
                    Response.Add(new ElasticResponse{ 
                        BookInfo = hit.Source.Info,
                        EntryPoints = 0
                        }
                    );
                }
                Responses.Add(Response);
            }
            // Вывод всех найденных книг в бд книг //foreach(var Response in Responses){
            //    Console.Write("----------");
            //    foreach(var Structure in Response){
            //        Console.WriteLine(Structure.BookInfo);
            //    }
            //}
            for (var i = 0; i < Responses.Count; i++){
                for (var b = i+1; b < Responses.Count; b++){
                    for (var c = 0; c < Responses[i].Count; c++){
                        foreach (var BookName in Responses[b]){
                            if (Responses[i][c].BookInfo == BookName.BookInfo){
                                Responses[i][c].EntryPoints++;
                            }
                        }
                    }
                }
            }
            List<string> Result = new List<string>();
            if(Responses.Count == 1){
                foreach(var Response in Responses){
                    foreach(var Structure in Response){
                        Result.Add(Structure.BookInfo);
                    }
                }
                return Result;
            }
            var IsThereAnyGoodEntries = 0;
            foreach(var Response in Responses){
                foreach(var Structure in Response){
                    if(Structure.EntryPoints > 0){
                        IsThereAnyGoodEntries++;
                        Result.Add(Structure.BookInfo);
                    }
                }
            }
            if(IsThereAnyGoodEntries == 0){
                foreach(var Response in Responses){
                    foreach(var Structure in Response){
                        Result.Add(Structure.BookInfo);
                    }
                }
            }
            return Result;
        }
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
