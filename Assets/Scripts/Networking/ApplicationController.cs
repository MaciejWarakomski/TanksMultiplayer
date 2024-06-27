using UnityEngine;
using Networking.Host;
using Networking.Client;
using System.Threading.Tasks;

namespace Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                
            }
            else
            {
                var hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost();
                
                var clientSingleton = Instantiate(clientPrefab);
                var authenticated = await clientSingleton.CreateClient();
                
                if (authenticated)
                {
                    clientSingleton.GameManager.GoToMenu();
                }
            }
        }
    }
}