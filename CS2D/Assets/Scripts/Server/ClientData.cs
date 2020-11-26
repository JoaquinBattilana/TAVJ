using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class ClientData {
        public int id;
        public IPEndPoint endpoint;
        private GameObject _entity;
        public GameObject Entity {
            get { return _entity; }
        }
        private CharacterController _controller;
        private GameObject _head;
        private InputManager _inputManager;
        public InputManager InputManager {
            get { return _inputManager; }
        }
        private int _health;
        private int _points;
        public int Health {
            get { return _health; }
        }

        public ClientData(int id, IPEndPoint endpoint, GameObject playerInstance) {
            this.endpoint = endpoint;
            this.id = id;
            _entity = playerInstance;
            _controller = _entity.GetComponent<CharacterController>();
            _inputManager = new InputManager();
            _health = 100;
            _points = 0;
            _head = _entity.FindInChildren("RigSpine1");
        }

        public void Serialize(BitBuffer buffer) {
            var position = _entity.transform.position;
            var rotation = _entity.transform.rotation;
            var headRotation = _head.transform.rotation;
            buffer.PutInt(id);
            buffer.PutInt(_inputManager.MostBigInput);
            buffer.PutInt(_health);
            buffer.PutInt(_points);
            buffer.PutFloat(position.x);
            buffer.PutFloat(position.y);
            buffer.PutFloat(position.z);
            buffer.PutFloat(rotation.x);
            buffer.PutFloat(rotation.y);
            buffer.PutFloat(rotation.z);
            buffer.PutFloat(rotation.w);
            buffer.PutFloat(headRotation.x);
            buffer.PutFloat(headRotation.y);
            buffer.PutFloat(headRotation.z);
            buffer.PutFloat(headRotation.w);
        }

        public void DeserializeInputs(BitBuffer buffer) {
            _inputManager.Deserialize(buffer);
        }

        public void ExecuteInputs() {
            _inputManager.ExecuteInputs(_entity);
        }

        public void ExecuteGravity() {
            if(_controller.isGrounded == false) {
                _controller.Move(Physics.gravity * Time.deltaTime);
            }
        }

        public bool IsMoving() {
            var isMoving = _entity.transform.hasChanged;
            var isHeadMoving = _head.transform.hasChanged;
            _entity.transform.hasChanged = false;
            _head.transform.hasChanged = false;
            return isMoving && isHeadMoving;
        }

        public override string ToString() {
            return "" + id;
        }

        public void Hit() {
            _health -= 20;
            Debug.Log(_health);
        }
    }
}
