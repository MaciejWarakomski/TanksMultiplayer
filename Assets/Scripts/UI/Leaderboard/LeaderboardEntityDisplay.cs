using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;

        [SerializeField] private Color myColour;

        private FixedString32Bytes _playerName;
        
        public ulong ClientId { get; private set; }
        public int Coins { get; private set; }
        
        public void Initialize(ulong clientId, FixedString32Bytes playerName, int coins)
        {
            ClientId = clientId;
            _playerName = playerName;

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                displayText.color = myColour;
            }
            
            UpdateCoins(coins);
        }

        public void UpdateCoins(int coins)
        {
            Coins = coins;
            
            UpdateText();
        }

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {_playerName.Value} ({Coins})";
        }
    }
}