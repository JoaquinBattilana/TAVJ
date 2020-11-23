using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInput {
    private Vector2 _movement;
    private int _number;

    public ClientInput(int number, float horizontal, float vertical) {
        _number = number;
        _movement = new Vector2(horizontal, vertical);
    }

    public ClientInput(BitBuffer buffer) {
        _number = buffer.GetInt();
        _movement = new Vector2(buffer.GetFloat(), buffer.GetFloat());
    }

    public bool IsZero() {
        return _movement == Vector2.zero;
    }

    public override string ToString() {
        var returnString = "" + string.Format("movimiento = {0}", _movement.ToString());
        return returnString;
    }

    public void Serialize(BitBuffer buffer) {
        buffer.PutInt(_number);
        buffer.PutFloat(_movement.x);
        buffer.PutFloat(_movement.y);
    }

    public int Execute(CharacterController controller) {
        Vector3 move = new Vector3(_movement.x, 0, _movement.y);
        controller.Move(move * Time.deltaTime * 2f);
        return _number;
    }
}
