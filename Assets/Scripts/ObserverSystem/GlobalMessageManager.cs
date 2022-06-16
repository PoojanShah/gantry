using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.ObserverSystem
{
    /// <summary>
    /// Handles global events across the project.
    ///
    /// There's some weird stuff here but trust me it all is necessary. I have a list of events, because for some reason
    /// on my mac, if I call GlobalEvents.ContainsKey in the AddEvent method, Unity hard crashes out with no error log.
    /// I could not figure out what was going on, but this fixed it. Everything else here is self-explanitory.
    /// </summary>
    public static class GlobalMessageManager
    {
        private static Dictionary<string, GameEvent> _globalEvents = new Dictionary<string,GameEvent>();
        private static List<string> _createdEvents = new List<string>();

        public static void AddListener(string eventName, MessageEvent handler)
        {
            if (eventName == null) return;
            if (handler == null) return;

            var existingEvent = GameEvent.GetEventName(eventName);
            if (_globalEvents.ContainsKey(existingEvent))
            {
                if (_globalEvents[existingEvent].CustomEvent == null) return;

                _globalEvents[existingEvent].CustomEvent += handler;
            }
            else
            {
                //Debug.LogError("AddListener: " + eventName);
                var newEvent = new GameEvent(eventName);
                newEvent.CustomEvent += handler;
                _globalEvents.Add(newEvent.EventName, newEvent);
            }
        }

        public static void RemoveListener(string eventName, MessageEvent removeHandler)
        {
            if (removeHandler == null) return;

            eventName = GameEvent.GetEventName(eventName);

            if (!_globalEvents.ContainsKey(eventName)) return;
            if (_globalEvents[eventName].CustomEvent == null) return;
            _globalEvents[eventName].CustomEvent -= removeHandler;
        }

        public static void AddEvent(GameEvent newEvent)
        {
            if (newEvent == null)
            {
                Debug.LogError("Received Null Event!");
                return;
            }
            if (!_createdEvents.Contains(newEvent.EventName))
            {
                _createdEvents.Add(newEvent.EventName);
            }   

            newEvent.CustomEvent += EventFired;
        }

        public static void RemoveEvent(GameEvent removeMe)
        {
            removeMe.CustomEvent -= EventFired;
        }

        /// <summary>
        /// Clears all events out. This should only be used when you reload the game, otherwise it will break everything.
        /// </summary>
        public static void ClearAll()
        {
            _createdEvents = new List<string>();
            _globalEvents = new Dictionary<string, GameEvent>();
        }

        private static void EventFired(string eventName, ref object data)
        {
            if (_globalEvents.ContainsKey(eventName))
                _globalEvents[eventName].FireEvent(ref data);
        }
    }
}