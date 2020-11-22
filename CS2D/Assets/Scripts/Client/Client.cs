using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TAVJ {
    public class Client : MonoBehaviour
    {
        private Channel channel;
        public int clientPort = 9001;
        private int serverPort = 9000;
        private string serverIp = "127.0.0.1";
        public GameObject playerClientPrefab;
        public Dictionary<int, Player> players;
        public int clientId = -1;
        public KeyCode jump;
        public KeyCode moveForward;
        public KeyCode moveRight;
        public KeyCode moveLeft;
        public KeyCode moveBackward;
        public Queue<Snapshot> snapshotQueue;
        public int snapshotQueueSize = 3;
        public float acumTime;
        public float pps;

        void Awake() {
            channel = new Channel(serverIp, clientPort, serverPort);
            players = new Dictionary<int, Player>();
            playerClientPrefab = Resources.Load<GameObject>("PlayerClient");
            snapshotQueue = new Queue<Snapshot>();
        }

        /*
        Cuando se crea manda un JOIN al servidor
        */

        void Start() {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.JOIN, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.Flush();
            channel.Send(packet);
            Debug.Log("Cliente Nuevo: Mando un paquete JOIN");
            acumTime = 0f;
            pps = 30f;
        }

        private void OnDestroy() {
            channel.Disconnect();
        }

        /*
        Escucha los eventos
        Escucha los inputs
        */

        void Update() {
            acumTime += Time.deltaTime;
            var packet = channel.GetPacket();
            while (packet != null) {
                Event clientEvent = (Event) packet.buffer.GetBits(0, Enum.GetValues(typeof(Event)).Length);
                ManageServerEvents(clientEvent, packet.buffer);
                packet = channel.GetPacket();
            }
            ManageInputs();
            Interpolate();
        }

        /*
        Maneja los eventos
            JOIN
                -Saca el clientId
                -Saca la cantidad de players
                -Va sacando dependiendo la cantidad de players
                    -Saca clientId
                    -Crea el player
                    -Saca la posicion y rotacion
        */

        void ManageServerEvents(Event clientEvent, BitBuffer buffer) {
            switch(clientEvent) {
                case Event.JOIN:
                    manageJoinEvent(buffer);
                    break;
                case Event.JOIN_BROADCAST:
                    manageJoinBroadcastEvent(buffer);
                    break;
                case Event.SNAPSHOT:
                    manageSnapshotEvent(buffer);
                    break;
            }
        }

        void ManageInputs() {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.INPUT, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.PutInt(this.clientId);
            if (Input.GetKeyDown(jump)) {
            }
            if (Input.GetKeyDown(moveForward)) {
                ManageSendInput(Controllers.MOVE_FORWARD);
            }
            if (Input.GetKeyDown(moveBackward)) {
                ManageSendInput(Controllers.MOVE_BACKWARD);
            }
            if (Input.GetKeyDown(moveRight)) {
                ManageSendInput(Controllers.MOVE_RIGHT);
            }
            if (Input.GetKeyDown(moveLeft)) {
                ManageSendInput(Controllers.MOVE_LEFT);
            }
        }

        void ManageSendInput(Controllers key) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.INPUT, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.PutInt(this.clientId);
            packet.buffer.PutBits((int) key, 0, Enum.GetValues(typeof(Controllers)).Length);
            packet.buffer.Flush();
            channel.Send(packet);
            packet.Free();
        }

        void manageJoinEvent(BitBuffer buffer) {
            this.clientId = buffer.GetInt();
            var playersLength = buffer.GetInt();
            Debug.Log("Cliente " + this.clientId + ": Recibo el evento JOIN, hay " + playersLength + " clientes en el servidor");
            for(var i = 0; i < playersLength; i++) {
                var clientId = buffer.GetInt();
                var player = new Player(clientId, Instantiate(playerClientPrefab, Vector3.zero, Quaternion.identity));
                Debug.Log("Cliente " + this.clientId + ": creo el cliente id " + clientId);
                players.Add(player.id, player);
                player.Deserialize(buffer);
            }
        }

        void manageJoinBroadcastEvent(BitBuffer buffer) {
            var newClientId = buffer.GetInt();
            Debug.Log("Cliente " + this.clientId + ": Recibo el evento JOIN_BROADCAST y creo el cliente " + newClientId);
            var newPlayer = new Player(newClientId, Instantiate(playerClientPrefab, Vector3.zero, Quaternion.identity));
            players.Add(newPlayer.id, newPlayer);
            newPlayer.Deserialize(buffer);
        }

        void manageSnapshotEvent(BitBuffer buffer) {
            Snapshot sp = new Snapshot(buffer);
            snapshotQueue.Enqueue(sp);
            acumTime = 0;
            if (snapshotQueue.Count >= snapshotQueueSize) {
                sp = snapshotQueue.Dequeue();
                var keys = sp.GetClientsIds();
                foreach(var key in keys) {
                    if(players.ContainsKey(key)) {
                        PlayerNetworkData data = sp.GetClient(key);
                        Player p = players[key];
                        p.UpdatePosition(data);
                    }
                }
            }
        }

        void Interpolate() {
            if(snapshotQueue.Count >= snapshotQueueSize) {
                Snapshot nextSnapshot = snapshotQueue.Peek();
                foreach(var playerId in nextSnapshot.GetClientsIds()) {
                    if(players.ContainsKey(playerId)) {
                        players[playerId].Interpolate(nextSnapshot.GetClient(playerId), acumTime/(1f/pps));
                    }
                }
            }
        }
    }
}
