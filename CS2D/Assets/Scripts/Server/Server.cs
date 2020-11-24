using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using System.Linq;

namespace TAVJ {
    public class Server : MonoBehaviour
    {
        private int _port = 9000;
        private GameObject _playerServerPrefab;
        private List<ClientData> _clients;
        private int _pps;
        private float _acumTime;
        private NetworkManager _networkManager;

        void Awake() {
            _networkManager = new NetworkManager(_port);
            _clients = new List<ClientData>();
            _playerServerPrefab = Resources.Load<GameObject>("PlayerServer");
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
            ManageSnapshots();
        }

        void FixedUpdate() {
            ManageGravity();
        }

        void ManageEvents(NetworkEvent nEvent) {
            switch(nEvent.Type) {
                case NetworkEvent.EventType.JOIN:
                    ManageJoinEvent(nEvent);
                    break;
                case NetworkEvent.EventType.INPUT:
                    ManageInputEvent(nEvent);
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
            ClientData client = new ClientData(clientId, packet.fromEndPoint, Instantiate(_playerServerPrefab, Vector3.zero, Quaternion.identity));
            _clients.Add(client);
            _networkManager.SendJoinAck(client, _clients);
            _networkManager.SendJoinBroadcast(client, _clients);
        }

        void ManageInputEvent(NetworkEvent nEvent) {
            var packet = nEvent.Packet;
            var id = packet.buffer.GetInt();
            ClientData client = _clients[id];
            client.DeserializeInputs(packet.buffer);
            client.ExecuteInputs();
            _networkManager.SendInputAck(client);
        }
    }
}
