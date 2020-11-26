using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TAVJ {
    public class UiManager {
        private Player _player;
        private GameObject _ui;
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _pointsText;
        public UiManager(Player player, GameObject userInterface) {
            _player = player;
            _ui = userInterface;
            if(_ui != null) {
                _healthText = _ui.transform.Find("healthText").gameObject.GetComponent<TextMeshProUGUI>();
                _pointsText = _ui.transform.Find("pointsText").gameObject.GetComponent<TextMeshProUGUI>();
            }
        }

        public void SetData(PlayerNetworkData data) {
            _healthText.text = string.Format("Vida: {0}", data.Health);
            _pointsText.text = string.Format("Puntos: {0}", data.Points);
        }
    }
}
    
