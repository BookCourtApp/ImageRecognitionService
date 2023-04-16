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
            context.Photos.AddRangeAsync(Photos);
            await context.SaveChangesAsync();
        }
    }
}
