using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

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
        public Camera camera;
        private UiManager _uiManager;
        private Actions _actions;
        private PlayerController _playerController;

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
            _networkManager = new NetworkManager(clientDestIp, clientSourcePort, clientDestPort, this);
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
                _actions.Attack();
                RaycastHit hit;
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    if(hit.transform.gameObject.tag == "Player") {
                        int clientHit = hit.transform.gameObject.GetComponent<DummyScript>().id;
                        _networkManager.SendHit(_clientId, clientHit);
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Keypad9)) {
                _networkManager.LatencyUp();
            }
            if(Input.GetKeyDown(KeyCode.Keypad6)) {
                _networkManager.LatencyDown();
            }
            if(Input.GetKeyDown(KeyCode.P)) {
                _networkManager.DisconnectFromServer(_clientId);
                Destroy(_players[_clientId].Entity, 1f);
                Destroy(dummy);
                SceneManager.LoadScene("MainMenu");
            }
            _networkManager.TimeHasPass(Time.deltaTime);
            _networkManager.ResendNotAckPackets();
        }

        void FixedUpdate() {
            _acumTime += Time.deltaTime;
            if(_clientId != -1) {
                ManageInputs();
                _snapshotManager.Interpolate(_players, _acumTime, _clientId);
                Snapshot currentSnapshot = _snapshotManager.CurrentSnapshot;
                if (currentSnapshot != null && !currentSnapshot.IsEmpty() && currentSnapshot.GetClient(_clientId) != null) {
                    PlayerNetworkData meInSnapshot = currentSnapshot.GetClient(_clientId);
                    _uiManager.SetData(meInSnapshot);
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
                case NetworkEvent.EventType.HIT_ACK:
                    ManageHitAckEvent(nEvent);
                    break;
                case NetworkEvent.EventType.DISCONNECT_BROADCAST:
                    ManageDisconnectBroadcast(nEvent);
                    break;
            }
        }

        private void ManageDisconnectBroadcast(NetworkEvent nEvent) {
            int clientId = nEvent.Packet.buffer.GetInt();
            if(_players.ContainsKey(clientId)) {
                Destroy(_players[clientId].Entity, 1f);
                _players.Remove(clientId);
            }
            nEvent.Packet.Free();
        }

        private void ManageHitAckEvent(NetworkEvent nEvent) {
            int maxAck = nEvent.Packet.buffer.GetInt();
            _networkManager.DeleteAckPackets(maxAck);
            nEvent.Packet.Free();
        }

        private void ManageJoinEvent(NetworkEvent nEvent) {
            BitBuffer buffer = nEvent.Packet.buffer;
            _clientId = buffer.GetInt();
            var playersLength = buffer.GetInt();
            for(var i = 0; i < playersLength; i++) {
                var clientId = buffer.GetInt();
                Player player;
                if(clientId == _clientId) {
                    GameObject go = Instantiate(playerClientPrefab, new Vector3(0f, -25f, 0f), Quaternion.identity);
                    player = new Player(clientId, go);
                    camera = go.transform.Find("Camera").gameObject.GetComponent<Camera>();
                    _uiManager = new UiManager(player, go.transform.Find("Ui").gameObject);
                    _actions = go.GetComponent<Actions>();
                    _playerController = go.GetComponent<PlayerController>();
                    _playerController.SetArsenal("Rifle");
                    _actions.Aiming();
                } else {
                    GameObject go = Instantiate(otherPlayerClientPrefab, new Vector3(0f, -25f, 0f), Quaternion.identity);
                    go.GetComponent<DummyScript>().id = clientId;
                    player = new Player(clientId, go);
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
                _actions.Walk();
                _inputManager.ExecuteLast(me.Entity);
            } else {
                _actions.Stay();
            }
            if(_inputManager.Inputs.Count > 0) {
                _networkManager.SendInputs(_clientId, _inputManager);
            }
            me.PredictGravity();
            dummy.GetComponent<CharacterController>().Move(Physics.gravity * Time.deltaTime);
        }
    }
}
