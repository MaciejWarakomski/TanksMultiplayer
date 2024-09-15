using UnityEngine;
using Core.Player;
using Unity.Netcode;
using System.Collections;

namespace Core.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            var players = FindObjectsOfType<TankPlayer>();
            foreach (var player in players)
            {
                HandlePlayerSpawned(player);
            }
            
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerSpawned += HandlePlayerDespawned;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerSpawned -= HandlePlayerDespawned;
        }
        
        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.Health.OnDie += _ => HandlePlayerDie(player);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.Health.OnDie -= _ => HandlePlayerDie(player);
        }

        private void HandlePlayerDie(TankPlayer player)
        {
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayer(player.OwnerClientId));
        }

        private IEnumerator RespawnPlayer(ulong ownerClientId)
        {
            yield return null;
            
            var playerInstance = Instantiate(
                playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
            playerInstance.SpawnAsPlayerObject(ownerClientId);
        }
    }
}