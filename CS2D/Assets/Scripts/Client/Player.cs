using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class Player {
        public int id;
        private GameObject _entity;
        private CharacterController _controller;
        public CharacterController Controller {
            get { return _controller; }
        }
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public Player(int id, GameObject entity) {
            this.id = id;
            _entity = entity;
            _lastPosition = new Vector3(0, 0, 0);
            _lastRotation = new Quaternion(0, 0, 0, 0);
            _controller = _entity.GetComponent<CharacterController>();
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
            _entity.transform.position = position;
            _entity.transform.rotation = rotation;
        }

        public void UpdateLastPosition() {
            _lastPosition = new Vector3(_entity.transform.position.x, _entity.transform.position.y, _entity.transform.position.z);
            _lastRotation = new Quaternion(_entity.transform.rotation.x, _entity.transform.rotation.y, _entity.transform.rotation.z, _entity.transform.rotation.w);
        }

        public void Interpolate(PlayerNetworkData data, float time) {
            _entity.transform.position = Vector3.Lerp(_lastPosition, data.Position, time);
            _entity.transform.rotation = Quaternion.Lerp(_lastRotation, data.Rotation, time);
        }
    }
}
