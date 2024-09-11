﻿using System;
using UnityEngine;
using System.Threading.Tasks;

namespace Networking.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        private static ClientSingleton _instance;

        public static ClientSingleton Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindObjectOfType<ClientSingleton>();
                if (_instance) return _instance;
                
                Debug.LogError("No ClientSingleton in the scene!");
                return null;
            }
        }

        public ClientGameManager GameManager { get; private set; }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> CreateClient()
        {
            GameManager = new ClientGameManager();
            return await GameManager.InitAsync();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}