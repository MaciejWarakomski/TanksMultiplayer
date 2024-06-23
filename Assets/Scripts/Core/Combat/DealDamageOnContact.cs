using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class DealDamageOnContact : MonoBehaviour
    {
        [SerializeField] private int damage = 5;

        private ulong _ownerClientID;

        public void SetOwner(ulong ownerClientID) => _ownerClientID = ownerClientID;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.attachedRigidbody) return;
            if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out var netObj))
            {
                if (_ownerClientID == netObj.OwnerClientId) return;
            }
            if (other.attachedRigidbody.TryGetComponent<Health>(out var otherHealth))
            {
                otherHealth.TakeDamage(damage);
            }
        }
    }
}