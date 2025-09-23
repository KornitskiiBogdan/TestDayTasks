using MapLibrary.DAL;
using MapLibrary.DAL.Events;
using MapLibrary.Service;
using MapLibrary.Utils;
using StackExchange.Redis;
using System.Text.Json;

namespace MapLibrary.Infrastructure
{
    public class ObjectRepository(
        IEventPublisher eventPublisher, 
        IDatabase redis, 
        IСoordinateConverter coordinateConverter, 
        JsonSerializerOptions options) : IStorage<DAL.Entities.Object>
    {
        private const string OBJECT_KEY_PREFIX = "object:";
        private const string GEO_KEY = "objects_geo";

        public async Task AddAsync(DAL.Entities.Object obj)
        {
            var serObj = JsonSerializer.Serialize(obj, options);

            var geoCoordinate = coordinateConverter.ToGeoCoords(obj.Coordinate);

            await redis.StringSetAsync($"{OBJECT_KEY_PREFIX}{obj.Id}", serObj);

            await redis.GeoAddAsync(GEO_KEY, geoCoordinate.Longitude, geoCoordinate.Latitude, obj.Id);

            await eventPublisher.Publish(new ObjectAdded(obj));
        }

        public async Task<DAL.Entities.Object?> GetByCoordinateAsync(Сoordinate coord)
        {
            var geoCoordinate = coordinateConverter.ToGeoCoords(coord);
            
            var results = await redis.GeoRadiusAsync(GEO_KEY, geoCoordinate.Longitude, geoCoordinate.Latitude, 1, GeoUnit.Meters);
            
            if (!results.Any())
            {
                return null;
            }

            var id = results.First().Member.ToString();
            return await GetByIdAsync(id);
        }

        public async Task<DAL.Entities.Object?> GetByIdAsync(string id)
        {
            var serObj = await redis.StringGetAsync($"{OBJECT_KEY_PREFIX}{id}");
            
            if (!serObj.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<DAL.Entities.Object>(serObj.ToString(), options);
        }

        public async Task<IEnumerable<DAL.Entities.Object>> GetInAreaAsync(Area area)
        {
            var topLeftGeo = coordinateConverter.ToGeoCoords(area.TopLeft);
            var bottomRightGeo = coordinateConverter.ToGeoCoords(area.BottomRight);

            var centerX = (area.TopLeft.X + area.BottomRight.X) / 2;
            var centerY = (area.TopLeft.Y + area.BottomRight.Y) / 2;

            var centerGeo = coordinateConverter.ToGeoCoords(new Сoordinate() { X = centerX, Y = centerY });

            var widthMeters = Math.Abs(area.BottomRight.X - area.TopLeft.X);
            var heightMeters = Math.Abs(area.BottomRight.Y - area.TopLeft.Y);

            var geoShape = new GeoSearchBox(heightMeters, widthMeters);

            var results = await redis.GeoSearchAsync(GEO_KEY,
                centerGeo.Longitude, centerGeo.Latitude, geoShape);

            var objects = new List<DAL.Entities.Object>();
            
            foreach (var result in results)
            {
                var obj = await GetByIdAsync(result.ToString());
                if (obj != null)
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            var obj = await GetByIdAsync(id);

            await redis.GeoRemoveAsync(GEO_KEY, id);
            
            var removedFromStorage = await redis.KeyDeleteAsync($"{OBJECT_KEY_PREFIX}{id}");

            if (removedFromStorage)
            {
                if (obj != null)
                {
                    await eventPublisher.Publish(new ObjectRemoved(obj));
                }
            }

            return removedFromStorage;
        }

        public async Task UpdateAsync(DAL.Entities.Object obj)
        {
            var serObj = JsonSerializer.Serialize(obj, options);
            var geoCoordinate = coordinateConverter.ToGeoCoords(obj.Coordinate);

            await redis.StringSetAsync($"{OBJECT_KEY_PREFIX}{obj.Id}", serObj);

            await redis.GeoAddAsync(GEO_KEY, geoCoordinate.Longitude, geoCoordinate.Latitude, obj.Id);

            await eventPublisher.Publish(new ObjectUpdated(obj));
        }
    }
}
