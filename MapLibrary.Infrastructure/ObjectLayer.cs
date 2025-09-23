using MapLibrary.DAL;
using MapLibrary.DAL.Events;
using MapLibrary.Service;
using MapLibrary.Utils;
using StackExchange.Redis;
using System.Text.Json;
using System.Linq;

namespace MapLibrary.Infrastructure
{
    public class ObjectLayer(IEventPublisher eventPublisher, 
        IDatabase redis, 
        IСoordinateConverter coordinateConverter, 
        JsonSerializerOptions options) : IStorage<DAL.Entities.Object>
    {
		private const string GeoKey = "map:objects:geo";
		private static string ObjectKey(string id) => $"map:objects:{id}";

        public async Task AddAsync(DAL.Entities.Object obj)
        {
			var serObj = JsonSerializer.Serialize(obj, options);

			var geoCoordinate = coordinateConverter.ToGeoCoords(obj.Coordinate);

			// Store object payload by ID
			await redis.StringSetAsync(ObjectKey(obj.Id), serObj).ConfigureAwait(false);
			// Index object position in common geo set by its ID
			await redis.GeoAddAsync(GeoKey, geoCoordinate.Longitude, geoCoordinate.Latitude, obj.Id).ConfigureAwait(false);

			await eventPublisher.Publish(new ObjectAdded(obj));
        }

		public async Task<DAL.Entities.Object?> GetByCoordinateAsync(Сoordinate coord)
        {
			var geoCoordinate = coordinateConverter.ToGeoCoords(coord);
			// Find nearby candidates (small radius) and filter by containment
			var candidates = await redis.GeoRadiusAsync(GeoKey, geoCoordinate.Longitude, geoCoordinate.Latitude, 1, GeoUnit.Kilometers).ConfigureAwait(false);
			if (candidates is null || candidates.Length == 0)
			{
				return null;
			}

			foreach (var entry in candidates)
			{
				var id = (string)entry.Member;
				var obj = await GetByIdAsync(id).ConfigureAwait(false);
				if (obj is null)
				{
					continue;
				}
				if (PointInsideObject(coord, obj))
				{
					return obj;
				}
			}

			return null;
        }

		public async Task<DAL.Entities.Object?> GetByIdAsync(string id)
        {
			var payload = await redis.StringGetAsync(ObjectKey(id)).ConfigureAwait(false);
			if (!payload.HasValue)
			{
				return null;
			}
			return JsonSerializer.Deserialize<DAL.Entities.Object>(payload.ToString(), options);
        }

		public async Task<IEnumerable<DAL.Entities.Object>> GetInAreaAsync(Area area)
        {
			// Approximate search by box around area center, then filter precisely in tile space
			var centerX = (area.TopLeft.X + area.BottomRight.X) / 2.0;
			var centerY = (area.TopLeft.Y + area.BottomRight.Y) / 2.0;
			var centerGeo = coordinateConverter.ToGeoCoords(new Сoordinate { X = (int)centerX, Y = (int)centerY });

			// Radius that covers the box diagonally (heuristic)
			var topLeftGeo = coordinateConverter.ToGeoCoords(area.TopLeft);
			var bottomRightGeo = coordinateConverter.ToGeoCoords(area.BottomRight);
			double dLon = Math.Abs(topLeftGeo.Longitude - bottomRightGeo.Longitude);
			double dLat = Math.Abs(topLeftGeo.Latitude - bottomRightGeo.Latitude);
			double diag = Math.Sqrt(dLon * dLon + dLat * dLat);
			var radiusKm = Math.Max(diag, 0.001); // avoid zero radius

			var candidates = await redis.GeoRadiusAsync(GeoKey, centerGeo.Longitude, centerGeo.Latitude, radiusKm, GeoUnit.Kilometers).ConfigureAwait(false);
			if (candidates is null || candidates.Length == 0)
			{
				return Enumerable.Empty<DAL.Entities.Object>();
			}

			var result = new List<DAL.Entities.Object>(candidates.Length);
			foreach (var entry in candidates)
			{
				var id = (string)entry.Member;
				var obj = await GetByIdAsync(id).ConfigureAwait(false);
				if (obj is null)
				{
					continue;
				}
				if (RectangleIntersectsArea(obj, area))
				{
					result.Add(obj);
				}
			}

			return result;
        }

		public async Task<bool> RemoveAsync(string id)
        {
			var existing = await GetByIdAsync(id).ConfigureAwait(false);
			if (existing is null)
			{
				return false;
			}

			// Remove from geo index and payload storage
			await redis.SortedSetRemoveAsync(GeoKey, id).ConfigureAwait(false);
			await redis.KeyDeleteAsync(ObjectKey(id)).ConfigureAwait(false);

			await eventPublisher.Publish(new ObjectRemoved(existing));
			return true;
        }

		public async Task UpdateAsync(DAL.Entities.Object obj)
        {
			var existing = await GetByIdAsync(obj.Id).ConfigureAwait(false);
			if (existing is null)
			{
				// Treat as add if not exists
				await AddAsync(obj).ConfigureAwait(false);
				return;
			}

			var serObj = JsonSerializer.Serialize(obj, options);
			await redis.StringSetAsync(ObjectKey(obj.Id), serObj).ConfigureAwait(false);

			var geoCoordinate = coordinateConverter.ToGeoCoords(obj.Coordinate);
			await redis.GeoAddAsync(GeoKey, geoCoordinate.Longitude, geoCoordinate.Latitude, obj.Id).ConfigureAwait(false);

			await eventPublisher.Publish(new ObjectUpdated(obj));
        }

		private static bool PointInsideObject(Сoordinate point, DAL.Entities.Object obj)
		{
			int left = obj.Coordinate.X;
			int top = obj.Coordinate.Y;
			int right = left + obj.Width - 1;
			int bottom = top + obj.Height - 1;
			return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
		}

		private static bool RectangleIntersectsArea(DAL.Entities.Object obj, Area area)
		{
			int objLeft = obj.Coordinate.X;
			int objTop = obj.Coordinate.Y;
			int objRight = objLeft + obj.Width - 1;
			int objBottom = objTop + obj.Height - 1;

			int areaLeft = area.TopLeft.X;
			int areaTop = area.TopLeft.Y;
			int areaRight = area.BottomRight.X;
			int areaBottom = area.BottomRight.Y;

			bool separated = objRight < areaLeft || objLeft > areaRight || objBottom < areaTop || objTop > areaBottom;
			return !separated;
		}
    }
}
