using UnityEngine;
using Unity.Netcode;
using Networking.Host;
using Networking.Client;
using Networking.Server;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private ServerSingleton serverPrefab;
        [SerializeField] private NetworkObject playerPrefab;
        
        private ApplicationData _applicationData;
        
        private const string GameSceneName = "Game";
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
        }

        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                Application.targetFrameRate = 60;
                
                _applicationData = new ApplicationData();
                var serverSingleton = Instantiate(serverPrefab);

                StartCoroutine(LoadGameSceneAsync(serverSingleton));
            }
            else
            {
                var hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost(playerPrefab);
                
                var clientSingleton = Instantiate(clientPrefab);
                var authenticated = await clientSingleton.CreateClient();
                
                if (authenticated)
                {
                    clientSingleton.GameManager.GoToMenu();
                }
            }
        }

        private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(GameSceneName);

            while (!asyncOperation.isDone)
            {
                yield return null;                
            }

            var createServerTask = serverSingleton.CreateServer(playerPrefab);
            yield return new WaitUntil(() => createServerTask.IsCompleted);
            
            var startServerTask = serverSingleton.GameManager.StartGameServerAsync();
            yield return new WaitUntil(() => startServerTask.IsCompleted);
        }
    }
}