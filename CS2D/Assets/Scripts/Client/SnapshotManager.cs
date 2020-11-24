using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TAVJ {
    public class SnapshotManager {
        public Queue<Snapshot> _snapshotQueue;
        private int _snapshotQueueSize = 3;
        private float _promedyBetweenSnapshots = 0;
        private DateTime _lastTimestamp;
        private Snapshot _currentSnapshot = null;

        public SnapshotManager() {
            _snapshotQueue = new Queue<Snapshot>();
        }

        public void AddSnapshot(BitBuffer buffer) {
            Snapshot sp = new Snapshot(buffer);
            _snapshotQueue.Enqueue(sp);
            if(_snapshotQueue.Count > _snapshotQueueSize) {
                _currentSnapshot = _snapshotQueue.Dequeue();
            }
        }

        public void Interpolate(Dictionary<int, Player> players, float time, int _clientId) {
            if(_currentSnapshot != null) {
                foreach(var playerId in _currentSnapshot.GetClientsIds()) {
                    if(players.ContainsKey(playerId)) {
                        players[playerId].Interpolate(_currentSnapshot.GetClient(playerId), time/(1f/20f));
                    }
                }
            }
        }
    }
}