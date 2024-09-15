using UnityEngine;
using Unity.Netcode;

namespace Core.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        public NetworkVariable<int> TotalCoins = new();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Coin>(out var coin)) return;
            
            var coinValue = coin.Collect();
            if (!IsServer) return;
            
            TotalCoins.Value += coinValue;
        }

        public void SpendCoins(int value)
        {
            TotalCoins.Value -= value;
        }
    }
}