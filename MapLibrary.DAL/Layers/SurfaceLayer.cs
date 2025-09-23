namespace MapLibrary.DAL.Layers;

public class SurfaceLayer : LayerBase<SurfaceType>
{
    public SurfaceLayer(int width, int height, SurfaceType defaultType = SurfaceType.Plain) : base(width, height)
    {
        if (defaultType != SurfaceType.Plain)
        {
            Fill(0, 0, width, height, defaultType);
        }
    }

    public bool CanPlaceObjectInArea(int startX, int startY, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        int endX = startX + width;
        int endY = startY + height;

        if (startX < 0 || startY < 0 || endX > Width || endY > Height)
        {
            return false;
        }

        for (int y = startY; y < endY; y++)
        {
            int baseIndex = y * Width + startX;
            for (int x = 0; x < width; x++)
            {
                if (_tiles[baseIndex + x] != SurfaceType.Plain)
                {
                    return false;
                }
            }
        }

        return true;
    }
}



