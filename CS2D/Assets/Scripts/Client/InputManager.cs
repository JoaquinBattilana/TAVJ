using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager {
    private List<ClientInput> _inputs;
    private int _inputsNumber = 0;

    public InputManager() {
        _inputs = new List<ClientInput>();
    }

    public Add(float horizontal, float vertical) {
        if(horizontal != 0 ||  vertical != 0) {
            ClientInput input = new ClientInput(_inputsNumber, horizontal, vertical);
            _inputsNumber++;
            _inputs.Add(input);
        }
    }

    public enum Type {
        MOVEMENT = 0
    }
}
