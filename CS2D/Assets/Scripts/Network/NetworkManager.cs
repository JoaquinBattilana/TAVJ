using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TAVJ {
    public class NetworkManager {
        private Channel _channel;

        public NetworkManager(string ip, int clientPort, int serverPort) {
            _channel = new Channel(ip, clientPort, serverPort);
        }

        public void SendJoin() {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.JOIN, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.Flush();
            _channel.Send(packet);
            packet.Free();
        }

        public void SendInputs(int clientId, List<ClientInput> inputs) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.INPUT, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.PutInt(clientId);
            packet.buffer.PutInt(inputs.Count);
            foreach(ClientInput input in inputs) {
                input.Serialize(packet.buffer);
            }
            packet.buffer.Flush();
            _channel.Send(packet);
            packet.Free();
        }

        public Packet GetPacket() {
            return _channel.GetPacket();
        }

        public Event GetEvent() {
            Packet packet = _channel.GetPacket();
            if(packet != null) {
                return new Event(packet);
            }
            return null;
        }

        public void Disconnect() {
            _channel.Disconnect();
        }
    }
}
