using Core.Models;

namespace Core.Repository;

public interface IRepository{
    public Task TestAddRangeAsync(List<Photo> Photos);
    public Task<List<Photo>> GetAllPhotosAsync();
    public Task AddLearnSessionAsync(LearnSession Session);
    public Task<LearnSession> GetLatestLearnSession();
    public Task<LearnSession> GetBestLearnSession();
    public Task<LearnSession> GetLearnSessionById(Guid Id);
    public Task<List<LearnSession>> GetAllLearnSessions();
}