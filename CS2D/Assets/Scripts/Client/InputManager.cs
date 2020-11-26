using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TAVJ {
    public class InputManager {
        private List<ClientInput> _inputs;
        public List<ClientInput> Inputs {
            get { return _inputs; }
        }

        private int _mostBigInput = 0;
        public int MostBigInput {
            get { return _mostBigInput; }
        }

        private int _inputUidGenerator = 0;

        public InputManager() {
            _inputs = new List<ClientInput>();
        }

        public void Deserialize(BitBuffer buffer) {
            var inputsQuantity = buffer.GetInt();
            for(int i = 0; i < inputsQuantity; i++) {
                ClientInput input = new ClientInput(buffer);
                if(input.Number > _mostBigInput) {
                    _inputs.Add(input);
                }
            }
        }

        public void Add(float horizontal, float vertical, float mouseX, float mouseY) {
            ClientInput input = new ClientInput(_inputUidGenerator, horizontal, vertical, mouseX, mouseY);
            _inputUidGenerator++;
            _inputs.Add(input);
        }

        public void PredictLastInput(Player player) {
            _inputs[_inputs.Count-1].Execute(player.Entity);
        }

        public void Serialize(BitBuffer buffer) {
            foreach(ClientInput input in _inputs) {
                input.Serialize(buffer);
            }
        }

        public void ExecuteInputs(GameObject entity) {
            foreach(ClientInput input in _inputs) {
                input.Execute(entity);
                if(input.Number > _mostBigInput) {
                    _mostBigInput = input.Number;
                }
            }
            _inputs.Clear();
        }

        public void ExecuteConciliateInputs(GameObject dummy) {
            foreach(ClientInput input in _inputs) {
                input.Execute(dummy);
                dummy.GetComponent<CharacterController>().Move(Physics.gravity * Time.deltaTime);
            }
        }

        public void ExecuteLast(GameObject entity) {
            _inputs[_inputs.Count-1].Execute(entity);
        }

        public void RemoveAckInputs(int mostBigInput) {
            _mostBigInput = mostBigInput;
            _inputs = _inputs.Where(input => input.Number > _mostBigInput).ToList();
        }

        public enum Type {
            MOVEMENT = 0
        }

        public override string ToString() {
            string str = string.Format("Input más grande: {0}", _mostBigInput);
            str += string.Format("Cantidad de inputs = {0}", _inputs.Count);
            return str;
        }
    }
}