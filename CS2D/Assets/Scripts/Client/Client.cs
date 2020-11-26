using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace TAVJ {
    public class Client : MonoBehaviour {
        public GameObject playerClientPrefab;
        public GameObject otherPlayerClientPrefab;
        public GameObject dummyPrefab;
        public GameObject dummy;
        public GameObject dummyHead;
        public string clientDestIp = "127.0.0.1";
        public int clientSourcePort;
        public int clientDestPort = 9000;
        public int _clientId = -1;
        private Dictionary<int, Player> _players;
        private NetworkManager _networkManager;
        private SnapshotManager _snapshotManager;
        private InputManager _inputManager;
        private float _acumTime = 0;
        private bool _menuOpen = false;

        void Awake() {
            _players = new Dictionary<int, Player>();
            _snapshotManager = new SnapshotManager();
            _inputManager = new InputManager();
            if(PlayerPrefs.HasKey("clientSourcePort") && PlayerPrefs.HasKey("clientDestIp") && PlayerPrefs.HasKey("clientDestPort")) {
                clientSourcePort = PlayerPrefs.GetInt("clientSourcePort");
                clientDestPort = PlayerPrefs.GetInt("clientDestPort");
                clientDestIp = PlayerPrefs.GetString("clientDestIp");
            }
            PlayerPrefs.DeleteAll();
            _networkManager = new NetworkManager(clientDestIp, clientSourcePort, clientDestPort);
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
            if(Input.GetKeyDown(KeyCode.Escape)) {
                _menuOpen = !_menuOpen;
                if(!_menuOpen) {
                    Cursor.lockState = CursorLockMode.None;
                } else {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            if(Input.GetKeyDown(KeyCode.Mouse0)){
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
        }

        void FixedUpdate() {
            _acumTime += Time.deltaTime;
            if(_clientId != -1) {
                ManageInputs();
                _snapshotManager.Interpolate(_players, _acumTime, _clientId);
                Snapshot currentSnapshot = _snapshotManager.CurrentSnapshot;
                if (currentSnapshot != null && !currentSnapshot.IsEmpty() && currentSnapshot.GetClient(_clientId) != null) {
                    PlayerNetworkData meInSnapshot = currentSnapshot.GetClient(_clientId);
                    _inputManager.RemoveAckInputs(meInSnapshot.MostBigInput);
                    dummy.transform.position = meInSnapshot.Position;
                    dummy.transform.rotation = meInSnapshot.Rotation;
                    dummyHead.transform.rotation = meInSnapshot.Rotation;
                    _inputManager.ExecuteConciliateInputs(dummy);
                    _players[_clientId].Conciliate(dummy);
                }
            }
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
            }
        }

        private void ManageJoinEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            _clientId = buffer.GetInt();
            var playersLength = buffer.GetInt();
            Debug.Log(playersLength);
            for(var i = 0; i < playersLength; i++) {
                var clientId = buffer.GetInt();
                Player player;
                if(clientId == _clientId) {
                    player = new Player(clientId, Instantiate(playerClientPrefab, Vector3.zero, Quaternion.identity));
                } else {
                    player = new Player(clientId, Instantiate(otherPlayerClientPrefab, Vector3.zero, Quaternion.identity));
                }
                _players.Add(player.id, player);
                player.Deserialize(buffer);
            }
            dummy = Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity);
            dummyHead = dummy.FindInChildren("RigSpine1");
        }

        void manageJoinBroadcastEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            var newClientId = buffer.GetInt();
            var newPlayer = new Player(newClientId, Instantiate(otherPlayerClientPrefab, Vector3.zero, Quaternion.identity));
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
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            Player me = _players[_clientId];
            if( horizontal != 0 || vertical != 0 || mouseX != 0 || mouseY != 0) {
                _inputManager.Add(horizontal, vertical, mouseX, mouseY);
                _inputManager.ExecuteLast(me.Entity);
            }
            if(_inputManager.Inputs.Count > 0) {
                _networkManager.SendInputs(_clientId, _inputManager);
            }
            me.PredictGravity();
            dummy.GetComponent<CharacterController>().Move(Physics.gravity * Time.deltaTime);
        }
    }
}
