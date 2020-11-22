using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetworkData
{
    private Vector3 _position;
    public Vector3 Position {
        get { return _position; }
    }

    private Quaternion _rotation;
    public Quaternion Rotation {
        get { return _rotation; }
    }

    public PlayerNetworkData(BitBuffer buffer) {
        _position = new Vector3();
        _rotation = new Quaternion();
        _position.x = buffer.GetFloat();
        _position.y = buffer.GetFloat();
        _position.z = buffer.GetFloat();
        _rotation.x = buffer.GetFloat();
        _rotation.y = buffer.GetFloat();
        _rotation.z = buffer.GetFloat();
        _rotation.w = buffer.GetFloat();
    }
}
