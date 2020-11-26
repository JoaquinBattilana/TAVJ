using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAVJ {
    public class ClientInput {
        private float _horizontal;
        private float _vertical;
        private float _mouseX;
        private float _mouseY;
        private int _number;
        public int Number {
            get { return _number; }
        }

        public ClientInput(int number, float horizontal, float vertical, float mouseX, float mouseY) {
            _number = number;
            _horizontal = horizontal;
            _vertical = vertical;
            _mouseX = mouseX;
            _mouseY = mouseY;
        }

        public ClientInput(BitBuffer buffer) {
            _number = buffer.GetInt();
            _horizontal = buffer.GetFloat();
            _vertical = buffer.GetFloat();
            _mouseX = buffer.GetFloat();
            _mouseY = buffer.GetFloat();
        }

        public void Serialize(BitBuffer buffer) {
            buffer.PutInt(_number);
            buffer.PutFloat(_horizontal);
            buffer.PutFloat(_vertical);
            buffer.PutFloat(_mouseX);
            buffer.PutFloat(_mouseY);
        }

        public override string ToString() {
            return string.Format("input = {0}, horizontal = {1}, vertical = {2}, mouseX = {3}. mouseY = {4}", _number, _horizontal, _vertical, _mouseX, _mouseY);
        }

        public void Execute(GameObject entity) {
            GameObject head = entity.FindInChildren("RigSpine1");
            CharacterController controller = entity.GetComponent<CharacterController>();
            Vector3 movement = Vector3.zero;
            movement += (entity.transform.forward.normalized * _vertical);
            movement += (entity.transform.right.normalized * _horizontal);
            controller.Move(movement * 2f * Time.deltaTime);
            entity.transform.Rotate(0, _mouseX * 2f, 0);
            //head.transform.Rotate(_mouseY * 2f, 0, 0);
        }
    }
}
