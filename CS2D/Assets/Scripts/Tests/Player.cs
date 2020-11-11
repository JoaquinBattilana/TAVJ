using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class Player {
        public int id;
        public GameObject entity;

        public Player(int id, GameObject entity) {
            this.id = id;
            this.entity = entity;
        }

        public void Deserialize(BitBuffer buffer) {
            var position = new Vector3();
            var rotation = new Quaternion();
            position.x = buffer.GetFloat();
            position.y = buffer.GetFloat();
            position.z = buffer.GetFloat();
            rotation.x = buffer.GetFloat();
            rotation.y = buffer.GetFloat();
            rotation.z = buffer.GetFloat();
            rotation.w = buffer.GetFloat();
            entity.transform.position = position;
            entity.transform.rotation = rotation;
        }
    }
}
