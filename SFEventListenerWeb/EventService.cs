namespace SFEventListenerWeb
{
    public class EventMessage
    {
        public string EventName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public delegate void EventReceivedHandler(EventMessage message);
    }
}
