using MapLibrary.DAL.Events;

namespace MapLibrary.Service
{
    public interface IEventPublisher
    {
        void Publish<TEvent>(TEvent @event) where TEvent : EventBase;
    }
}
