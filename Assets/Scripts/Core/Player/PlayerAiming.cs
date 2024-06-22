using Input;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform turretTransform;

        private void LateUpdate()
        {
            if (!IsOwner) return;

            var aimWorldPosition = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
            turretTransform.up = new Vector2(
                aimWorldPosition.x - turretTransform.position.x,
                aimWorldPosition.y - turretTransform.position.y);
        }
    }
}