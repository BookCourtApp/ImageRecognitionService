using Microsoft.EntityFrameworkCore;

using Core.Repository;
using Core.Models;

namespace Infrastructure.DataAccessLayer;

public class MakrupRepository : IRepository
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    public MakrupRepository(IDbContextFactory<ApplicationContext> contextFactory){
        _contextFactory = contextFactory;
    }
    public async Task TestAddRangeAsync(List<Photo> Photos){
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            await context.Photos.AddRangeAsync(Photos);
            await context.SaveChangesAsync();
        }
    }
    public async Task<List<Photo>> GetAllPhotosAsync(){
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var Photos = await context.Photos
                .Include(p => p.BookMarkups)
                    .ThenInclude(t => t.TextMarkups)
                .ToListAsync();
            return Photos;
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
