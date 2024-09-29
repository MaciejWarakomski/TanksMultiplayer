using Input;
using UnityEngine;
using Unity.Netcode;

namespace Core.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private ParticleSystem dustCloud;

        [Header("Settings")]
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float turningRate = 30f;
        [SerializeField] private float particleEmmisionValue = 10f;

        private ParticleSystem.EmissionModule _emissionModule;
        
        private Vector2 _previousMovementInput;
        private Vector3 _previousPos;

        private const float ParticleStopThreshold = 0.005f;
        
        private void Awake()
        {
            _emissionModule = dustCloud.emission;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            
            inputReader.MoveEvent += HandleMove;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            
            inputReader.MoveEvent -= HandleMove;
        }

        private void Update()
        {
            if (!IsOwner) return;

            var zRotation = _previousMovementInput.x * -turningRate * Time.deltaTime;
            bodyTransform.Rotate(0f, 0f, zRotation);
        }

        private void FixedUpdate()
        {
            if ((transform.position - _previousPos).sqrMagnitude > ParticleStopThreshold)
            {
                _emissionModule.rateOverTime = particleEmmisionValue;
            }
            else
            {
                _emissionModule.rateOverTime = 0f;
            }
                
            _previousPos = transform.position;
            
            if (!IsOwner) return;

            rb.velocity = (Vector2)bodyTransform.up * (_previousMovementInput.y * movementSpeed);
        }

        private void HandleMove(Vector2 movementInput) => _previousMovementInput = movementInput;
    }
}