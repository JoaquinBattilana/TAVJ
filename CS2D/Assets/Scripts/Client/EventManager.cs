using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager {
    private List<Event> _events;

    public EventManager() {
        _events = new List<Events>();
    }

    public AddEvent(Event event) {
        _events.Add(event);
    }
}
