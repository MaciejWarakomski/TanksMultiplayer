﻿using System.Threading.Tasks;
using Unity.Services.Authentication;

namespace Networking.Client
{
    public static class AuthenticationWrapper
    {
        public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

        public static async Task<AuthState> DoAuth(int maxTries = 5)
        {
            if (AuthState == AuthState.Authenticated) return AuthState;

            AuthState = AuthState.Authenticating;
            var tries = 0;
            while (AuthState == AuthState.Authenticating && tries < maxTries)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
                
                tries++;
                await Task.Delay(1000);
            }
            return AuthState;
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