using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class ClientData {
        int id;
        IPEndPoint endpoint;

        public ClientData(int id, IPEndPoint endpoint) {
            this.endpoint = endpoint;
        }
    }
}
