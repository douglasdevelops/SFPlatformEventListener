using static SFEventListenerWeb.EventMessage;

namespace SFEventListenerWeb.Services
{
    public interface IEventService
    {
        void Subscribe(string eventName, string accessToken, string instanceUrl, EventReceivedHandler handler);
        void Unsubscribe(string eventName, EventReceivedHandler handler);
        void AddMessage(EventMessage message);

        public Task<List<string>> GetPlatformEventsAsync(string instanceUrl, string accessToken);
        List<EventMessage> GetMessages();
    }
}
