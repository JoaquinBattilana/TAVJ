using System;

namespace TAVJ {
    public class NetworkEvent {
        private EventType _type;
        public EventType Type {
            get { return _type; }
        }

        private Packet _packet;
        public Packet Packet {
            get { return _packet; }
        }

        private bool _reliable = false;

        public NetworkEvent(Packet packet) {
            _type = (EventType) packet.buffer.GetBits(0, Enum.GetValues(typeof(EventType)).Length);
            _packet = packet;
        }

        public enum EventType {
            JOIN = 0,
            DISCONNECT = 1,
            INPUT = 2,
            JOIN_BROADCAST = 3,
            SNAPSHOT = 4,
            INPUT_ACK = 5,
            HIT = 6,
            HIT_ACK = 7,
            DISCONNECT_BROADCAST= 8
        }
    }
}
