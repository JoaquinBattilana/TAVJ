using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAVJ {
    public class EventManager {
        private List<NetworkEvent> _events;

        public EventManager() {
            _events = new List<NetworkEvent>();
        }

        public void AddEvent(NetworkEvent nEvent) {
            _events.Add(nEvent);
        }
    }
}