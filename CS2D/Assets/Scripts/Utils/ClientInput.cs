using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInput {
    private Vector3 _movement;
    private int _number;
    public int Number {
        get { return _number; }
    }

    public ClientInput(int number, float horizontal, float vertical) {
        _number = number;
        _movement = new Vector3(horizontal, 0,  vertical);
    }

    public ClientInput(BitBuffer buffer) {
        _number = buffer.GetInt();
        _movement = new Vector3(buffer.GetFloat(), 0,  buffer.GetFloat());
    }

    public bool IsZero() {
        return _movement == Vector3.zero;
    }

    public override string ToString() {
        var returnString = "" + string.Format("movimiento = {0}", _movement.ToString());
        return returnString;
    }

    public void Serialize(BitBuffer buffer) {
        buffer.PutInt(_number);
        buffer.PutFloat(_movement.x);
        buffer.PutFloat(_movement.z);
    }

    public void Execute(CharacterController controller) {
        controller.Move(_movement * Time.deltaTime * 2f);
    }
}
