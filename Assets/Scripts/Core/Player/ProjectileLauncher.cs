﻿using Core.Combat;
using Input;
using UnityEngine;
using Unity.Netcode;

namespace Core.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Collider2D playerCollider;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;

        private float _muzzleFlashTimer;
        private float _previousFireTime;
        private bool _shouldFire;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            
            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }

        private void HandlePrimaryFire(bool shoutFire)
        {
            _shouldFire = shoutFire;
        }

        private void Update()
        {
            if (_muzzleFlashTimer > 0f)
            {
                _muzzleFlashTimer -= Time.deltaTime;
                if (_muzzleFlashTimer <= 0f)
                {
                    muzzleFlash.SetActive(false);
                }
            }
            
            if (!IsOwner || !_shouldFire || Time.time < 1 / fireRate + _previousFireTime) return;
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
            _previousFireTime = Time.time;
        }
        
        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
        {
            var projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);
            projectileInstance.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            if (projectileInstance.TryGetComponent<DealDamageOnContact>(out var dealDamage))
            {
                dealDamage.SetOwner(OwnerClientId);
            }
            
            if (projectileInstance.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
            
            SpawnDummyProjectileClientRpc(spawnPos, direction);
        }
        
        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
        {
            if (IsOwner) return;
            
            SpawnDummyProjectile(spawnPos, direction);
        }
        
        private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
        {
            muzzleFlash.SetActive(true);
            _muzzleFlashTimer = muzzleFlashDuration;
            
            var projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);
            projectileInstance.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            if (projectileInstance.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
        }
    }
}