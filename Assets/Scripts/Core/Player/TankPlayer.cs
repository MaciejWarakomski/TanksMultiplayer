using Cinemachine;
using UnityEngine;
using Unity.Netcode;
using Networking.Host;
using Unity.Collections;

namespace Core.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        public NetworkVariable<FixedString32Bytes> playerName = new();
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                var userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                playerName.Value = userData.userName;
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
            }
        }
    }
}