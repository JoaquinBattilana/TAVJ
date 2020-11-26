using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Snapshot {
    private Dictionary <int, PlayerNetworkData> _playersData;

    public Snapshot(BitBuffer buffer) {
        _playersData = new Dictionary<int, PlayerNetworkData>();
        var playersLengths = buffer.GetInt();
        for(var i = 0; i < playersLengths; i++) {
            var playerId = buffer.GetInt();
            var networkData = new PlayerNetworkData(buffer);
            _playersData.Add(playerId, networkData);
        }
    }

    public List<int> GetClientsIds() {
        return _playersData.Keys.ToList();
    }

    public PlayerNetworkData GetClient(int id) {
        if (_playersData.ContainsKey(id)) {
            return _playersData[id];
        }
        return null;
    }

    public bool IsEmpty() {
        return _playersData.Count == 0;
    }

    public override string ToString() {
        var returnString = "";
        foreach(KeyValuePair<int, PlayerNetworkData> kvp in _playersData) {
            returnString += string.Format("Key = {0}, Value= {1}", kvp.Key, kvp.Value);
        }
        return returnString;
    }
}
