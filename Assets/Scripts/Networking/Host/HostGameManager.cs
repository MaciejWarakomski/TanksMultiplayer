using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

namespace Networking.Host
{
    public class HostGameManager
    {
        private Allocation _allocation;
        
        private string _joinCode;

        private const string GameSceneName = "Game";
        private const int MaxConnections = 20;
        
        public async Task StartHostAsync()
        {
            try
            {
                _allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                return;
            }
            
            try
            {
                _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(_joinCode);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                return;
            }

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(_allocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }
}