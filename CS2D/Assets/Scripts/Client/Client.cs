using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TAVJ {
    public class Client : MonoBehaviour {
        private GameObject _playerClientPrefab;
        public int clientPort = 9001;
        private int serverPort = 9000;
        private string serverIp = "127.0.0.1";
        public int _clientId = -1;
        private Dictionary<int, Player> _players;
        private NetworkManager _networkManager;
        private SnapshotManager _snapshotManager;
        private InputManager _inputManager;
        private float _acumTime = 0;

        void Awake() {
            _players = new Dictionary<int, Player>();
            _playerClientPrefab = Resources.Load<GameObject>("PlayerClient");
            _networkManager = new NetworkManager(serverIp, clientPort, serverPort);
            _snapshotManager = new SnapshotManager();
            _inputManager = new InputManager();
        }

        void Start() {
            _networkManager.SendJoin();
        }

        void OnDestroy() {
            _networkManager.Disconnect();
        }

        void Update() {
            NetworkEvent nEvent = _networkManager.GetEvent();
            while (nEvent != null) {
                ManageEvent(nEvent);
                nEvent = _networkManager.GetEvent();
            }
        }

        void FixedUpdate() {
            _acumTime += Time.deltaTime;
            ManageInputs();
            _snapshotManager.Interpolate(_players, _acumTime, _clientId);
        }

        private void ManageEvent(NetworkEvent nEvent) {
            switch(nEvent.Type) {
                case NetworkEvent.EventType.JOIN:
                    ManageJoinEvent(nEvent);
                    break;
                case NetworkEvent.EventType.JOIN_BROADCAST:
                    manageJoinBroadcastEvent(nEvent);
                    break;
                case NetworkEvent.EventType.SNAPSHOT:
                    manageSnapshotEvent(nEvent);
                    break;
                case NetworkEvent.EventType.INPUT_ACK:
                    ManageInputAck(nEvent);
                    break;
            }
        }

        private void ManageJoinEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            _clientId = buffer.GetInt();
            var playersLength = buffer.GetInt();
            for(var i = 0; i < playersLength; i++) {
                var clientId = buffer.GetInt();
                var player = new Player(clientId, Instantiate(_playerClientPrefab, Vector3.zero, Quaternion.identity));
                _players.Add(player.id, player);
                player.Deserialize(buffer);
            }
        }

        void manageJoinBroadcastEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            var newClientId = buffer.GetInt();
            var newPlayer = new Player(newClientId, Instantiate(_playerClientPrefab, Vector3.zero, Quaternion.identity));
            _players.Add(newPlayer.id, newPlayer);
            newPlayer.Deserialize(buffer);
        }

        void manageSnapshotEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            _snapshotManager.AddSnapshot(buffer);
            foreach(KeyValuePair<int, Player> entry in _players) {
                entry.Value.UpdateLastPosition();
            }
            _acumTime = 0;
        }

        void ManageInputs() {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            _inputManager.Add(horizontal, vertical);
            if(_inputManager.Inputs.Count > 0) {
                _networkManager.SendInputs(_clientId, _inputManager);
            }
        }

        void ManageInputAck(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            _inputManager.RemoveAckInputs(buffer);
        }
    }
}
