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

        void Awake() {
            channel = new Channel(serverIp, clientPort, serverPort);
            players = new Dictionary<int, Player>();
            playerClientPrefab = Resources.Load<GameObject>("PlayerClient");
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
        }

        private void OnDestroy() {
            channel.Disconnect();
        }

        /*
        Escucha los eventos
        Escucha los inputs
        */

        void Update() {
            var packet = channel.GetPacket();
            while (packet != null) {
                Event clientEvent = (Event) packet.buffer.GetBits(0, Enum.GetValues(typeof(Event)).Length);
                ManageServerEvents(clientEvent, packet.buffer);
                packet = channel.GetPacket();
            }
            if (Input.GetKeyDown(jump)) {
                var sendPacket = Packet.Obtain();
                sendPacket.buffer.PutBits((int) Event.INPUT, 0, Enum.GetValues(typeof(Event)).Length);
                sendPacket.buffer.PutInt(this.clientId);
                sendPacket.buffer.PutBits((int) Controllers.JUMP, 0, Enum.GetValues(typeof(Controllers)).Length);
                sendPacket.buffer.Flush();
                channel.Send(sendPacket);
            }
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
            var playersLengths = buffer.GetInt();
            Debug.Log("Jugadores moviendose: " + playersLengths);
            Debug.Log("Cliente " + this.clientId + ": Recibo el evento SNAPSHOT");
            for(var i = 0; i < playersLengths; i++) {
                var player = players[buffer.GetInt()];
                player.Deserialize(buffer);
            }
        }
    }
}
