using Core.Models;

namespace Core.Repository;

public interface IRepository{
    public Task TestAddRangeAsync(List<Photo> Photos);
}