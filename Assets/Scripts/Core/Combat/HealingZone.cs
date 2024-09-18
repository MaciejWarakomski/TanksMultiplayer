using Core.Player;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image healPowerBar;

    [Header("Settings")]
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healthPerTick = 10;

    private readonly List<TankPlayer> _playersInZone = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        _playersInZone.Add(player);
        
        Debug.Log($"Entered: {player.playerName.Value}");
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        _playersInZone.Remove(player);
        Debug.Log($"Exited: {player.playerName.Value}");
    }
}
