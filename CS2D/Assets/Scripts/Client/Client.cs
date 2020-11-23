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

        void Awake() {
            _players = new Dictionary<int, Player>();
            _playerClientPrefab = Resources.Load<GameObject>("PlayerClient");
            _networkManager = new NetworkManager(serverIp, clientPort, serverPort);
            _snapshotManager = new SnapshotManager();
            _inputManager = new newInputManager();
        }

        void Start() {
            _networkManager.SendJoin();
        }

        private void OnDestroy() {
            _networkManager.Disconnect();
        }

        void Update() {
            Event event = _networkManager.GetEvent();
            while (event != null) {
                ManageEvent(event);
                event = _networkManager.GetEvent();
            }
            ManageInputs();
            _snapshotManager.Interpolate(_players);
        }

        private void ManageEvent(Event event) {
            switch(event.Type) {
                case Event.Type.JOIN:
                    ManageJoinEvent(event);
                    break;
                case Event.JOIN_BROADCAST:
                    manageJoinBroadcastEvent(event);
                    break;
                case Event.SNAPSHOT:
                    manageSnapshotEvent(event);
                    break;
            }
        }

        private void ManageJoinEvent(Event event) {
            BitBuffer buffer = event.Packet.buffer;
            _clientId = buffer.GetInt();
            var playersLength = buffer.GetInt();
            for(var i = 0; i < playersLength; i++) {
                var clientId = buffer.GetInt();
                var player = new Player(clientId, Instantiate(playerClientPrefab, Vector3.zero, Quaternion.identity));
                _players.Add(player.id, player);
                player.Deserialize(buffer);
            }
        }

        void manageJoinBroadcastEvent(Event event) {
            Bitbuffer buffer = event.Packet.buffer;
            var newClientId = buffer.GetInt();
            var newPlayer = new Player(newClientId, Instantiate(playerClientPrefab, Vector3.zero, Quaternion.identity));
            _players.Add(newPlayer.id, newPlayer);
            newPlayer.Deserialize(buffer);
        }

        void manageSnapshotEvent(Event event) {
            _snapshotManager.AddSnapshot(buffer);
            _snapshotManager.UpdatePositionsBySnapshots(players);
        }

        void ManageInputs() {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            _inputManager.Add(horizontal, vertical);
        }
    }
}
