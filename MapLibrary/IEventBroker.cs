using MapLibrary.DAL.Events;

namespace MapLibrary.Service
{
    public interface IEventPublisher
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : EventBase;
    }
}
