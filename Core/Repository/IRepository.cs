using Core.Models;
using Nest;

namespace Core.Repository;

public interface IRepository{
    public Task MarkupAddAsync(BooksImage Image);
    public Task AddLearnSessionAsync(LearnSession Session);
    public Task<LearnSession> GetLatestLearnSession();
    public Task<LearnSession> GetBestLearnSession();
    public Task<LearnSession> GetLearnSessionById(Guid Id);
    public Task<List<LearnSession>> GetAllLearnSessions();
    public Task<ISearchResponse<BookInfo>> GetBooksByKeywordAsync(string Keywords);
    public Task TransferData();
}