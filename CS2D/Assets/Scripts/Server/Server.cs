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
            pps = 30;
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
            ManageGravity();
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
            var player = clients[id];
            var playerController = player.entity.GetComponent<CharacterController>();
            switch(controller) {
                case Controllers.MOVE_FORWARD:
                    playerController.Move(Vector3.up * 5);
                    break;
                case Controllers.MOVE_BACKWARD:
                    playerController.Move(Vector3.up * 5);
                    break;
                case Controllers.MOVE_RIGHT:
                    playerController.Move(Vector3.up * 5);
                    break;
                case Controllers.MOVE_LEFT:
                    playerController.Move(Vector3.up * 5);
                    break;
            }
        }

        void ManageGravity() {
            foreach (var client in clients) {
                var controller = client.entity.GetComponent<CharacterController>();
                if(controller.isGrounded == false) {
                    controller.Move(Physics.gravity * Time.deltaTime);
                }
            }
        }

        void ManageSnapshots() {
            acumTime += Time.deltaTime;
            if(acumTime >= 1f/pps) {
                var packet = Packet.Obtain();
                packet.buffer.PutBits((int) Event.SNAPSHOT, 0, Enum.GetValues(typeof(Event)).Length);
                var clientsMoving = clients.Where(elem => elem.IsMoving()).ToList();
                packet.buffer.PutInt(clientsMoving.Count);
                if(clientsMoving.Count > 0) {
                    foreach (var client in clientsMoving) {
                        client.Serialize(packet.buffer);
                    }
                } else {
                    packet.buffer.PutInt(0);
                }
                packet.buffer.Flush();
                foreach (var client in clients) {
                    channel.Send(packet, client.endpoint);
                }
                packet.Free();
                acumTime = 0;
            }
        }
    }
}
