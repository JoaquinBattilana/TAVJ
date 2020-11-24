using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace TAVJ {
    public class ClientData {
        public int id;
        public IPEndPoint endpoint;
        private GameObject _entity;
        private CharacterController _controller;
        private InputManager _inputManager;
        public InputManager InputManager {
            get { return _inputManager; }
        }

        public ClientData(int id, IPEndPoint endpoint, GameObject playerInstance) {
            this.endpoint = endpoint;
            this.id = id;
            _entity = playerInstance;
            _controller = _entity.GetComponent<CharacterController>();
            _inputManager = new InputManager();
        }

        public void SerializePosition(BitBuffer buffer) {
            var position = _entity.transform.position;
            var rotation = _entity.transform.rotation;
            buffer.PutInt(id);
            buffer.PutFloat(position.x);
            buffer.PutFloat(position.y);
            buffer.PutFloat(position.z);
            buffer.PutFloat(rotation.x);
            buffer.PutFloat(rotation.y);
            buffer.PutFloat(rotation.z);
            buffer.PutFloat(rotation.w);
        }

        public void DeserializeInputs(BitBuffer buffer) {
            _inputManager.Deserialize(buffer);
        }

        public void ExecuteInputs() {
            _inputManager.ExecuteInputs(_controller);
        }

        public void ExecuteGravity() {
            if(_controller.isGrounded == false) {
                _controller.Move(Physics.gravity * Time.deltaTime);
            }
        }

        public bool IsMoving() {
            var isMoving = _entity.transform.hasChanged;
            _entity.transform.hasChanged = false;
            return isMoving;
        }
    }
}
