using MapLibrary.DAL;
using MapLibrary.DAL.Events;
using MapLibrary.Service;
using MapLibrary.Utils;
using StackExchange.Redis;
using System.Text.Json;

namespace MapLibrary.Infrastructure
{
    public class ObjectLayer(IEventPublisher eventPublisher, 
        IDatabase redis, 
        IСoordinateConverter coordinateConverter, 
        JsonSerializerOptions options) : IStorage<DAL.Entities.Object>
    {
        public async Task AddAsync(DAL.Entities.Object obj)
        {
            var serObj = JsonSerializer.Serialize(obj, options);

            var geoCoordinate = coordinateConverter.ToGeoCoords(obj.Coordinate);

            await redis.GeoAddAsync(obj.Id, geoCoordinate.Longitude, geoCoordinate.Longitude, serObj);

            await eventPublisher.Publish(new ObjectAdded(obj));
        }

        public Task<DAL.Entities.Object?> GetByCoordinateAsync(Сoordinate coord)
        {
            throw new NotImplementedException();
        }

        public Task<DAL.Entities.Object?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DAL.Entities.Object>> GetInAreaAsync(Area area)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(DAL.Entities.Object obj)
        {
            throw new NotImplementedException();
        }
    }
}
