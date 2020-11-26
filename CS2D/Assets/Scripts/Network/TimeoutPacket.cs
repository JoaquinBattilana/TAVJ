using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TAVJ {
    public class TimeoutPacket {
        private int _packetNumber;
        public int PacketNumber {
            get { return _packetNumber; }
        }
        private Packet _packet;
        public Packet OriginalPacket {
            get { return _packet; }
        }

        private float _timeout;

        public TimeoutPacket(int packetNumber, Packet packet, float timeout) {
            _packetNumber = packetNumber;
            _packet = packet;
            _timeout = timeout;
        }

        public bool IsExpired() {
            return _timeout < 0;
        }

        public void LessTime(float time) {
            _timeout -= time;
        }
    }

}
