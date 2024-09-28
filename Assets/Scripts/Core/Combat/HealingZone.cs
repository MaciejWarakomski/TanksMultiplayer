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

    private NetworkVariable<int> HealPower = new();

    private float _remainingCooldown;
    private float _tickTimer;
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChange;
            HandleHealPowerChange(0, HealPower.Value);
        }

        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChange;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        _playersInZone.Add(player);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        
        if (!other.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;
        _playersInZone.Remove(player);
    }

    private void Update()
    {
        if (!IsServer) return;
        
        if (_remainingCooldown > 0)
        {
            _remainingCooldown -= Time.deltaTime;

            if (_remainingCooldown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }
        
        _tickTimer += Time.deltaTime;
        if (_tickTimer >=  1 / healTickRate)
        {
            foreach (var player in _playersInZone)
            {
                if (HealPower.Value == 0) break;
                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;
                if (player.Wallet.TotalCoins.Value < coinsPerTick) continue;
                player.Wallet.SpendCoins(coinsPerTick);
                player.Health.RestoreHealth(healthPerTick);
                HealPower.Value -= 1;

                if (HealPower.Value == 0)
                {
                    _remainingCooldown = healCooldown;
                }
            }

            _tickTimer = _tickTimer % (1 / healTickRate);
        }
    }

    private void HandleHealPowerChange(int oldHealPower, int newHealPower)
    {
        healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
    }
}
