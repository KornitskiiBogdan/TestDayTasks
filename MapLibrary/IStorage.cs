using MapLibrary.DAL;

namespace MapLibrary.Service
{
    public interface IStorage<TObject>
    {
        Task AddAsync(TObject obj);
        Task UpdateAsync(TObject obj);
        Task<bool> RemoveAsync(string id);
        Task<TObject?> GetByIdAsync(string id);
        Task<TObject?> GetByCoordinateAsync(Сoordinate coord);
        Task<IEnumerable<TObject>> GetInAreaAsync(Area area);
    }
}
