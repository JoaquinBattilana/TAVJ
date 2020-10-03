using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class Server : MonoBehaviour
    {
        private Channel channel;
        private int port = 9000;
        List<ClientData> clients;

        void Awake() {
            channel = new Channel(port);
            clients = new List<ClientData>();
        }

        void Start() {
        }

        private void OnDestroy() {
            channel.Disconnect();
        }

        void Update() {
            var packet = channel.GetPacket();
            while ( packet != null) {
                Event clientEvent = (Event) packet.buffer.GetInt();
                ManageEvents(clientEvent, packet.fromEndPoint);
                packet = channel.GetPacket();
            }
        }
        void ManageEvents(Event clientEvent, IPEndPoint fromEndPoint) {
            switch(clientEvent) {
                case Event.JOIN:
                    clients.Add(new ClientData(clients.Count, fromEndPoint));
                    break;
            }
        }
    }
}
