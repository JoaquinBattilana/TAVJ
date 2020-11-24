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

        public void Add(float horizontal, float vertical) {
            ClientInput input = new ClientInput(_inputUidGenerator, horizontal, vertical);
            _inputUidGenerator++;
            _inputs.Add(input);
        }

        public void PredictLast(CharacterController controller) {
            
        }

        public void Serialize(BitBuffer buffer) {
            foreach(ClientInput input in _inputs) {
                input.Serialize(buffer);
            }
        }

        public void ExecuteInputs(CharacterController controller) {
            foreach(ClientInput input in _inputs) {
                input.Execute(controller);
                if(input.Number > _mostBigInput) {
                    _mostBigInput = input.Number;
                }
            }
            _inputs.Clear();
        }

        public void RemoveAckInputs(BitBuffer buffer) {
            _mostBigInput = buffer.GetInt();
            _inputs = _inputs.Where(input => input.Number > _mostBigInput).ToList();
        }

        public enum Type {
            MOVEMENT = 0
        }
    }
}