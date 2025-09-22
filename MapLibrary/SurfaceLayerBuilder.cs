namespace ConsoleApp1;

public class SurfaceLayerBuilder
{
    public static SurfaceLayer CreateFromArray(IList<SurfaceType> source, int width, int height)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        var layer = new SurfaceLayer(width, height);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                layer[x, y] = source[y * width + x];
            }
        }

        return layer;
    }
}



