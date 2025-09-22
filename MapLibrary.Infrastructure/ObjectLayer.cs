using MapLibrary.DAL;
using MapLibrary.DAL.Entities;
using MapLibrary.Service;

namespace MapLibrary.Infrastructure
{
    public class ObjectLayer : IStorage<Object>
    {
        public Task AddAsync(DAL.Entities.Object obj)
        {
            throw new NotImplementedException();
        }

        public Task<DAL.Entities.Object?> GetByCoordinateAsync(Cordinate coord)
        {
            throw new NotImplementedException();
        }

        public Task<DAL.Entities.Object?> GetByIdAsync(uint id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DAL.Entities.Object>> GetInAreaAsync(Area area)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(uint id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(DAL.Entities.Object obj)
        {
            throw new NotImplementedException();
        }
    }
}
