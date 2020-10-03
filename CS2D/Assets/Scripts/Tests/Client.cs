using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAVJ {
    public class Client : MonoBehaviour
    {
        private Channel channel;
        public int clientPort = 9001;
        private int serverPort = 9000;
        private string serverIp = "127.0.0.1";

        void Awake() {
            channel = new Channel(serverIp, clientPort, serverPort);
        }

        void Start() {
            var packet = Packet.Obtain();
            packet.buffer.PutInt((int) Event.JOIN);
            packet.buffer.Flush();
            channel.Send(packet);
        }

        private void OnDestroy() {
            channel.Disconnect();
        }

        // Update is called once per frame
        void Update() {
            
        }
    }
}

