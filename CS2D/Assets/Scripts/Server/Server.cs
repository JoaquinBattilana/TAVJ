using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using System.Linq;

namespace TAVJ {
    public class Server : MonoBehaviour
    {
        private int _serverPort = 9000;
        public GameObject playerServerPrefab;
        private List<ClientData> _clients;
        private int _pps;
        private float _acumTime;
        private NetworkManager _networkManager;

        void Awake() {
            if(PlayerPrefs.HasKey("serverPort")) {
                _serverPort = PlayerPrefs.GetInt("serverPort");
            }
            _networkManager = new NetworkManager(_serverPort);
            _clients = new List<ClientData>();
        }

        void Start() {
            _pps = 20;
            _acumTime = 0f;
        }

        private void OnDestroy() {
            _networkManager.Disconnect();
        }

        void Update() {
            NetworkEvent nEvent = _networkManager.GetEvent();
            while ( nEvent != null) {
                ManageEvents(nEvent);
                nEvent = _networkManager.GetEvent();
            }
        }

        void FixedUpdate() {
            ManageGravity();
            ExecuteInputs();
            ManageSnapshots();
        }

        void ManageEvents(NetworkEvent nEvent) {
            switch(nEvent.Type) {
                case NetworkEvent.EventType.JOIN:
                    ManageJoinEvent(nEvent);
                    break;
                case NetworkEvent.EventType.INPUT:
                    ManageInputEvent(nEvent);
                    break;
                case NetworkEvent.EventType.HIT:
                    ManageHitEvent(nEvent);
                    break;
                case NetworkEvent.EventType.DISCONNECT:
                    ManageDisconnectEvent(nEvent);
                    break;
            }
        }

        void ManageGravity() {
            foreach (var client in _clients) {
                client.ExecuteGravity();
            }
        }

        void ManageSnapshots() {
            _acumTime += Time.deltaTime;
            if(_acumTime >= 1f/_pps) {
                _networkManager.SendSnapshots(_clients);
                _acumTime = 0;
            }
        }

        void ManageJoinEvent(NetworkEvent nEvent) {
            var packet = nEvent.Packet;
            var clientId = _clients.Count;
            ClientData client = new ClientData(clientId, packet.fromEndPoint, Instantiate(playerServerPrefab, new Vector3(0, -25f, 0) , Quaternion.identity));
            _clients.Add(client);
            _networkManager.SendJoinAck(client, _clients);
            _networkManager.SendJoinBroadcast(client, _clients);
        }

        void ManageInputEvent(NetworkEvent nEvent) {
            var packet = nEvent.Packet;
            var id = packet.buffer.GetInt();
            ClientData client = _clients[id];
            client.DeserializeInputs(packet.buffer);
        }

        void ManageHitEvent(NetworkEvent nEvent) {
            var packet = nEvent.Packet;
            var clientId = packet.buffer.GetInt();
            var clientHit = packet.buffer.GetInt();
            int ack = packet.buffer.GetInt();
            foreach(ClientData data in _clients) {
                if(data.id == clientHit) {
                    data.Hit();
                }
            }
            _networkManager.SendHitAck(_clients[clientId], ack);
        }

        void ManageDisconnectEvent(NetworkEvent nEvent) {
            var packet = nEvent.Packet;
            var clientId = packet.buffer.GetInt();
            foreach(ClientData data in _clients) {
                if(data.id == clientId) {
                    Destroy(data.Entity, 1f);
                    _clients.Remove(data);
                }
            }
            _networkManager.SendDisconnectBroadcast(clientId, _clients);
        }

        void ExecuteInputs() {
            foreach(ClientData client in _clients) {
                client.ExecuteInputs();
            }
        }
    }
}
