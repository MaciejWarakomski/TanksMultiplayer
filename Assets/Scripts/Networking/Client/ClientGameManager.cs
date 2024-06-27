using Unity.Services.Core;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Networking.Client
{
    public class ClientGameManager
    {
        private const string MenuSceneName = "Menu";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();
            var authState = await AuthenticationWrapper.DoAuth();

            return authState == AuthState.Authenticated;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }
    }
}