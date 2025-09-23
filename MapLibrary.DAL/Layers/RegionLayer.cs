using MapLibrary.DAL.Entities;

namespace MapLibrary.DAL.Layers
{
    public class RegionLayer : LayerBase<uint>
    {
        private const int SEED = 2025;
        private readonly Dictionary<uint, RegionObject> _regions = new ();
        public RegionLayer(int width, int height) : base(width, height)
        {
        }

        public void AddRegion(RegionObject regionObject)
        {
            var success =  _regions.TryAdd(regionObject.Id, regionObject);

            if(!success)
            {
                throw new InvalidOperationException($"The region with the id: {regionObject.Id} has already been added.");
            }

			var startX = regionObject.Coordinate.X;
			var startY = regionObject.Coordinate.Y;

			Fill(startX, startY, regionObject.Width, regionObject.Height, regionObject.Id);
        }

        public uint GetTyleId(int x, int y) => this[x, y];

        public RegionObject? GetMetadataById(uint id) => _regions.TryGetValue(id, out var result) ? result : null;

        public bool CheckTyleInRegion(int tyleX, int tyleY, RegionObject regionObject) => this[tyleX, tyleY] == regionObject.Id;

        public IEnumerable<RegionObject> GetRegionsInArea(Area area)
        {
			var ids = new HashSet<uint>();

			for(int x = area.TopLeft.X; x < area.BottomRight.X; x++)
			{
                for (int y = area.TopLeft.Y; y < area.BottomRight.Y; y++)
                {
					var id = this[x, y];
					if(id != 0)
					{
						ids.Add(this[x, y]);
					}
                }
            }

			foreach(var id in ids)
			{
				if(_regions.TryGetValue(id, out var region))
				{
					yield return region;
				}
			}
        }

		public void GenerateRegions(uint inputRegionWidth, uint inputRegionHeight)
		{
			if (inputRegionWidth == 0 || inputRegionHeight == 0)
			{
				throw new ArgumentOutOfRangeException("Region dimensions must be greater than zero.");
			}

			int maxRegionWidth = (int)Math.Min(inputRegionWidth, (uint)Width);
			int maxRegionHeight = (int)Math.Min(inputRegionHeight, (uint)Height);

			if (maxRegionWidth <= 0 || maxRegionHeight <= 0)
			{
				return;
			}
			
			var random = new Random(SEED);
			uint nextId = 1;

			int startX = 0;
			int startY = 0;

			// Пытаемся разместить регионы в случайных позициях без пересечений
			while (true)
			{
				int regionHeight = random.Next(0, maxRegionHeight);
				int regionWidth = random.Next(0, maxRegionWidth);

				// Проверяем, что вся целевая область пуста (исключаем перекрытия)
				bool isEmpty = true;
				for (int y = startY; y < startY + regionHeight && isEmpty; y++)
				{
					for (int x = 0; x < startX + regionWidth; x++)
					{
						if (this[x, y] != 0)
						{
							isEmpty = false;
							break;
						}
					}
				}

				maxRegionHeight -= regionHeight;
                maxRegionWidth -= regionWidth;

                if (!isEmpty)
				{
                    
                    startX += regionWidth;
                    startY += regionHeight;

                    continue;
				}

				var region = new RegionObject
				{
					Id = nextId++,
					Coordinate = new Сoordinate { X = startX, Y = startY },
					Width = regionWidth,
					Height = regionHeight,
					Name = $"Region {nextId - 1}"
				};

				AddRegion(region);
			}
		}
    }
}