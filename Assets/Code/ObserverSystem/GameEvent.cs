namespace Assets.Code.ObserverSystem
{
    public delegate void MessageEvent(string eventName, ref object data);

    /// <summary>
    /// A generic event which can pass data, or pass nothing.
    /// </summary>
    public class GameEvent
    {
        public MessageEvent CustomEvent;
        private readonly string _eventName;
        private object _eventData = null;

        public string EventName
        {
            get { return _eventName; }
        }

        public GameEvent(string eventName)
        {
            _eventName = GetEventName(eventName);
        }

        public void FireEvent()
        {
            if (CustomEvent != null)
                CustomEvent.Invoke(_eventName, ref _eventData);
        }

        public void FireEvent(ref object data)
        {
            if (CustomEvent != null)
                CustomEvent.Invoke(_eventName, ref data);
        }

        /// <summary>
        /// Spaces were causing problems with events, so I created this.
        /// </summary>
        public static string GetEventName(string eventName)
        {
            eventName = eventName.Replace(" ", "");
            if (!eventName.EndsWith("EventId"))
            {
                eventName += "EventId";
            }

            return eventName;
        }
    }
}