using TMPro;
using System;
using UnityEngine;
using Networking.Host;
using Networking.Client;
using Unity.Services.Lobbies;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;
        [SerializeField] private TMP_InputField joinCodeField;

        private float _timeInQueue;
        
        private bool _isMatchmaking;
        private bool _isCancelling;
        private bool _isBusy;

        private void Start()
        {
            if (!ClientSingleton.Instance) return;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        private void Update()
        {
            if (_isMatchmaking)
            {
                _timeInQueue += Time.deltaTime;
                var ts = TimeSpan.FromSeconds(_timeInQueue);
                queueTimerText.text = $"{ts.Minutes:00}:{ts.Seconds:00}";
            }
        }

        public async void FindMatchPressed()
        {
            if (_isCancelling) return;

            if (_isMatchmaking)
            {
                queueStatusText.text = "Canceling...";
                _isCancelling = true;
                await ClientSingleton.Instance.GameManager.CancelMatchmaking();
                _isCancelling = false;
                _isMatchmaking = false;
                _isBusy = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                queueTimerText.text = string.Empty;
                return;
            }

            if (_isBusy) return;
            
            ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Searching...";
            _timeInQueue = 0;
            _isMatchmaking = true;
            _isBusy = true;
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    queueStatusText.text = "Connecting...";
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    queueStatusText.text = "TicketCreationError";
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    queueStatusText.text = "TicketCancellationError";
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    queueStatusText.text = "TicketRetrievalError";
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    queueStatusText.text = "MatchAssignmentError";
                    break;
            }
        }

        public async void StartHost()
        {
            if (_isBusy) return;
            
            _isBusy = true;
            await HostSingleton.Instance.GameManager.StartHostAsync();
            _isBusy = false;
        }

        public async void StartClient()
        {
            if (_isBusy) return;
            
            _isBusy = true;
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
            _isBusy = false;
        }
        
        public async void JoinAsync(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            if (_isBusy) return;
            _isBusy = true;

            try
            {
                var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                var joinCode = joiningLobby.Data["JoinCode"].Value;

                await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _isBusy = false;
        }
    }
}