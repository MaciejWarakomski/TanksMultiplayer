using System;
using Core.Coins;
using Core.Combat;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using Networking.Host;
using Unity.Collections;
using UnityEngine.Serialization;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [field: Header("References")]
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinWallet Wallet { get; private set; }
        
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private SpriteRenderer minimapIconRenderer;
        [FormerlySerializedAs("cursorTexture")] [SerializeField] private Texture2D crosshair;
        
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
                
                Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2f, crosshair.height / 2f), CursorMode.Auto);
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