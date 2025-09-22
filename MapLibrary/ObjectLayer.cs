namespace ConsoleApp1;

public class ObjectLayer : SurfaceLayer
{
    public event Action<Object> OnAdded;
    public event Action<Object> OnRemoved;
    public event Action<Object> OnChanged;

    public ObjectLayer(int width, int height) : base(width, height)
    {
    }

    public Object Get(string id)
    {

    }

    public void Remove(Object @object)
    {

    }

    public Object Get(Coordinate coordinate)
    {

    }

    public IEnumerable<Object> Get(Area area)
    {

    }

    public bool CheckInArea(Object @object, Area area)
    {

    }
}



