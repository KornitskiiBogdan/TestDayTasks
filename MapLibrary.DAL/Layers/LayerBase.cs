namespace MapLibrary.DAL.Layers;

public class LayerBase<T>
{
    public int Width { get; }
    public int Height { get; }

    protected T[] _tiles;

    public T this[int x, int y]
    {
        get
        {
            if (x < Width && y < Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _tiles[y * Width + x];
        }
        set
        {
            if (x < Width && y < Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            _tiles[y * Width + x] = value;
        }
    }

    protected LayerBase(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        Width = width;
        Height = height;
        _tiles = new T[checked(width * height)];
    }

    public void Resize(int width, int height)
    {
        Array.Resize(ref _tiles, width * height);
    }


    public void Fill(int startX, int startY, int width, int height, T type)
    {
        if (width < 0 || height < 0)
        {
            throw new ArgumentOutOfRangeException();
        }
        int endX = startX + width;
        int endY = startY + height;
        if (startX < 0 || startY < 0 || endX > Width || endY > Height)
        {
            throw new ArgumentOutOfRangeException();
        }

        for (int y = startY; y < endY; y++)
        {
            int rowIndex = y * Width + startX;
            Array.Fill(_tiles, type, rowIndex, width);
        }
    }
}



