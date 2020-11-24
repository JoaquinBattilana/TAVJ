using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace TAVJ {
    public class NetworkManager {
        private Channel _channel;

        public NetworkManager(string ip, int clientPort, int serverPort) {
            _channel = new Channel(ip, clientPort, serverPort);
        }
        public NetworkManager(int port) {
            _channel = new Channel(port);
        }

        public void SendJoin() {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.JOIN, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.Flush();
            _channel.Send(packet);
            packet.Free();
        }

        public void SendJoinAck(ClientData clientData, List<ClientData> clients) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.JOIN, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clientData.id);
            packet.buffer.PutInt(clients.Count);
            foreach (var client in clients) {
                client.SerializePosition(packet.buffer);
            }
            packet.buffer.Flush();
            _channel.Send(packet, clientData.endpoint);
            packet.Free();
        }

        public void SendJoinBroadcast(ClientData clientData, List<ClientData> clients) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.JOIN_BROADCAST, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            clientData.SerializePosition(packet.buffer);
            packet.buffer.Flush();
            SendToAllClientsExceptSender(clients, packet, clientData.id);
            packet.Free();
        }

        public void SendInputs(int clientId, InputManager inputManager) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.INPUT, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clientId);
            packet.buffer.PutInt(inputManager.Inputs.Count);
            inputManager.Serialize(packet.buffer);
            packet.buffer.Flush();
            _channel.Send(packet);
            packet.Free();
        }

        public void SendInputAck(ClientData client) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.INPUT_ACK, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(client.InputManager.MostBigInput);
            packet.buffer.Flush();
            _channel.Send(packet, client.endpoint);
            packet.Free();
        }

        public void SendSnapshots(List<ClientData> clients) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.SNAPSHOT, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            var clientsMoving = clients.Where(elem => elem.IsMoving()).ToList();
            packet.buffer.PutInt(clientsMoving.Count);
            if(clientsMoving.Count > 0) {
                foreach (var client in clientsMoving) {
                    client.SerializePosition(packet.buffer);
                }
            }
            packet.buffer.Flush();
            SendToAllClients(clients, packet);
            packet.Free();
        }

        private void SendToAllClients(List<ClientData> clients, Packet packet) {
            foreach (var client in clients) {
                _channel.Send(packet, client.endpoint);
            }
        }

        private void SendToAllClientsExceptSender(List<ClientData> clients, Packet packet, int id) {
            foreach (var client in clients) {
                if(client.id != id) {
                    _channel.Send(packet, client.endpoint);
                }
            }
        }

        public NetworkEvent GetEvent() {
            Packet packet = _channel.GetPacket();
            if(packet != null) {
                return new NetworkEvent(packet);
            }
            return null;
        }

        public void Disconnect() {
            _channel.Disconnect();
        }

    }
}
