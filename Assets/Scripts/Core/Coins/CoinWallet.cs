using Core.Combat;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace Core.Coins
{
    public class CoinWallet : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private BountyCoin coinPrefab;

        [Header("Settings")] 
        [SerializeField] private float coinSpread = 3f;
        [SerializeField] private float bountyPercentage = 50f;
        [SerializeField] private int bountyCoinCount = 10;
        [SerializeField] private int minBountyCoinValue = 5;
        [SerializeField] private LayerMask layerMask;
        
        public NetworkVariable<int> totalCoins = new();
        
        private readonly Collider2D[] _coinBuffer = new Collider2D[1];
        private float _coinRadius;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            _coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

            health.OnDie += HandleDie;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            health.OnDie -= HandleDie;
        }

        private void HandleDie(Health dyingHealht)
        {
            var bountyValue = (int)(totalCoins.Value * (bountyPercentage / 100f));
            var bountyCoinValue = bountyValue / minBountyCoinValue;

            if (bountyCoinValue < minBountyCoinValue) return;

            for (var i = 0; i < bountyCoinCount; i++)
            {
                var coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
                coinInstance.SetValue(bountyCoinValue);
                coinInstance.NetworkObject.Spawn();
            }
        }
        
        private Vector2 GetSpawnPoint()
        {
            while (true)
            {
                var spawnPoint = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;
                var numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, _coinBuffer, layerMask);
                if (numColliders == 0)
                {
                    return spawnPoint;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Coin>(out var coin)) return;
            
            var coinValue = coin.Collect();
            if (!IsServer) return;
            
            totalCoins.Value += coinValue;
        }

        public void SpendCoins(int value)
        {
            totalCoins.Value -= value;
        }
    }
}