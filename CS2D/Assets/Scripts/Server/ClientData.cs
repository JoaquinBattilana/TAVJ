using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class ClientData {
        public int id;
        public IPEndPoint endpoint;
        public GameObject entity;

        public ClientData(int id, IPEndPoint endpoint, GameObject playerInstance) {
            this.endpoint = endpoint;
            this.id = id;
            entity = playerInstance;
        }

        public void Serialize(BitBuffer buffer) {
            var position = entity.transform.position;
            var rotation = entity.transform.rotation;
            buffer.PutInt(id);
            buffer.PutFloat(position.x);
            buffer.PutFloat(position.y);
            buffer.PutFloat(position.z);
            buffer.PutFloat(rotation.x);
            buffer.PutFloat(rotation.y);
            buffer.PutFloat(rotation.z);
            buffer.PutFloat(rotation.w);
        }

        public bool IsMoving() {
            var isMoving = this.entity.transform.hasChanged;
            this.entity.transform.hasChanged = false;
            return isMoving;
        }
    }
}
