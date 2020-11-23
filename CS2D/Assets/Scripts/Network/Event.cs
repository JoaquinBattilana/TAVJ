namespace TAVJ {
    public class Event {
        private Type _type;
        public Type Type {
            get { return _type; }
        }

        private Packet _packet;
        public Packet Packet {
            get { return _packet; }
        }

        private bool _reliable = false;

        public Event(Packet packet) {
            _type = (Type) packet.buffer.GetBits(0, Enum.GetValues(typeof(Event)).Length);
            _packet = packet;
        }

        public enum Type {
            JOIN = 0,
            DISCONNECT = 1,
            INPUT = 2,
            JOIN_BROADCAST = 3,
            SNAPSHOT = 4,
            INPUT_ACK = 5
        }
    }
}
