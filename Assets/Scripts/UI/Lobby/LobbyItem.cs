using TMPro;
using UnityEngine;

namespace UI.Lobby
{
    public class LobbyItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyPlayersText;

        private LobbiesList _lobbiesList;
        private Unity.Services.Lobbies.Models.Lobby _lobby;
        
        public void Initialize(LobbiesList lobbiesList, Unity.Services.Lobbies.Models.Lobby lobby)
        {
            _lobbiesList = lobbiesList;
            _lobby = lobby;
            lobbyNameText.text = lobby.Name;
            lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        public void Join()
        {
            _lobbiesList.JoinAsync(_lobby);
        }
    }
}