using System;
using Core.Coins;
using Core.Combat;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using Networking.Host;
using Unity.Collections;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [field: Header("References")]
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinWallet Wallet { get; private set; }
        
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private SpriteRenderer minimapIconRenderer;
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;
        [SerializeField] private Color ownerColor;
        
        public NetworkVariable<FixedString32Bytes> playerName = new();
        
        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawned;
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                var userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                playerName.Value = userData.userName;
                
                OnPlayerSpawned?.Invoke(this);
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
                
                minimapIconRenderer.color = ownerColor;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                OnPlayerDespawned?.Invoke(this);
            }
        }
    }
}