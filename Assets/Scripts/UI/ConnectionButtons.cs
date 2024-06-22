using UnityEngine;
using Unity.Netcode;

namespace UI
{
    public class ConnectionButtons : MonoBehaviour
    {
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
    
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}