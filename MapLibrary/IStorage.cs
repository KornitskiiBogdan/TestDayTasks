using MapLibrary.DAL;

namespace MapLibrary.Service
{
    public interface IStorage<TObject>
    {
        Task AddAsync(TObject obj);
        Task UpdateAsync(TObject obj);
        Task<bool> RemoveAsync(uint id);
        Task<TObject?> GetByIdAsync(uint id);
        Task<TObject?> GetByCoordinateAsync(Cordinate coord);
        Task<IEnumerable<TObject>> GetInAreaAsync(Area area);
    }
}
