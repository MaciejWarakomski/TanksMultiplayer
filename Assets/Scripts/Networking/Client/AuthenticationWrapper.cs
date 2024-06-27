using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Networking.Client
{
    public static class AuthenticationWrapper
    {
        public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

        public static async Task<AuthState> DoAuth(int maxRetries = 5)
        {
            if (AuthState == AuthState.Authenticated) return AuthState;

            if (AuthState == AuthState.Authenticating)
            {
                Debug.LogWarning("Already authenticating!");
                await Authenticating();
            }
            else
            {
                await SignInAnonymouslyAsync(maxRetries);
            }

            return AuthState;
        }

        private static async Task Authenticating()
        {
            while (AuthState is AuthState.Authenticating or AuthState.NotAuthenticated)
            {
                await Task.Delay(200);
            }
        }

        private static async Task SignInAnonymouslyAsync(int maxRetries)
        {
            AuthState = AuthState.Authenticating;
            var retries = 0;
            while (AuthState == AuthState.Authenticating && retries < maxRetries)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                    {
                        AuthState = AuthState.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException authException)
                {
                    Debug.LogError(authException);
                    AuthState = AuthState.Error;
                }
                catch (RequestFailedException requestException)
                {
                    Debug.LogError(requestException);
                    AuthState = AuthState.Error;
                }

                retries++;
                await Task.Delay(1000);
            }

            if (AuthState != AuthState.Authenticated)
            {
                Debug.LogWarning($"Player was not signed in successfully after {retries} retries");
                AuthState = AuthState.TimeOut;
            }
        }
    }

    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        TimeOut,
        Error
    }
}