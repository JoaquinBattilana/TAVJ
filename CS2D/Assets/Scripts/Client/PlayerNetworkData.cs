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

    private Quaternion _headRotation;
    public Quaternion HeadRotation {
        get { return _headRotation; }
    }

    private int _mostBigInput;
    public int MostBigInput {
        get { return _mostBigInput; }
    }

    private int _health;
    public int Health {
        get { return _health; }
    }

    private int _points;
    public int Points {
        get { return _points; }
    }

    public PlayerNetworkData(BitBuffer buffer) {
        _position = new Vector3();
        _rotation = new Quaternion();
        _mostBigInput = buffer.GetInt();
        _health = buffer.GetInt();
        _points = buffer.GetInt();
        _position.x = buffer.GetFloat();
        _position.y = buffer.GetFloat();
        _position.z = buffer.GetFloat();
        _rotation.x = buffer.GetFloat();
        _rotation.y = buffer.GetFloat();
        _rotation.z = buffer.GetFloat();
        _rotation.w = buffer.GetFloat();
        _headRotation.x = buffer.GetFloat();
        _headRotation.y = buffer.GetFloat();
        _headRotation.z = buffer.GetFloat();
        _headRotation.w = buffer.GetFloat();
    }
}
