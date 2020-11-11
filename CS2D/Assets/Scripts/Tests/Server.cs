using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using System.Linq;

namespace TAVJ {
    public class Server : MonoBehaviour
    {
        private Channel channel;
        private int port = 9000;
        public GameObject playerServerPrefab;
        public List<ClientData> clients;
        public int pps;
        private float acumTime;

        void Awake() {
            channel = new Channel(port);
            clients = new List<ClientData>();
        }

        void Start() {
            pps = 1;
            acumTime = 0f;
        }

        private void OnDestroy() {
            channel.Disconnect();
        }

        /*
        Escucha los eventos
        */
        void Update() {
            var packet = channel.GetPacket();
            while ( packet != null) {
                Event clientEvent = (Event) packet.buffer.GetBits(0, Enum.GetValues(typeof(Event)).Length);
                ManageEvents(clientEvent, packet);
                packet = channel.GetPacket();
            }
            ManageSnapshots();
        }
        void ManageEvents(Event clientEvent, Packet packet) {
            switch(clientEvent) {
                case Event.JOIN:
                    var clientId = clients.Count;
                    Debug.Log("Servidor: Recibo un paquete JOIN y creo un cliente con el id: " + clientId);
                    ClientData client = new ClientData(clientId, packet.fromEndPoint, Instantiate(playerServerPrefab, Vector3.zero, Quaternion.identity));
                    clients.Add(client);
                    SendJoin(client);
                    SendJoinBroadcast(client);
                    break;
                case Event.INPUT:
                    var id = packet.buffer.GetInt();
                    Debug.Log("Servidor: Recibo un evento INPUT del cliente: " + id);
                    Controllers controller = (Controllers) packet.buffer.GetBits(0, Enum.GetValues(typeof(Controllers)).Length);
                    ManageInputs(id, controller);
                    break;
            }
        }

        /*
        Agrega a un paquete:
            - Evento JOIN_BROADCAST
            - Serialize del cliente nuevo (IdCliente posicion rotacion)
        Envia a todos los clientes menos el que se unio el paquete
        */

        void SendJoinBroadcast(ClientData clientData) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.JOIN_BROADCAST, 0, Enum.GetValues(typeof(Event)).Length);
            clientData.Serialize(packet.buffer);
            packet.buffer.Flush();
            foreach (var client in clients) {
                if(client.id != clientData.id) {
                    channel.Send(packet, client.endpoint);
                }
            }
            packet.Free();
        }

        /*
        Agrega a un paquete:
            - Evento JOIN
            - IdCliente creado
            - Cantidad de clientes actuales (nuevo cliente incluido)
            - Serialize de clientes (iDCliente posicion rotacion)
        Envia al cliente el paquete
        */

        void SendJoin(ClientData clientData) {
            var packet = Packet.Obtain();
            packet.buffer.PutBits((int) Event.JOIN, 0, Enum.GetValues(typeof(Event)).Length);
            packet.buffer.PutInt(clientData.id);
            packet.buffer.PutInt(clients.Count);
            foreach (var client in clients) {
                client.Serialize(packet.buffer);
            }
            packet.buffer.Flush();
            channel.Send(packet, clientData.endpoint);
            packet.Free();
        }

        void ManageInputs(int id, Controllers controller) {
            switch(controller) {
                case Controllers.JUMP:
                    Debug.Log("Servidor: Ejecuto el controller Jump del cliente " + id );
                    var player = clients[id];
                    player.entity.GetComponent<Rigidbody>().AddForceAtPosition(Vector3.up * 5, Vector3.zero, ForceMode.Impulse);
                    break;
            }
        }

        void ManageSnapshots() {
            acumTime += Time.deltaTime;
            if(acumTime >= 1f/pps) {
                var clientsMoving = clients.Where(elem => elem.IsMoving()).ToList();
                if(clientsMoving.Count > 0) {
                    var packet = Packet.Obtain();
                    packet.buffer.PutBits((int) Event.SNAPSHOT, 0, Enum.GetValues(typeof(Event)).Length);
                    packet.buffer.PutInt(clientsMoving.Count);
                    foreach (var client in clientsMoving) {
                        client.Serialize(packet.buffer);
                    }
                    packet.buffer.Flush();
                    foreach (var client in clients) {
                        channel.Send(packet, client.endpoint);
                    }
                    packet.Free();
                }
                acumTime = 0;
            }
        }
    }
}
