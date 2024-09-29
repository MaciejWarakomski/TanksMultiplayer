using UnityEngine;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace UI.Lobby
{
    public class LobbiesList : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private Transform lobbyItemParent;
        [SerializeField] private LobbyItem lobbyItemPrefab;
        
        private bool _isRefreshing;

        private void OnEnable()
        {
            RefreshList();
        }

        public async void RefreshList()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                var options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0"),
                        new(
                            field: QueryFilter.FieldOptions.IsLocked,
                            op: QueryFilter.OpOptions.EQ,
                            value: "0")
                    }
                };

                var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

                foreach (Transform child in lobbyItemParent)
                {
                    Destroy(child.gameObject);
                }

                foreach (var lobby in lobbies.Results)
                {
                    var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                    lobbyItem.Initialize(this, lobby);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _isRefreshing = false;
        }

        public void JoinAsync(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            mainMenu.JoinAsync(lobby);
        }
    }
}