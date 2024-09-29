using Input;
using Core.Coins;
using Core.Combat;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

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
        [SerializeField] private CoinWallet coinWallet;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;
        [SerializeField] private int costToFire;

        private float _muzzleFlashTimer;
        private float _timer;
        
        private bool _isPointerOverUI;
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

            if (!IsOwner) return;
            
            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
            }
            
            if (!_shouldFire || _timer > 0f || coinWallet.TotalCoins.Value < costToFire) return;
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
            _timer = 1 / fireRate;
        }
        
        private void HandlePrimaryFire(bool shoutFire)
        {
            if (shoutFire && _isPointerOverUI) return;
            
            _shouldFire = shoutFire;
        }
        
        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
        {
            if (coinWallet.TotalCoins.Value < costToFire) return;
            
            coinWallet.SpendCoins(costToFire);
            
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