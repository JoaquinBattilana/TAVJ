using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class Player {
        public int id;
        private GameObject _entity;
        public GameObject Entity {
            get { return _entity; }
        }
        private CharacterController _controller;
        public CharacterController Controller {
            get { return _controller; }
        }
        private GameObject _head;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Quaternion _lastHeadRotation;
        private int _health;
        public int Health {
            get { return _health; }
        }
        private int _mostBigInput;

        public Player(int id, GameObject entity) {
            this.id = id;
            _entity = entity;
            _health = 100;
            _lastPosition = new Vector3(0, 0, 0);
            _lastRotation = new Quaternion(0, 0, 0, 0);
            _lastHeadRotation = new Quaternion(0, 0, 0, 0);
            _controller = _entity.GetComponent<CharacterController>();
            _head = _entity.FindInChildren("RigSpine1");
        }

        public void Deserialize(BitBuffer buffer) {
            var position = new Vector3();
            var rotation = new Quaternion();
            var headRotation = new Quaternion();
            _mostBigInput = buffer.GetInt();
            _health = buffer.GetInt();
            position.x = buffer.GetFloat();
            position.y = buffer.GetFloat();
            position.z = buffer.GetFloat();
            rotation.x = buffer.GetFloat();
            rotation.y = buffer.GetFloat();
            rotation.z = buffer.GetFloat();
            rotation.w = buffer.GetFloat();
            headRotation.x = buffer.GetFloat();
            headRotation.y = buffer.GetFloat();
            headRotation.z = buffer.GetFloat();
            headRotation.w = buffer.GetFloat();
            _entity.transform.position = position;
            _entity.transform.rotation = rotation;
            _head.transform.rotation = headRotation;
        }

        public void PredictGravity() {
            _controller.Move(Physics.gravity * Time.deltaTime);
        }

        public void Conciliate(GameObject dummy) {
            GameObject dummyHead = dummy.FindInChildren("RigSpine1").gameObject;
            _entity.transform.position = dummy.transform.position;
            _entity.transform.rotation = dummy.transform.rotation;
            _head.transform.rotation = dummyHead.transform.rotation;
        }

        public void UpdateLastPosition() {
            _lastPosition = new Vector3(_entity.transform.position.x, _entity.transform.position.y, _entity.transform.position.z);
            _lastRotation = new Quaternion(_entity.transform.rotation.x, _entity.transform.rotation.y, _entity.transform.rotation.z, _entity.transform.rotation.w);
            _lastHeadRotation = new Quaternion(_head.transform.rotation.x, _head.transform.rotation.y, _head.transform.rotation.z, _head.transform.rotation.w);
        }

        public void Interpolate(PlayerNetworkData data, float time) {
            _entity.transform.position = Vector3.Lerp(_lastPosition, data.Position, time);
            _entity.transform.rotation = Quaternion.Lerp(_lastRotation, data.Rotation, time);
            _head.transform.rotation = Quaternion.Lerp(_lastHeadRotation, data.HeadRotation, time);
        }
    }
}
