using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotManager {
    public Queue<Snapshot> _snapshotQueue;
    private int _snapshotQueueSize = 3;
    private float _promedyBetweenSnapshots = 0;
    private DateTime _lastTimestamp;

    public SnapshotManager() {
        _snapshotQueue = new Queue<Snapshot>();
    }

    public void AddSnapshot(Bitbuffer buffer) {
        Snapshot sp = new Snapshot(buffer);
        _snapshotQueue.Enqueue(sp);
    }

    public void UpdatePositionBySnapshots(Dictionary<int, Player> players) {
        while (_snapshotQueue.Count >= _snapshotQueueSize) {
            Snapshot sp = _snapshotQueue.Dequeue();
            var keys = sp.GetClientsIds();
            foreach(var key in keys) {
                if(players.ContainsKey(key)) {
                    PlayerNetworkData data = sp.GetClient(key);
                    Player p = _players[key];
                    p.UpdatePosition(data);
                }
            }
        }
    }

    public void Interpolate(Dictionary<int, Player> players) {
        while(snapshotQueue.Count >= snapshotQueueSize) {
            Snapshot nextSnapshot = snapshotQueue.Peek();
            foreach(var playerId in nextSnapshot.GetClientsIds()) {
                if(players.ContainsKey(playerId)) {
                    players[playerId].Interpolate(nextSnapshot.GetClient(playerId), acumTime/(1f/pps));
                }
            }
        }
    }
}
