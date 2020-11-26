using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace TAVJ {
    public class NetworkManager {
        private Channel _channel;
        private float _latency = 0;
        private List<TimeoutPacket> _notAckHits;
        private int _notAckHitsIncrement;
        private int _ackHitsMax;
        private MonoBehaviour _monobehaviour;

        public NetworkManager(string ip, int clientPort, int serverPort, MonoBehaviour monobehavior) {
            _channel = new Channel(ip, clientPort, serverPort);
            _notAckHits = new List<TimeoutPacket>();
            _notAckHitsIncrement = 0;
            _ackHitsMax = 0;
            _monobehaviour = monobehavior;
        }
        public NetworkManager(int port) {
            _channel = new Channel(port);
            _notAckHits = new List<TimeoutPacket>();
            _notAckHitsIncrement = 0;
            _ackHitsMax = 0;
        }

        public void LatencyUp() {
            _latency += 0.1f;
            Debug.Log(_latency);
        }

        public void LatencyDown() {
            if(_latency >= 0.1f) {
                _latency -= 0.1f;
            } else {
                _latency = 0f;
            }
            Debug.Log(_latency);
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
                client.Serialize(packet.buffer);
            }
            packet.buffer.Flush();
            _channel.Send(packet, clientData.endpoint);
            packet.Free();
        }

        public void SendJoinBroadcast(ClientData clientData, List<ClientData> clients) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.JOIN_BROADCAST, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            clientData.Serialize(packet.buffer);
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
            if(_latency > 0) {
                _monobehaviour.StartCoroutine(SendLaggedPacket(packet));
            } else {
                _channel.Send(packet);
            }
        }

        public IEnumerator SendLaggedPacket(Packet packet) {
            yield return new WaitForSeconds(_latency);
            _channel.Send(packet);
        }

        public void ResendNotAckPackets() {
            foreach(TimeoutPacket packet in _notAckHits) {
                if(packet.IsExpired()) {
                    _channel.Send(packet.OriginalPacket);
                }
            }
        }

        public void DeleteAckPackets(int packetNumber) {
            foreach(TimeoutPacket timeoutPacket in _notAckHits ) {
                if(timeoutPacket.PacketNumber <= packetNumber ) {
                    timeoutPacket.OriginalPacket.Free();
                }
            }
            _notAckHits = _notAckHits.Where(elem => elem.PacketNumber >= packetNumber).ToList();
        }

        public void TimeHasPass(float time) {
            foreach(TimeoutPacket timeoutPacket in _notAckHits) {
                timeoutPacket.LessTime(time);
            }
        }

        public void SendSnapshots(List<ClientData> clients) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.SNAPSHOT, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clients.Count);
            if(clients.Count > 0) {
                foreach (var client in clients) {
                    client.Serialize(packet.buffer);
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

        public void SendHit(int clientId, int idHit) {
            Packet packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.HIT, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clientId);
            packet.buffer.PutInt(idHit);
            packet.buffer.PutInt(_notAckHitsIncrement);
            packet.buffer.Flush();
            _notAckHits.Add(new TimeoutPacket(_notAckHitsIncrement, packet, 1000f));
            _notAckHitsIncrement++;
            _channel.Send(packet);
        }

        public NetworkEvent GetEvent() {
            Packet packet = _channel.GetPacket();
            if(packet != null) {
                return new NetworkEvent(packet);
            }
            return null;
        }

        public void SendHitAck(ClientData player, int ack) {
            Packet packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.HIT_ACK, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(ack);
            packet.buffer.Flush();
            _channel.Send(packet, player.endpoint);
            packet.Free();
        }

        public void Disconnect() {
            _channel.Disconnect();
        }

        public void DisconnectFromServer(int clientId) {
            Packet packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.DISCONNECT, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clientId);
            packet.buffer.Flush();
            _channel.Send(packet);
            packet.Free();
        }

        public void SendDisconnectBroadcast(int clientId, List<ClientData> clients) {
            Packet packet = Packet.Obtain();
            packet.buffer.PutBits((int) NetworkEvent.EventType.DISCONNECT_BROADCAST, 0, Enum.GetValues(typeof(NetworkEvent.EventType)).Length);
            packet.buffer.PutInt(clientId);
            packet.buffer.Flush();
            SendToAllClients(clients, packet);
            packet.Free();
        }

    }
}
