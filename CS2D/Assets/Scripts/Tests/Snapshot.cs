using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Snapshot
{
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
        return _playersData[id];
    }
}
